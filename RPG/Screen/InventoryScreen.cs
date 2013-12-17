using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using RPG.Entities;
using RPG.Items;
using RPG.Tiles;

namespace RPG.Screen
{
    class InventoryScreen : PopUpScreen {
        private Player player;
        private Texture2D background;
        private Item heldItem;
        private ItemBlock hoverOver;
        private ItemBlock[] itemBlocks;
        private MouseState oldMouse;

        private Rectangle helmRect, bodyRect, legsRect;
        public bool buttonPressed;

        public InventoryScreen(ScreenManager sm)
            : base(sm, "Status", "", new string[] { "Back" }, new Action[] { MenuItemFunctions.BackToGame })
        {
            buttonPressed = false;
            itemBlocks = new ItemBlock[] { 
                new ItemBlock(0), new ItemBlock(1), new ItemBlock(2), new ItemBlock(3), new ItemBlock(4), new ItemBlock(5), 
                new ItemBlock(6), new ItemBlock(7), new ItemBlock(8), new ItemBlock(9), new ItemBlock(10), new ItemBlock(11)
            };

            oldMouse = Mouse.GetState();

            helmRect = new Rectangle(100, 52, 155, 16);
            bodyRect = new Rectangle(100, 76, 155, 16);
            legsRect = new Rectangle(100, 100, 155, 16);
        }

        public override void LoadContent() {
            background = Content.Load<Texture2D>("GUI/Inventory");
            base.LoadContent();
        }

        public override void setDraw(bool val) {
            base.setDraw(val);
            if (val) player = ((GameScreen) ScreenManager.getScreen(ScreenId.Game)).Player; 
        }

        public override void setUpdate(bool val) {
            base.setUpdate(val);
            if (val) player = ((GameScreen) ScreenManager.getScreen(ScreenId.Game)).Player; 
        } 

        public void toggleDrawing() {
            this.setDraw(!this.doDraw());
        }

        public override void Update(GameTime gTime) {
            MouseState ms = Mouse.GetState();
            KeyboardState kb = ScreenManager.kbState;

            if (kb.IsKeyDown(Keys.I)) {
                buttonPressed = true;
            } else if (buttonPressed) {
                buttonPressed = false;
                toggleDrawing();

                // If paused draw this and not game
                ScreenManager.getScreen(ScreenId.Game).setUpdate(!this.doDraw());
                ScreenManager.getScreen(ScreenId.Pause).setUpdate(!this.doDraw());
            }

            // Stuff that only happens if open
            if (this.doDraw()) {
                hoverOver = null;
                Item oldHeldItem = heldItem;
                for (int idx = 0; idx < itemBlocks.Length; idx++) {
                    ItemBlock ib = itemBlocks[idx];
                    if (ib.IsOn(ms)) {
                        if (player.Alive) {
                            if (oldMouse.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released) {
                                if (heldItem == null) {
                                    heldItem = ib.Item;
                                    player.removeItem(ib.Item);
                                } else {
                                    // Pickup the old item
                                    heldItem = player.setItemAt(idx, heldItem);
                                }
                            } else if (oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                                ib.Action(player);
                            }
                        }
                        hoverOver = ib;
                        break;
                    }
                }

                // Holding an item and clicked not on slot
                if (heldItem != null && heldItem == oldHeldItem 
                        && oldMouse.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released) {
                    player.gameScreen.TileMap.dropItem(heldItem, player);
                    heldItem = null;
                }

                Point p = new Point(ms.X, ms.Y);
                if (helmRect.Contains(p)) {
                    if (player.Alive && oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                        player.addItem(player.equipment.Head.Item);
                        player.equipment.setHead(null);
                    } else if (player.equipment.Head != null) {
                        hoverOver = new ItemBlock(player.equipment.Head.Item);
                    }
                } else if (bodyRect.Contains(p)) {
                    if (player.Alive && oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                        player.addItem(player.equipment.Body.Item);
                        player.equipment.setBody(null);
                    } else if (player.equipment.Body != null) {
                        hoverOver = new ItemBlock(player.equipment.Body.Item);
                    }
                } else if (legsRect.Contains(p)) {
                    if (player.Alive && oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                        player.addItem(player.equipment.Legs.Item);
                        player.equipment.setLegs(null);
                    } else if (player.equipment.Legs != null) {
                        hoverOver = new ItemBlock(player.equipment.Legs.Item);
                    }
                }
            }

            oldMouse = ms;

            base.Update(gTime);
        }

        public override void Draw(GameTime gTime) {
            SpriteBatch.End(); // End normal, drawing different

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive); // Draw additive

            SpriteBatch.Draw(ScreenManager.WhiteRect, new Rectangle(0, 0,
                getScreenManager().GraphicsDevice.Viewport.Width, getScreenManager().GraphicsDevice.Viewport.Height), ScreenManager.AdditiveColor);

            SpriteBatch.End();

            // Draw the menu normal
            SpriteBatch.Begin();

            Rectangle bgRect = new Rectangle(85, 10, 460, 140);
            SpriteBatch.Draw(background, bgRect, Color.White);

