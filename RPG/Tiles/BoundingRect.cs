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
    public class BoundingRect
    {
        protected Rectangle rect;

        public BoundingRect(int x, int y, int width, int height) {
            rect = new Rectangle(x, y, width, height);
        }

        public virtual bool isBasePriority(Rectangle rect) {
            return false;
        }

        public Rectangle getDrawRectangle() {
            return rect;
        }

        public bool collides(Rectangle rect) {
            if (this.Width == 0 || this.Height == 0 || rect.Width == 0 || rect.Height == 0)
                return false;

            return this.rect.Intersects(rect);
        }

        public virtual void onStand(Entity e) { }

        public virtual BoundingRect shift(int x, int y) {
            return new BoundingRect(rect.X + x, rect.Y + y, Width, Height);
        }

        public virtual int getLeft(Bounds b, Direction facing) {
            return rect.Left;
        }

        public virtual int getRight(Bounds b, Direction facing) {
            return rect.Right;
        }

        public virtual int getTop(Bounds b, Direction facing) {
            return rect.Top;
        }

        public virtual int getBottom(Bounds b, Direction facing) {
            return rect.Bottom;
        }

        public int Width { get { return rect.Width; } }
        public int Height { get { return rect.Height; } }
    }
}
