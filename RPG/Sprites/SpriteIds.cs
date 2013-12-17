using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Sprites
{	
    public enum Animation { RedSpiral };
    public enum EntitySpriteId { Warrior, Warlock, Wraith, Skeleton_King };
    public enum AttackSpriteId { None, Fireball, Iceball, Scurge_Shot, Raise_Death };
    public enum TerrainSpriteId { None, Stone_Wall, Stone2_Wall, Door, EmptyMagicWall, ClosedChest, OpenChest, IronDoor, Stairs, Stairs_Flip };
    public enum GUISpriteId { Blocking, Ducking, Standing };
}
