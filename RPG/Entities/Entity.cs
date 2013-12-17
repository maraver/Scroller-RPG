using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Sprites;
using RPG.Screen;
using RPG.Helpers;
using RPG.GameObjects;
using RPG.Items;
using RPG.Tiles;

namespace RPG.Entities
{
    public enum EntityState { Attacking, AttackCrouch, Standing, Jumping, Moving, Crouching, Blocking, Dying, Dead };

    [Serializable]
    public class Entity : GameObject, ISerializable {
        // Static
        public const float SPEED_PER_MS = 0.1f;
        public const float JUMP_PER_MS = 0.4f;
        public const float GRAVITY_PER_MS = 0.0675f;

        public const int JUMP_DELAY_MS = 300;
        public const int ATTACK_DELAY_MS = 600;
        public const int HP_BAR_SHOW_MS = 3000;

        public const float BASE_HEAD_MULT = 1.125f;
        public const float BASE_BODY_MULT = 1.0f;
        public const float BASE_LEGS_MULT = 0.75f;

        public static Color HIT_BOX_COLOR = Color.Lerp(Color.Red, Color.Transparent, 0.8f);

        public GameScreen gameScreen;

        // Entity States
        public readonly string Name;

        public readonly Equipment equipment;
        public readonly EntityStats stats;
        public readonly PossibleDrop[] drops;

        protected readonly Func<Entity, TileMap, bool> AI;

        protected readonly Dictionary<string, bool> props;

        public readonly int XP_VALUE;
        private readonly float MAX_SPEED;

        // Display States
        private Direction facing;
        public bool freeMoveForward;

        public EntityState State { get; private set; }
        public bool moved { get; private set; }

        private float speedMultiplier;
        protected int jumpDelay, attackDelay;

        protected EntityPart lastHitPart;
        protected int showHpTicks;

        protected TimeSpan tElapsed;
        protected Vector2 msVel;

        public Entity(SerializationInfo info, StreamingContext cntxt) : base(info, cntxt) {
            XP_VALUE = (int) info.GetValue("Entity_XPValue", typeof(int));
            MAX_SPEED = (float) info.GetValue("Entity_MaxSpeed", typeof(float));

            AI = (Func<Entity, TileMap, bool>) info.GetValue("Entity_AI", typeof(Func<Entity, TileMap, bool>));
            Name = (string) info.GetValue("Entity_Name", typeof(string));
            equipment = (Equipment) info.GetValue("Entity_Equipment", typeof(Equipment));
            stats = (EntityStats) info.GetValue("Entity_Stats", typeof(EntityStats));
            facing = (Direction) info.GetValue("Entity_Facing", typeof(Direction));
            msVel = (Vector2) info.GetValue("Entity_MsVel", typeof(Vector2));
            bounds = (EntityBounds) info.GetValue("Entity_EBounds", typeof(EntityBounds));
            State = (EntityState) info.GetValue("Entity_State", typeof(EntityState));
            props = (Dictionary<string, bool>) info.GetValue("Entity_Props", typeof(Dictionary<string, bool>));

            // Init entity in loaded classes
            stats.setEntity(this);
            EBounds.setEntity(this);
            equipment.setEntity(this);

            // Un-saved values
            gameScreen = (GameScreen) ScreenManager.getScreen(ScreenId.Game);
            lastHitPart = EntityPart.Body;
            jumpDelay = attackDelay = showHpTicks = 0;
            speedMultiplier = MAX_SPEED;
            drops = new PossibleDrop[0];
            sprite.setFrame(250, 3);
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            base.GetObjectData(info, cntxt);

            info.AddValue("Entity_AI", AI);
            info.AddValue("Entity_Name", Name);
            info.AddValue("Entity_Equipment", equipment);
            info.AddValue("Entity_Stats", stats);
            info.AddValue("Entity_XPValue", XP_VALUE);
            info.AddValue("Entity_Facing", facing);
            info.AddValue("Entity_MaxSpeed", MAX_SPEED);
            info.AddValue("Entity_MsVel", msVel);
            info.AddValue("Entity_State", State);
            info.AddValue("Entity_EBounds", bounds);
            info.AddValue("Entity_Props", props);
        }

