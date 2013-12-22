using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Helpers;

namespace RPG.Entities.AI
{
    public class EntityAIList {
        private ToggleList<EntityAI> AIs;

        public EntityAIList() : this(null) {}

        public EntityAIList(EntityAI[] ais) {
            if (ais != null) {
                this.AIs = new ToggleList<EntityAI>(ais);
            } else {
                AIs = new ToggleList<EntityAI>();
            }
        }

        public void run() {
            int runMask = 0;
            EntityAI next = null;

            // Continue active
            AIs.resetActive();
            next = AIs.nextActive();
            while(next != null) {
                if ((next.RunMask & runMask) == 0 && next.continueRunning()) {
                    next.run();
                    runMask |= next.RunMask;
                } else {
                    next.reset();
                    AIs.flipActive();
                }

                next = AIs.nextActive();
            }

            // Start inactive
            AIs.resetInactive();
            next = AIs.nextInactive();
            while(next != null) {
                if ((next.RunMask & runMask) == 0 && next.shouldRun()) {
                    next.init();
                    AIs.flipInactive();
                    runMask |= next.RunMask;
                }

                next = AIs.nextInactive();
            }
        }
    }
}
