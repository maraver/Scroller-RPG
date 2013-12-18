using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Helpers
{
    class ToggleList<T> where T : class {
        private readonly int Length;

        private int activeIdx, inactiveIdx;
        private T[] active, inactive;

        public ToggleList() {
            this.Length = 0;
        }

        public ToggleList(T[] objs) {
            this.Length = objs.Length;
            this.inactive = (T[]) objs.Clone();
            this.active = new T[Length];
        }

        public void resetActive() {
            activeIdx = 0;
        }

        public T nextActive() {
            activeIdx += 1;
            while (activeIdx < Length && active[activeIdx] == null) {
                activeIdx += 1;
            }

            if (activeIdx < Length)
                return active[activeIdx];
            else
                return null;
        }

        public bool flipActive() {
            if (activeIdx < Length && active[activeIdx] != null) {
                inactive[activeIdx] = active[activeIdx];
                active[activeIdx] = null;

                return true;
            } else {
                return false;
            }
        }

        public void resetInactive() {
            inactiveIdx = 0;
        }

        public T nextInactive() {
            inactiveIdx += 1;
            while (inactiveIdx < Length && inactive[inactiveIdx] == null) {
                inactiveIdx += 1;
            }

            if (inactiveIdx < Length)
                return inactive[inactiveIdx];
            else
                return null;
        }

        public bool flipInactive() {
            if (inactiveIdx < Length && inactive[inactiveIdx] != null) {
                active[activeIdx] = inactive[inactiveIdx];
                inactive[inactiveIdx] = null;

                return true;
            } else {
                return false;
            }
        }
    }
}
