using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Screen;

namespace RPG.Tiles
{
    public class TileBlockList : List<TileBlock>
    {
        public new void Add(TileBlock block) {
            this.Add(block, null);
        }

        public void Add(TileBlock block, Func<GameScreen, TileBlock, bool> blockEvent = null) {
            base.Add(block.Clone().addEvent(blockEvent));
        }
    }
}
