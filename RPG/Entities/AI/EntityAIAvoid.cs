using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Screen;
using RPG.Helpers;

namespace RPG.Entities.AI
{
    public class EntityAIAvoid : EntityAI {
        private readonly int MinDist, MaxDist;

        private int targetX;

        public EntityAIAvoid(Entity e, int min, int max) : base(e, 6) {
            MaxDist = (int) MathHelp.constrain(max, 1, e.Map.Width - 32);
            MinDist = (int) MathHelp.constrain(min, 0, MaxDist);

            targetX = -1;
        }

        public override bool shouldRun() {
            if (ScreenManager.Rand.NextDouble() < 0.2) {
                NearEntity targ = Entity.Map.getNearestEntity(Entity, Entity.EBounds.Center);

                // Too close, run
                if (targ.Distance < MinDist) {
                    targetX = (int) targ.Entity.Location.X;
                    if (targetX > Entity.EBounds.Center.X) {
                        targetX += ScreenManager.Rand.Next(MinDist - MaxDist) + MinDist;
                        if (targetX > Entity.Map.Width) targetX = Entity.Map.Width - MaxDist;

                        Entity.setXSpeedPerMs(0.05f);
                    } else {
                        targetX -= ScreenManager.Rand.Next(MinDist - MaxDist) + MinDist;
                        if (targetX < 0) targetX = MaxDist;

                        Entity.setXSpeedPerMs(-0.05f);
                    }

                    return true;
                }
            }

            return false;
        }

        public override bool continueRunning() {
            if (Entity.isFacingForward()) {
                return Entity.Location.X < targetX;
            } else {
                return Entity.Location.X > targetX;
            }
        }
    }
}
