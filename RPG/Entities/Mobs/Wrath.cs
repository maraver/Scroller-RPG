using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using RPG.Screen;
using RPG.Sprites;
using RPG.Items;
using RPG.GameObjects;
using RPG.Entities.AI;

namespace RPG.Entities.Mobs
{
    [Serializable]
    public class Wrath : Entity, ISerializable {

        public Wrath(GameScreen game, int x, int y, float scalar) 
            : base(game, game.SprEntity[EntitySpriteId.Wraith], x, y, (int) (450 * scalar), 0.9f * scalar, (int) (8 * scalar)) 
        { }

        protected override void entityInit() {
            this.Drops = new PossibleDrop[] {
                new PossibleDrop(GameScreen.Items[ItemId.SmallPotion], 0.05f),
                new PossibleDrop(GameScreen.Items[ItemId.Key], 0.01f),
                new PossibleDrop(GameScreen.Items[ItemId.BronzeBody], 0.03f),
                new PossibleDrop(GameScreen.Items[ItemId.Gold], 0.25f)
            };

            this.AI = new EntityAIList(new EntityAI[] {
                new EntityAIJump(this),
                new EntityAIWander(this),
                new EntityAIAttack(this)
            });
        }

        public override bool aiEnabled() {
            return true;
        }

        public Attack attack(EntityPart part) {
            return this.attack(part, AttackFactory.Iceball);
        }
    }
}