        public Entity(GameScreen screen, int x, int y, Sprite s, Func<Entity, TileMap, bool> ai, 
            int hp=750, double ap=1, int xp=6, float speed=1, PossibleDrop[] pDrops=null, String name=null) : base(s, x, y) 
        {
            MAX_SPEED = speedMultiplier = speed;
            XP_VALUE = xp;

            AI = ai;
            gameScreen = screen;
            Name = ((name == null) ? RandomName.newName() : name);
            bounds = new EntityBounds(this, x, y, (int) TileMap.SPRITE_SIZE, 12, 14, 6, 24);
            State = EntityState.Standing;
            msVel = new Vector2(0, 0);
            facing = Direction.Right;
            jumpDelay = attackDelay = showHpTicks = 0;
            props = new Dictionary<string,bool>();

            // Stats
            equipment = new Equipment(this);
            stats = new EntityStats(this, hp, (float) ap);

            // Initialize drops, if none given empty array
            this.drops = (pDrops == null) ? new PossibleDrop[0] : pDrops;

            // Due to a quirk in the code the frame count here must be one more than actual
            sprite.setFrame(250, 3);
            if (!sprite.hasSpriteParts(SpriteParts.Entity))
                throw new ArgumentException("The sprite passed is not an Entity sprite");
        }

        protected virtual void runAI(TileMap map) {
            this.AI(this, map);
        }

        public override void update(TileMap map, TimeSpan elapsed) {
            if (SlatedToRemove) return;

            // ### Tick updates
            moved = false;
            tElapsed = elapsed;
            freeMoveForward = false;

            if (attackDelay > 0)
                attackDelay -= elapsed.Milliseconds;
            if (showHpTicks > 0)
                showHpTicks -= elapsed.Milliseconds;

            // ### Update entity State
            isOnFloor = map.isRectOnFloor(EBounds, facing, this);

            if (!Alive) {
                if (State == EntityState.Dead)
                    return;
                else if (State != EntityState.Dying)
                    die();
                else if (!isOnFloor)
                    EBounds.moveY(2);
                else {
                    setState(EntityState.Dead);
                    dropItems();
                }

                return;
            }

            // ### Update movement State based on movement
            if (msVel.X != 0 && State != EntityState.Jumping) {
                setState(EntityState.Moving);
                if (msVel.X < 0) facing = Direction.Left;
                else facing = Direction.Right;
            } else if (State == EntityState.Moving) { // If State still 'Moving' but not moving, change State
                setState(EntityState.Standing);
            }

            // ### Update X position

            int currXSpeed = (int) (getRealXSpeed() * speedMultiplier);
            if (attackDelay > ATTACK_DELAY_MS * 0.625f && speedMultiplier > 0.25f) // If attacked recently while jumping, move slower
                speedMultiplier *= 0.93f;
            else if (speedMultiplier < MAX_SPEED)
                speedMultiplier += 0.033f;
            else
                speedMultiplier = MAX_SPEED; // Don't overshoot

            moved = moveXWithVelocity(map, currXSpeed);

            // ### Update Y Position

            if (State == EntityState.Jumping) { // Gravity
                msVel.Y -= GRAVITY_PER_MS;
            } else if(jumpDelay > 0) {  // Tick jump delay
                jumpDelay -= elapsed.Milliseconds;
            }

            // Subtract so everything else doesn't have to be switched (0 is top)
            EBounds.moveY((int) -getRealYSpeed());
            if (bounds.Top >= map.getPixelHeight() - bounds.Height / 2) {
                EBounds.moveY((int) getRealYSpeed()); // Undo the move
                fallWithGravity();
            } else if (bounds.Bottom <= 0) {
                EBounds.moveY((int) getRealYSpeed()); // Undo the move
                hitGround();
            } else if (getRealYSpeed() > 0) {
                int newY = map.checkBoundsYUp(bounds, facing);
                if (newY != bounds.Y) { // Hit something
                    EBounds.moveY(newY - bounds.Y); // Move down correct amount (+)
                    fallWithGravity();
                }
            } else if (getRealYSpeed() < 0) {
                int newY = map.checkBoundsYDown(bounds, facing);
                if (newY != bounds.Y) { // Hit something
                    EBounds.moveY(newY - bounds.Y); // Move up correct amount (-)
                    hitGround();
                }
            }

            // ### Run the entities customizable AI
            if (AI != null)
                AI(this, map);
            else
                runAI(map);
        }

