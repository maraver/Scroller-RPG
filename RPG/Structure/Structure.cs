using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Tiles;

namespace RPG.Structures
{
    public class Structure {
        private int startX = -1, startY = -1;
        private StructureId id;

        public TileBlock start(int x, int y, StructureId id) {
            this.startX = x;
            this.startY = y;
            this.id = id;

            return getTileBlockAt(x, y);
        }

        public TileBlock getTileBlockAt(int x, int y) {
            int relativeX = x - startX, relativeY = y - startY;
            return STRUCTURES[this.id][relativeY, relativeX];
        }

        public bool placeAt(int x, int y) {
            if (this.startX < 0 || this.startY < 0) {
                return false;
            } if (x > startX + STRUCTURES[this.id].GetLength(1)) {
                this.startX = this.startY = -1;
                return false;
            } else {
                int relativeX = x - startX, relativeY = y - startY;
                if (relativeX >= 0 && relativeX < STRUCTURES[this.id].GetLength(1) 
                        && relativeY >= 0 && relativeY < STRUCTURES[this.id].GetLength(0)) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        private static readonly Dictionary<StructureId, TileBlock[,]> STRUCTURES = new Dictionary<StructureId,TileBlock[,]>();
        static Structure() {
            STRUCTURES.Add(StructureId.Ramp, new TileBlock[,] {
                { TileBlock.STAIRS_UP, TileBlock.STONE_WALL, TileBlock.STAIRS_DOWN }
            });
        }
    }
}
