using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Screen;
using RPG.Items;

namespace RPG.Tiles
{
    class TileBlockEvent
    {
        public static bool Nothing(GameScreen gs, TileBlock b) { return false; }

        public static bool NewMainRoom(GameScreen gs, TileBlock b) {
            if (gs.Player.Alive) {
                gs.Player.addXP(10);
                gs.Player.heal(100);
                gs.Player.newMainRoom();
            }
            return true;
        }

        public static bool NewTreasureRoom(GameScreen gameScreen, TileBlock b) {
            if (gameScreen.Player.Alive) {
                TileMap map = new TileMap(20, 6, gameScreen, gameScreen.TileMap);
                map.generate(MapType.Treasure);

                gameScreen.setRoom(map);
            }
            return true;
        }

        public static bool MapGoBack(GameScreen gs, TileBlock b) {
            if (gs.TileMap.OldMap != null) {
                gs.setRoom(gs.TileMap.OldMap);
                gs.Player.moveTo(gs.TileMap.LeavePoint);
            } else {
                gs.Player.newMainRoom();
            }
            return true;
        }

        public static bool WallHeal(GameScreen gs, TileBlock b) {
            if (gs.Player.Stats.HpPercent != 1) {
                gs.Player.heal((int) (gs.Player.Stats.MaxHp * 0.1));
                b.setTile(TileBlock.EMPTYMAGIC_WALL);
            }

            return true;
        }

        public static bool OpenChest(GameScreen gs, TileBlock b) {
            Item i = GameScreen.Items[ItemId.Gold].Clone();
            i.addToStack(ScreenManager.Rand.Next(6, 24));
            b.setTile(TileBlock.OPEN_CHEST);
            gs.TileMap.dropItem(i, gs.Player);
            return true;
        }
    }
}
