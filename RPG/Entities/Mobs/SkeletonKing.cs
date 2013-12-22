using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using RPG.Screen;
using RPG.Sprites;
using RPG.Items;
using RPG.Entities.AI;
using RPG.Helpers;
using RPG.GameObjects;

namespace RPG.Entities.Mobs
{
    [Serializable]
    public class SkeletonKing : Entity, ISerializable {

        public SkeletonKing(GameScreen game, int x, int y, float scalar) 
            : base(game, game.SprEntity[EntitySpriteId.SkeletonKing], x, y, (int) (1200 * scalar), 1.5f * scalar, (int) (20 * scalar)) 
        { 
            this.Name = RandomName.newCoolName();
        }

        protected override void entityInit() {
            this.Drops = new PossibleDrop[] {
                    new PossibleDrop(new Item(GameScreen.Items[ItemId.IronHead], Name + "'s Helm"), 0.1f),
                    new PossibleDrop(new Item(GameScreen.Items[ItemId.IronBody], Name + "'s Platebody"), 0.1f),
                    new PossibleDrop(new Item(GameScreen.Items[ItemId.IronLegs], Name + "'s Legs"), 0.1f),
                    new PossibleDrop(GameScreen.Items[ItemId.SmallPotion], 0.33f),
                    new PossibleDrop(GameScreen.Items[ItemId.Gold], 0.5f)
            };

            this.AI = new EntityAIList(new EntityAI[] {
                new EntityAIJump(this),
                new EntityAIAvoid(this, AttackFactory.RAISE_DEATH_WIDTH * 4, AttackFactory.RAISE_DEATH_WIDTH * 6),
                new EntityAIAttack(this)
            });
        }

        public override bool aiEnabled() {
            return true;
        }

        public Attack attack(EntityPart part) {
            return this.attack(part, AttackFactory.Raise_Death);
        }
    }
}
