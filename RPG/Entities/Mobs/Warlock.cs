using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using RPG.Screen;
using RPG.Sprites;
using RPG.Items;
using RPG.Entities.AI;

namespace RPG.Entities.Mobs
{
    [Serializable]
    public class Warlock : Entity, ISerializable {

        public Warlock(GameScreen game, int x, int y, float scalar) 
            : base(game, game.SprEntity[EntitySpriteId.Warlock], x, y, (int) (600 * scalar), 0.9f * scalar, (int) (8 * scalar)) 
        { }

        protected override void entityInit() {
            this.Drops = new PossibleDrop[] {
                new PossibleDrop(GameScreen.Items[ItemId.SmallPotion], 0.05f),
                new PossibleDrop(GameScreen.Items[ItemId.BronzeHead], 0.03f),
                new PossibleDrop(GameScreen.Items[ItemId.BronzeLegs], 0.03f),
                new PossibleDrop(GameScreen.Items[ItemId.Gold], 0.25f)
            };

            this.AI = new EntityAIList(new EntityAI[] {
                new EntityAIJump(this)
            });
        }

        public override bool aiEnabled() {
            return true;
        }
    }
}
