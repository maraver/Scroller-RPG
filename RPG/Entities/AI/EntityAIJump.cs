using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Entities.AI
{
    public class EntityAIJump : EntityAI {
        public EntityAIJump(Entity e) 
            : base(e, 1) 
        { }

        public override bool shouldRun() {
            return !Entity.moved && Entity.State == EntityState.Moving;
        }

        public override void init() {
            Entity.jump();
            Entity.setXSpeedPerMs((Entity.isFacingForward()?1:-1) * Entity.SPEED_PER_MS);
        }

        public override bool continueRunning() {
            return false;
        }
    }
}
