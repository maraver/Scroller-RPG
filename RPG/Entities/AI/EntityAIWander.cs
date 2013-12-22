using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Screen;

namespace RPG.Entities.AI
{
    class EntityAIWander : EntityAI {
        public EntityAIWander(Entity e) : base(e, 2) { }

        public override bool shouldRun() {
            double rnd = ScreenManager.Rand.NextDouble();
            if (rnd < 0.1) {
                Entity.setXSpeedPerMs(Entity.SPEED_PER_MS);
                return true;
            } else if (rnd < 0.2) {
                Entity.setXSpeedPerMs(-Entity.SPEED_PER_MS);
                return true;
            } else {
                return false;
            }
        }

        public override bool continueRunning() {
            return ScreenManager.Rand.NextDouble() < 0.5;
        }

        public override void reset() {
            Entity.setXSpeedPerMs(0);
        }
    }
}
