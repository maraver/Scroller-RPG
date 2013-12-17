﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    [Serializable]
    public class Player : Entity, ISerializable {
        public int RoomCount { get; private set; }
        int lvlXp = 33, xp;
        List<Attack> aliveSpells;
        Item[] items;
        HotBar hotbar;

        public Player(GameScreen screen, int x, int y, String name, Sprite s) : base(screen, x, y, s, null, 900, 1, 0, 1, null, name) {
            this.xp = 0;
            this.RoomCount = 0;

            hotbar = new HotBar();
            aliveSpells = new List<Attack>();
            items = new Item[this.inventorySize()];
        }

        public Player(SerializationInfo info, StreamingContext cntxt) : base (info, cntxt) {
            lvlXp = (int) info.GetValue("Player_Lvlxp", typeof(int));
            xp = (int) info.GetValue("Player_Xp", typeof(int));
            items = (Item[]) info.GetValue("Player_Items", typeof(Item[]));
            RoomCount = (int) info.GetValue("Player_RoomCount", typeof(int));
            hotbar = (HotBar) info.GetValue("Player_HotBar", typeof(HotBar));

            // Unsaved stuff
            aliveSpells = new List<Attack>();
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            base.GetObjectData(info, cntxt);

            info.AddValue("Player_Lvlxp", lvlXp);
            info.AddValue("Player_Xp", xp);
            info.AddValue("Player_Items", items);
            info.AddValue("Player_RoomCount", RoomCount);
            info.AddValue("Player_HotBar", hotbar);
        }

        public void addXP(int i) {
            xp += i;
        }

        private static bool AttackXpAdded(Attack a) { return (!a.Alive && !a.HasXP); }
        protected override void runAI(TileMap map) {
            foreach (Attack a in aliveSpells) {
                xp += a.getXP();
            }

            // Level up
            if (xp > lvlXp) {
                xp = 0;
                lvlXp = (int) (lvlXp * 1.5f);
                stats.levelUp();
            }

            aliveSpells.RemoveAll(new Predicate<Attack>(AttackXpAdded));
        }

        public override void draw(SpriteBatch spriteBatch, Point offset, TimeSpan elapsed) {
            // Draw hp bar
            Rectangle hpRect = new Rectangle(2, 2, 100, 16);
            hpRect.Width = (int) Math.Round(hpRect.Width * stats.HpPercent) + 1;
            spriteBatch.Draw(ScreenManager.WhiteRect, hpRect, Color.Green);

            // Draw level
            spriteBatch.DrawString(ScreenManager.Small_Font, "Lvl " + stats.Level + " " + Name, new Vector2(105, 0), Color.White);

            // Draw entity
            Rectangle pRect = EBounds.Rect;
            pRect.Offset(offset);
            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward()) {
                spriteBatch.Draw(sprite, pRect, Color.White);
            } else {
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }

            // Draw armour stats
            if (Alive) {
                // Draw armour
                equipment.draw(spriteBatch, offset);
           
                // Draw xp bar
                Rectangle xpBar = new Rectangle(0, spriteBatch.GraphicsDevice.Viewport.Height - 3,
                    (int) (xp / (float)lvlXp * spriteBatch.GraphicsDevice.Viewport.Width), 2);
                spriteBatch.Draw(ScreenManager.WhiteRect, xpBar, Color.LightBlue);

                Viewport vp = spriteBatch.GraphicsDevice.Viewport;
                Rectangle armourImg = new Rectangle(vp.Width - 45, 2, 32, 32);
                Texture2D img;
                switch (State) {
                    case EntityState.Blocking:
                        img = GameScreen.sprGUI[GUISpriteId.Blocking]; break;
                    case EntityState.Crouching:
                        img = GameScreen.sprGUI[GUISpriteId.Ducking]; break;
                    default:
                        img = GameScreen.sprGUI[GUISpriteId.Standing]; break;
                }
                if (isFacingForward())
                    spriteBatch.Draw(img, armourImg, Color.Black);
                else
                    spriteBatch.Draw(img, armourImg, img.Bounds, Color.Black, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                spriteBatch.DrawString(ScreenManager.Small_Font, stats.THeadMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 0), Color.White);
                spriteBatch.DrawString(ScreenManager.Small_Font, stats.TBodyMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 11), Color.White);
                spriteBatch.DrawString(ScreenManager.Small_Font, stats.TLegsMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 22), Color.White);
            
                // Draw hit box
                if (showHpTicks > HP_BAR_SHOW_MS / 2) {
                    Rectangle lastRect = EBounds.getRectFromPart(lastHitPart);
                    if (lastRect.Width != 0) {     
                        lastRect.Offset(offset);
                        lastRect.X += (int) (lastRect.Height * 0.1);
                        lastRect.Width = (int) (lastRect.Width * 0.8);
                        spriteBatch.Draw(ScreenManager.WhiteRect, lastRect, HIT_BOX_COLOR);
                    }
                }
            }
        }

        public int inventorySize() {
            return 12;
        }

        public Item getItemAt(int idx) {
            if (idx >= 0 && idx < items.Length)
                return items[idx];
            else
                return null;
        }

        // Puts item in slot, returns old item
        public Item setItemAt(int idx, Item item) {
            if (idx >= 0 && idx < items.Length) {
                Item oldItem = items[idx];
                items[idx] = item;
                return oldItem;
            } else {
                return item;
            }
        }

        public int getItemIndex(ItemId id) {
            for (int i=0; i<items.Length; i++) {
                if (items[i] != null && items[i].Id == id)
                    return i;
            }
            return -1;
        }

        public bool addItem(Item item) {
            if (item != null) {
                // Stackable and contains
                if (item.Stackable) {
                    for (int i=0; i < items.Length; i++) {
                        Item invItem = items[i];
                        if (invItem != null && invItem.Id == item.Id && invItem.Count < Item.MAX_STACK) {
                            Item overflowStack;
                            if (invItem.Copy) {
                                overflowStack = invItem.addToStack(item.Count);
                            } else {
                                // Make copy
                                items[i] = new Item(invItem);
                                overflowStack = items[i].addToStack(invItem.Count + item.Count - 1);
                            }
                            
                            // Add extra items to own stack
                            addItem(overflowStack);
                                
                            return true;
                         }
                    }
                }
                
                // Add item to first open slot
                for (int i=0; i < items.Length; i++) {
                    if (items[i] == null) {
                        items[i] = item;
                        return true;
                    }
                }
            }

            return false;
        }

        public Item removeItem() {
            return removeItem(0);
        }

        // Removes an item at slot 'idx', returns that item
        public Item removeItem(int idx) {
            if (idx >= 0 && idx < items.Length) {
                Item i = items[idx];
                items[idx] = null;
                return i;
            }
            return null;
        }

        public Item removeItem(Item item) {
            if (item == null) return null;

            // Find the index
            int idx = -1;
            for (int i=0; i<items.Length; i++) {
                if (items[i] == item) {
                    idx = i;
                    break;
                }
            }

            // Do any complex removal
            return removeItem(idx);
        }
        
        public void newMap(TileMap map) {
            RoomCount++;
            moveTo(new Vector2(0, TileMap.SPRITE_SIZE * map.getFloor()));
        }

        public void moveTo(Vector2 targ) {
            EBounds.moveX((int) targ.X - bounds.X);
            EBounds.moveY((int) targ.Y - bounds.Y);
        }

        public void doAttack(TileMap map, EntityPart part) {
            if (Alive) {
                Attack attack = base.attack(map, part, hotbar.getSelected());
                if (attack != null) {
                    aliveSpells.Add(attack);
                }
            }
        }

        public void doBlock() {
            if (Alive) base.block();
        }

        public void doDuck() {
            if (Alive) base.duck();
        }

        public void doJump() {
            if (Alive) base.jump();
        }

        public void doMove(Direction dir) {
            if (Alive) 
                base.setXSpeedPerMs(Entity.SPEED_PER_MS * (float) dir);
        }

        public void stand() {
            if (Alive) base.setState(EntityState.Standing);
        }

        public HotBar getHotBar() {
            return hotbar;
        }
    }
}
