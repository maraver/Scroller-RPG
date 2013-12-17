using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Screen;
using RPG.Entities;
using RPG.Tiles;
using RPG.Sprites;

namespace RPG.GameObjects
{
    [Serializable]
    public class HotBar : ISerializable {
        private const int HEIGHT = 21, DELAY = 5000;

        private int selected;
        private int height = HEIGHT, timeDelay = DELAY;
        private AttackSpriteId[] spells;

        public HotBar() {
            selected = 0;
            spells = new AttackSpriteId[9];

            spells[0] = AttackSpriteId.Fireball;
        }

        public HotBar(SerializationInfo info, StreamingContext cntxt) : this() {
            this.selected = (int) info.GetByte("HotBar_Selected");
            this.spells = (AttackSpriteId[]) info.GetValue("HotBar_Spells", typeof(AttackSpriteId[]));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("HotBar_Selected", (byte) selected);
            info.AddValue("HotBar_Spells", spells);
        }

        public void update(TimeSpan tElapsed) {
            if (timeDelay > 0) {
                timeDelay -= tElapsed.Milliseconds;

                if (height < HEIGHT) {
                    height += 1;
                }
            } else if (height > -2) {
                height -= 1;
            }
        }

        public void draw(SpriteBatch spriteBatch) {
            // Draw background
            int left = ScreenManager.Instance.Width / 2 - spells.Length * ((HEIGHT + 2) / 2);
            Rectangle rect = new Rectangle(left, ScreenManager.Instance.Height - height - 2, spells.Length * (HEIGHT + 1) + 1, HEIGHT + 2);
            spriteBatch.Draw(ScreenManager.WhiteRect, rect, ScreenManager.AdditiveColor);

            // Draw spells
            for (int i = 0; i < spells.Length; i++) {
                int x = left + i * (HEIGHT + 1) + 1;
                int y = ScreenManager.Instance.Height - height - 1;
                Rectangle spellRect = new Rectangle(x, y, HEIGHT, HEIGHT);
                spriteBatch.Draw(ScreenManager.WhiteRect, spellRect, Color.LightGray);

                if (spells[i] != AttackSpriteId.None) {
                    spriteBatch.Draw(GameScreen.sprAttacks_Icons[spells[i]], spellRect, Color.White);
                }

                // Selected spell
                if (i == this.selected) {
                    spriteBatch.Draw(ScreenManager.WhiteRect, spellRect, new Color(100, 100, 100, 75));
                }
            }
        }

        public void select(int index) {
            if (index >= 0 && index < spells.Length) {
                this.timeDelay = DELAY;
                this.selected = index;
            }
        }

        public void setSpell(int index, AttackSpriteId spell) {
            if (index >= 0 && index < spells.Length) {
                spells[index] = spell;
            }
        }
        
        public Func<Entity, EntityPart, TileMap, Attack> getSelected() {
            return AttackFactory.IdToAttackFactory(spells[selected]);
        }
    }
}
