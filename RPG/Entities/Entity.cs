using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Entities.AI;
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

        public const float MAX_SPEED = 1;

        public static Color HIT_BOX_COLOR = Color.Lerp(Color.Red, Color.Transparent, 0.8f);

        // Entity States
        public readonly int XPValue;
        public string Name { get; protected set; }

        protected PossibleDrop[] Drops;
        protected EntityAIList AI;

        public readonly Equipment Equipment;
        public readonly EntityStats Stats;

        public readonly Dictionary<string, bool> Properties;

        // Display States
        public bool freeMoveForward;

        public EntityState State { get; private set; }
        public bool Moved { get; private set; }

        private Direction facing;
        private float speedMultiplier;
        protected int jumpDelay, attackDelay;

        protected EntityPart lastHitPart;
        protected int showHpTicks;

        protected TimeSpan tElapsed;
        protected Vector2 msVel;

        public Entity(SerializationInfo info, StreamingContext cntxt) : base(info, cntxt) {
            Name = (string) info.GetValue("Name", typeof(string));
            Equipment = (Equipment) info.GetValue("Equipment", typeof(Equipment));
            Stats = (EntityStats) info.GetValue("Stats", typeof(EntityStats));
            facing = (Direction) info.GetValue("Facing", typeof(Direction));
            Bounds = (EntityBounds) info.GetValue("EBounds", typeof(EntityBounds));
            State = (EntityState) info.GetValue("State", typeof(EntityState));
            Properties = (Dictionary<string, bool>) info.GetValue("Props", typeof(Dictionary<string, bool>));

            // Init entity in loaded classes
            Stats.setEntity(this);
            EBounds.setEntity(this);
            Equipment.setEntity(this);

            // Un-saved values
            commonEntityInit();
        }

        public Entity(GameScreen screen, Sprite s, int x, int y, int hp, float ap = 1, int xp = 0) : base(screen, s, x, y)  {
            XPValue = xp;
            facing = Direction.Right;
            State = EntityState.Standing;

            Name = RandomName.newName();
            Properties = new Dictionary<string,bool>();
            Bounds = new EntityBounds(this, x, y, (int) TileMap.SPRITE_SIZE, 12, 14, 6, 24);

            // Stats
            Equipment = new Equipment(this);
            Stats = new EntityStats(this, hp, ap);

            commonEntityInit();
        }

        private void commonEntityInit() {
            msVel = new Vector2();
            speedMultiplier = MAX_SPEED;
            jumpDelay = attackDelay = showHpTicks = 0;

            Drops = new PossibleDrop[0];

            // Due to a quirk in the code the frame count here must be one more than actual
            Sprite.setFrame(250, 3);
            if (!Sprite.hasSpriteParts(SpriteParts.Entity)) {
                throw new ArgumentException("The sprite passed is not an Entity sprite");
            }

            this.entityInit();
        }

        protected virtual void entityInit() { }

        public new void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            base.GetObjectData(info, cntxt);

            info.AddValue("Name", Name);
            info.AddValue("Equipment", Equipment);
            info.AddValue("Stats", Stats);
            info.AddValue("State", State);
            info.AddValue("Facing", facing);
            info.AddValue("EBounds", Bounds);
            info.AddValue("Props", Properties);
        }

        public virtual bool aiEnabled() {
            return false;
        }

        public override void update(TimeSpan elapsed) {
            if (SlatedToRemove) return;

            // ### Tick updates
            Moved = false;
            tElapsed = elapsed;
            freeMoveForward = false;

            if (attackDelay > 0)
                attackDelay -= elapsed.Milliseconds;
            if (showHpTicks > 0)
                showHpTicks -= elapsed.Milliseconds;

            // ### Update entity State
            isOnFloor = Map.isRectOnFloor(EBounds, facing, this);

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

            Moved = moveXWithVelocity(currXSpeed);

            // ### Update Y Position

            if (State == EntityState.Jumping) { // Gravity
                msVel.Y -= GRAVITY_PER_MS;
            } else if(jumpDelay > 0) {  // Tick jump delay
                jumpDelay -= elapsed.Milliseconds;
            }

            // Subtract so everything else doesn't have to be switched (0 is top)
            EBounds.moveY((int) -getRealYSpeed());
            if (Bounds.Top >= Map.getPixelHeight() - Bounds.Height / 2) {
                EBounds.moveY((int) getRealYSpeed()); // Undo the move
                fallWithGravity();
            } else if (Bounds.Bottom <= 0) {
                EBounds.moveY((int) getRealYSpeed()); // Undo the move
                hitGround();
            } else if (getRealYSpeed() > 0) {
                int newY = Map.checkBoundsYUp(Bounds, facing);
                if (newY != Bounds.Y) { // Hit something
                    EBounds.moveY(newY - Bounds.Y); // Move down correct amount (+)
                    fallWithGravity();
                }
            } else if (getRealYSpeed() < 0) {
                int newY = Map.checkBoundsYDown(Bounds, facing);
                if (newY != Bounds.Y) { // Hit something
                    EBounds.moveY(newY - Bounds.Y); // Move up correct amount (-)
                    hitGround();
                }
            }

            // ### Run the entities customizable AI
            if (aiEnabled()) AI.run();
        }

        // return True if move successful
        private Boolean moveXWithVelocity(int vel) {
            int oldX = EBounds.X;
            EBounds.moveX(vel);
            if (Bounds.Right > Map.getPixelWidth()) {
                EBounds.moveX((Map.getPixelWidth() - Bounds.Width) - Bounds.X);
            } else if (Bounds.Left <= 0) {
                EBounds.moveX(-Bounds.X);
            } else if (vel > 0) {
                int newX = Map.checkBoundsXRight(Bounds, facing);
                if (!freeMoveForward) updateBoundsX(newX);
            } else if (vel < 0) {
                int newX = Map.checkBoundsXLeft(Bounds, facing);
                if (!freeMoveForward) updateBoundsX(newX);
            }

            return (oldX != EBounds.X);
        }

        public bool shouldFall() {
            return !isOnFloor && State != EntityState.Jumping;
        }

        private void fallWithGravity() {
            msVel.Y = 0;
            setState(EntityState.Jumping);
        }

        private void updateBoundsX(int newX) {
            if (shouldFall()) {
                fallWithGravity();
            } else if (newX != Bounds.X) {
                EBounds.moveX(newX - Bounds.X);
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

        public Attack attack(EntityPart part) {
            return this.attack(part, AttackFactory.Fireball);
        }

        public Attack attack(EntityPart part, Func<Entity, EntityPart, TileMap, Attack> factoryFunc) {
            if (canAttack() && factoryFunc != null) {
                Attack a = factoryFunc(this, part, Map);
                if (a != null) {
                    Map.addAttack(a);
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
                Stats.addHp(amnt);
            }
        }

        // Return damage if hits wall
        public int slide(Direction dir) {
            if (dir == Direction.Stopped) {
                return 0;
            } else {
                int vel = (dir == Direction.Left ? -2 : 2);
                if (moveXWithVelocity(vel)) {
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
                realDmg = (int)(dmg * Stats.TLegsMultiplier);
            else if (part == EntityPart.Head)
                realDmg = (int)(dmg * Stats.THeadMultiplier);
            else if (part == EntityPart.Body)
                realDmg = (int) (dmg * Stats.TBodyMultiplier);
            realDmg = (int) (realDmg * reducer);

            Stats.addHp(-realDmg);
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
                Stats.headMultiplier -= 0.1f;
                Stats.legsMultiplier += 0.2f;
            }
        }

        public void block() {
            // Must wait a while after attacking to block again
            if (isOnFloor && attackDelay < ATTACK_DELAY_MS*0.8) {
                setState(EntityState.Blocking);
                EBounds.block(facing); // Resets position
                setXSpeedPerMs(0);
                Stats.bodyMultiplier += 0.5f;
                Stats.headMultiplier += 0.2f;
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
                Stats.resetReducers();
                this.State = State;
            }
        }

        protected void dropItems() {
            for (int i=0; i < Drops.Length; i++) {
                if (Drops[i].Chance > ScreenManager.Rand.NextDouble())
                    Map.dropItem(Drops[i].Item, this);
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

        protected Texture2D getSprite(int elapsed) {
            EntityState State = getDrawState();
            if (!Alive) { // State == EntityState.Dead || State == EntityState.Dying
                return Sprite.getSpritePart(SpriteParts.Part.Dead);
            } else if (State == EntityState.Crouching) {
                return Sprite.getSpritePart(SpriteParts.Part.Crouch);
            } else if (State == EntityState.Blocking) {
                return Sprite.getSpritePart(SpriteParts.Part.Block);
            } else if (State == EntityState.Moving) {
                Sprite.tick(elapsed);

                if (Sprite.FrameIdx == 0)
                    return Sprite.getSpritePart(SpriteParts.Part.Move);
                else
                    return Sprite.Base;
            } else if (State == EntityState.Attacking) {
                return Sprite.getSpritePart(SpriteParts.Part.Attack);
            } else if (State == EntityState.AttackCrouch) {
                return Sprite.getSpritePart(SpriteParts.Part.CrouchAttack);
            } else if (State == EntityState.Jumping) {
                return Sprite.getSpritePart(SpriteParts.Part.Move);
            } else {
                return Sprite.Base;
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
                hpRect.Width = (int) Math.Round(hpRect.Width * 0.75 * Stats.HpPercent) + 1;
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
        
        public EntityBounds EBounds {  get { return (EntityBounds) Bounds; }  }
        
        protected float getRealXSpeed() { return MathHelp.constrain(msVel.X * tElapsed.Milliseconds, -7, 7); }
        protected float getRealYSpeed() { return MathHelp.constrain(msVel.Y * tElapsed.Milliseconds, -7, 7); }
        public bool Alive { get { return Stats.Hp > 0; } }

        public float getSpeedX() { return msVel.X; }
        public float getSpeedY() { return msVel.Y; }
        public bool isFacingForward() { return facing != Direction.Left; }

        public bool isTryingToMove() { return msVel.X != 0; }

        public bool this[string s] {
            get { return (Properties.ContainsKey(s)) ? Properties[s] : false; }
            set { Properties[s] = value; }
        }
    }
}
