using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Entities;
using RPG.GameObjects;
using RPG.Helpers;

namespace RPG.Tiles
{
    public class BoundingTriangle : BoundingRect
    {
        protected float slope;

        public BoundingTriangle(int x, int y, int width, int height, float slope) : base(x, y, width, height) {
            this.slope = slope;
        }

        public override bool isBasePriority(Rectangle rect) {
            return (rect.Right >= this.rect.Left && rect.Left <= this.rect.Left)
                    || (rect.Left <= this.rect.Right && rect.Right >= this.rect.Right);
        }

        public override void onStand(Entity e) {
            e.freeMoveForward = true;
        }

        public override BoundingRect shift(int x, int y) {
            return new BoundingTriangle(rect.X + x, rect.Y + y, Width, Height, slope);
        }

        public override int getLeft(Bounds b, Direction facing) {
            if (slope < 0 && b.Right <= rect.Left) {
                return rect.Left;
            } else if (b.Right >= rect.Left && b.Left <= rect.Right) {
                int top = getTop(b, facing);
                if (b.Bottom >= top + 1 || b.canTerrainSnap()) {
                    b.moveY(top - b.Bottom);
                }
            }
            
            return int.MaxValue;
        }

        public override int getRight(Bounds b, Direction facing) {
            if (slope > 0 && b.Left >= rect.Right) {
                return rect.Right;
            } else if (b.Right >= rect.Left && b.Left <= rect.Right) {
                int top = getTop(b, facing);
                if (b.Bottom >= top + 1 || b.canTerrainSnap()) {
                    b.moveY(top - b.Bottom);
                }
            }

            return int.MinValue;
        }

        public override int getTop(Bounds b, Direction facing) {
            int top;
            if (slope > 0) top = rect.Bottom - (int) ((b.Center.X - rect.Left) * slope);
            else top = rect.Top - (int) ((b.Center.X - rect.Left) * slope);

            // Bounds check
            if (top < rect.Top) {
                return rect.Top;
            } else if (top > rect.Bottom) {
                return rect.Bottom;
            } else {
                return top;
            }
        }

        public override int getBottom(Bounds b, Direction facing) {
            return rect.Bottom;
        }
    }
}