            // Draw stats left
            Vector2 pos = new Vector2(110, 25);
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Hp: " + player.stats.Hp + " (" + (player.stats.HpPercent * 100).ToString("0") + "%)", pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, player.equipment.Head.ToString(14), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, player.equipment.Body.ToString(14), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, player.equipment.Legs.ToString(14), pos, Color.White);

            drawHover(100, 25, 160, 16, "Hit points");
            String head = player.equipment.Head.ToString(), body = player.equipment.Body.ToString(), legs = player.equipment.Legs.ToString();
            if (head != "") drawHover(100, 49, 160, 16, head);
            if (body != "") drawHover(100, 73, 160, 16, body);
            if (legs != "") drawHover(370, 97, 160, 16, legs);

            // Draw armour display
            Rectangle armourRect = new Rectangle(SpriteBatch.GraphicsDevice.Viewport.Width/2 - TileMap.SPRITE_SIZE/2, 
                    SpriteBatch.GraphicsDevice.Viewport.Height/2 - TileMap.SPRITE_SIZE/2, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE);
            SpriteBatch.Draw(player.Sprite.Base, armourRect, Color.White);
            if (player.equipment.Head.Stand != null) SpriteBatch.Draw(player.equipment.Head.Stand, armourRect, Color.White);
            if (player.equipment.Body.Stand != null) SpriteBatch.Draw(player.equipment.Body.Stand, armourRect, Color.White);
            if (player.equipment.Legs.Stand != null) SpriteBatch.Draw(player.equipment.Legs.Stand, armourRect, Color.White);

            for (int i=0; i<player.inventorySize(); i++) {
                Item item = player.getItemAt(i);
                itemBlocks[i].Item = item;
                if (item != null) {
                    drawItem(item, itemBlocks[i].Rect);
                }
            }

            // Draw menu items
            base.Draw(gTime, Color.White);

            // Draw held item
            if (heldItem != null) {
                drawItem(heldItem, new Rectangle(oldMouse.X - 12, oldMouse.Y - 12, 24, 24));
            }

            // Draw stats right
            pos = new Vector2(380, 25);
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Att Mult : " + player.stats.AttackPower.ToString("0.00"), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Head Mult: " + player.stats.THeadMultiplier.ToString("0.00"), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Body Mult: " + player.stats.TBodyMultiplier.ToString("0.00"), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Legs Mult: " + player.stats.TLegsMultiplier.ToString("0.00"), pos, Color.White);

            drawHover(370, 25, 160, 16, "Attack power multiplier");
            drawHover(370, 49, 160, 16, "Head damage multiplier");
            drawHover(370, 73, 160, 16, "Body damage multiplier");
            drawHover(370, 97, 160, 16, "Legs damage multiplier");

            // Draw hover over item name
            if (heldItem == null && hoverOver != null && hoverOver.Item != null) {
                string name = hoverOver.Item.ToString();
                Rectangle hoverRect = getHoverRect(oldMouse.X, oldMouse.Y, name);
                SpriteBatch.Draw(ScreenManager.WhiteRect, hoverRect, ScreenManager.AdditiveColor);
                SpriteBatch.DrawString(ScreenManager.Small_Font, name, new Vector2(hoverRect.X + 2, hoverRect.Y), Color.White);
            }
        }

        protected Rectangle getHoverRect(int x, int y, String text) {
            Vector2 strSize = ScreenManager.Small_Font.MeasureString(text);
            return new Rectangle((int) (x - strSize.X - 6), y, (int) strSize.X + 6, (int) strSize.Y);
        }

        private void drawHover(int x, int y, int width, int height, String text) {
            Rectangle area = new Rectangle(x, y, width, height);
            if (area.Contains(oldMouse.X, oldMouse.Y)) {
                Rectangle hoverRect = getHoverRect(oldMouse.X, oldMouse.Y, text);
                SpriteBatch.Draw(ScreenManager.WhiteRect, hoverRect, ScreenManager.AdditiveColor);

                Vector2 mouse = new Vector2(hoverRect.X, hoverRect.Y);
                SpriteBatch.DrawString(ScreenManager.Small_Font, text, mouse, Color.White);
            }
        }

        private void drawItem(Item item, Rectangle rect) {
            SpriteBatch.Draw(item.Sprite.Base, rect, Color.White);
            if (item.Stackable) {
                Vector2 txtSize = ScreenManager.Small_Font.MeasureString(item.Count.ToString());
                SpriteBatch.DrawString(ScreenManager.Small_Font, item.Count.ToString(), 
                        new Vector2(rect.Right - txtSize.X - 2, rect.Bottom - txtSize.Y + 2), Color.White);
            }
        }

        private class ItemBlock {
            public Item Item;
            public readonly Rectangle Rect;

            public ItemBlock(Item i) {
                Item = i;
                Rect = new Rectangle(0, 0, 0, 0);
            }

            public ItemBlock(int idx) {
                Item = null;
                Rect = new Rectangle(128 + (idx * 32), 122, 24, 24);
            }

            public bool IsOn(MouseState ms) {
                return (Rect.Width * Rect.Height > 0 && Rect.Contains(new Point(ms.X, ms.Y)));
            }

            public void Action(Player p) {
                if (Item != null && p != null) {
                    Item.UseFunc(p, Item);
                }
            }
        }
    }
}
