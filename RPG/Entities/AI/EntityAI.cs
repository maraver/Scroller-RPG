using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Entities.AI
{
    public abstract class EntityAI {
        public readonly Entity Entity;
        public readonly int RunMask;

        public EntityAI(Entity e, int runMask) {
            this.Entity = e;
            this.RunMask = runMask;
        }

        public abstract bool shouldRun();

        public virtual void init() { }

        public virtual void run() { }

        public virtual bool continueRunning() {
            return false;
        }

        public virtual void reset() { }
    }
}
