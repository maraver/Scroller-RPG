using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Screen;

namespace RPG.Entities.AI
{
    class EntityAIAttack : EntityAI {
        NearEntity target;

        public EntityAIAttack(Entity e) : base(e, 6) { }

        public override bool shouldRun() {
            return ScreenManager.Rand.NextDouble() < 0.2;
        }

        public override void init() {
            target = Entity.Map.getNearestEntity(Entity, Entity.Bounds.Center);
        }

        public override void run() {
            EntityPart part = (target.Entity.Location.Y > Entity.Location.Y) ? EntityPart.Head : EntityPart.Body;
            Entity.attack(part);

            if (target.Entity.Location.X < Entity.Location.X)
                Entity.setXSpeedPerMs(-0.05f);
            else
                Entity.setXSpeedPerMs(0.05f);
        }

        public override bool continueRunning() {
            return target != null && ScreenManager.Rand.NextDouble() < 0.5;
        }

        public override void reset() {
            Entity.setXSpeedPerMs(0);
        }
    }
}