        // return True if move successful
        private Boolean moveXWithVelocity(TileMap map, int vel) {
            int oldX = EBounds.X;
            EBounds.moveX(vel);
            if (bounds.Right > map.getPixelWidth()) {
                EBounds.moveX((map.getPixelWidth() - bounds.Width) - bounds.X);
            } else if (bounds.Left <= 0) {
                EBounds.moveX(-bounds.X);
            } else if (vel > 0) {
                int newX = map.checkBoundsXRight(bounds, facing);
                if (!freeMoveForward) updateBoundsX(map, newX);
            } else if (vel < 0) {
                int newX = map.checkBoundsXLeft(bounds, facing);
                if (!freeMoveForward) updateBoundsX(map, newX);
            }

            if (oldX != EBounds.X) {
                return true;
            } else {
                return false;
            }
        }

        private void fallWithGravity() {
            msVel.Y = 0;
            setState(EntityState.Jumping);
        }

        private void updateBoundsX(TileMap map, int newX) {
            if (!isOnFloor && State != EntityState.Jumping) {
                fallWithGravity();
            } else if (newX != bounds.X) {
                EBounds.moveX(newX - bounds.X);
                // msVel.X = 0;
            }
        }

        public void setXSpeedPerMs(float speedPerMs) {
            msVel.X = speedPerMs;

            if (msVel.X < 0) facing = Direction.Left;
            else if (msVel.X > 0) facing = Direction.Right;
        }

        protected void hitGround() {
            isOnFloor = true;
            msVel.Y = 0;
            setState(EntityState.Standing);
        }

        public Attack attack(TileMap map, EntityPart part, Func<Entity, EntityPart, TileMap, Attack> factoryFunc) {
            if (canAttack() && factoryFunc != null) {
                Attack a = factoryFunc(this, part, map);
                if (a != null) {
                    map.addAttack(a);
                    attackDelay = ATTACK_DELAY_MS;
                    return a;
                }
            }
            return null;
        }
        
        public bool canAttack() {
            return (attackDelay <= 0 && State != EntityState.Blocking && State != EntityState.Dead);
        }

        public void heal(int amnt) {
            if (Alive) {
                stats.addHp(amnt);
            }
        }

        // Return damage if hits wall
        public int slide(TileMap map, Direction dir) {
            if (dir == Direction.Stopped) {
                return 0;
            } else {
                int vel = (dir == Direction.Left ? -2 : 2);
                if (moveXWithVelocity(map, vel)) {
                    return 0;
                } else {
                    return 1;
                }
            }
        }

        public int hitInThe(EntityPart part, int dmg, float reducer) {
            showHpTicks = HP_BAR_SHOW_MS;
            lastHitPart = part;

            int realDmg = 0;
            if (part == EntityPart.Legs)
                realDmg = (int)(dmg * stats.TLegsMultiplier);
            else if (part == EntityPart.Head)
                realDmg = (int)(dmg * stats.THeadMultiplier);
            else if (part == EntityPart.Body)
                realDmg = (int) (dmg * stats.TBodyMultiplier);
            realDmg = (int) (realDmg * reducer);

            stats.addHp(-realDmg);
            return realDmg;
        }

        public void jump() {
            if (jumpDelay <= 0 && isOnFloor) {
                msVel.Y = JUMP_PER_MS;
                setState(EntityState.Jumping);
                jumpDelay = JUMP_DELAY_MS; // Won't start decreasing until no longer jumping
                isOnFloor = false;
            }
        }

        public void duck() {
            if (isOnFloor) {
                setState(EntityState.Crouching);
                EBounds.duck(); // Resets position
                setXSpeedPerMs(0);
                stats.headMultiplier -= 0.1f;
                stats.legsMultiplier += 0.2f;
            }
        }

        public void block() {
            // Must wait a while after attacking to block again
            if (isOnFloor && attackDelay < ATTACK_DELAY_MS*0.8) {
                setState(EntityState.Blocking);
                EBounds.block(facing); // Resets position
                setXSpeedPerMs(0);
                stats.bodyMultiplier += 0.5f;
                stats.headMultiplier += 0.2f;
            }
        }

        protected void die() {
            EBounds.die();
            msVel.X = msVel.Y = 0;
            State = EntityState.Dying;
        }

