using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RPG.Helpers
{
    class MathHelp
    {
        public static int getDistanceSq(Point p1, Point p2) {
            return Math.Abs((p2.X-p1.X)*(p2.X-p1.X) + (p2.Y-p1.Y)*(p2.Y-p1.Y));
        }

        public static float constrain(float num, float a, float b) {
            if (num < a) return a;
            else if (num > b) return b;
            else return num;
        }
    }
}