        protected void setState(EntityState State) {
            if (Alive || State == EntityState.Dying || State == EntityState.Dead) {
                EBounds.resetPositions();  // Resets position
                stats.resetReducers();
                this.State = State;
            }
        }

        protected void dropItems() {
            for (int i=0; i < drops.Length; i++) {
                if (drops[i].Chance > ScreenManager.Rand.NextDouble())
                    gameScreen.TileMap.dropItem(drops[i].Item, this);
            }
        }

        public EntityState getDrawState() {
            if (attackDelay > ATTACK_DELAY_MS*0.2) {
                if (State == EntityState.Standing || State == EntityState.Moving || State == EntityState.Jumping) 
                    return EntityState.Attacking;
                else if (State == EntityState.Crouching)
                    return EntityState.AttackCrouch;
            }
            
            return State; 
        }

        protected Texture2D getSprite(int elapsed)
        {
            EntityState State = getDrawState();
            if (!Alive) { // State == EntityState.Dead || State == EntityState.Dying
                return sprite.getSpritePart(SpriteParts.Part.Dead);
            } else if (State == EntityState.Crouching) {
                return sprite.getSpritePart(SpriteParts.Part.Crouch);
            } else if (State == EntityState.Blocking) {
                return sprite.getSpritePart(SpriteParts.Part.Block);
            } else if (State == EntityState.Moving) {
                sprite.tick(elapsed);

                if (sprite.FrameIdx == 0)
                    return sprite.getSpritePart(SpriteParts.Part.Move);
                else
                    return sprite.Base;
            } else if (State == EntityState.Attacking) {
                return sprite.getSpritePart(SpriteParts.Part.Attack);
            } else if (State == EntityState.AttackCrouch) {
                return sprite.getSpritePart(SpriteParts.Part.CrouchAttack);
            } else if (State == EntityState.Jumping) {
                return sprite.getSpritePart(SpriteParts.Part.Move);
            } else {
                return sprite.Base;
            }
        }

        public override void draw(SpriteBatch spriteBatch, Point offset, TimeSpan elapsed) {
            if (SlatedToRemove) return;
            
            Rectangle pRect = EBounds.Rect;
            pRect.Offset(offset);
            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward())
                spriteBatch.Draw(sprite, pRect, Color.White);
            else
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);

            // Was hit resently, show hp bar
            if (Alive && showHpTicks != 0) {
                Rectangle hpRect = EBounds.Rect;
                hpRect.Offset(offset);
                hpRect.X += (int) (hpRect.Width * 0.125); // Offset and a bit over from the absolute left side
                hpRect.Y -= 7;
                hpRect.Height = 3;
                hpRect.Width = (int) Math.Round(hpRect.Width * 0.75 * stats.HpPercent) + 1;
                spriteBatch.Draw(ScreenManager.WhiteRect, hpRect, Color.Red);

                Vector2 vect = ScreenManager.Small_Font.MeasureString(Name);
                spriteBatch.DrawString(ScreenManager.Small_Font, Name, new Vector2(pRect.Center.X - vect.X/2, hpRect.Y - 19), Color.White);

                Rectangle lastRect = EBounds.getRectFromPart(lastHitPart);
                if (showHpTicks > HP_BAR_SHOW_MS / 2 && lastRect.Width != 0) {     
                    lastRect.Offset(offset);
                    lastRect.X += (int) (lastRect.Height * 0.1);
                    lastRect.Width = (int) (pRect.Width * 0.8);
                    spriteBatch.Draw(ScreenManager.WhiteRect, lastRect, HIT_BOX_COLOR);
                }
            }
        }
        
        public EntityBounds EBounds { 
            get { return (EntityBounds) bounds; } 
        }
        
        protected float getRealXSpeed() { return MathHelp.constrain(msVel.X * tElapsed.Milliseconds, -7, 7); }
        protected float getRealYSpeed() { return MathHelp.constrain(msVel.Y * tElapsed.Milliseconds, -7, 7); }
        public bool Alive { get { return stats.Hp > 0; } }

        public float getSpeedX() { return msVel.X; }
        public float getSpeedY() { return msVel.Y; }
        public bool isFacingForward() { return facing != Direction.Left; }

        public bool this[string s] {
            get { return (props.ContainsKey(s)) ? props[s] : false; }
            set { props[s] = value; }
        }
    }
}
