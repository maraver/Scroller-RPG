using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Entities;
using RPG.Tiles;
using RPG.Helpers;

namespace RPG.GameObjects
{
    public class Attack
    {
        public readonly Entity Owner;

        int xp;
        int dmg;
        bool alive;
        Texture2D sprite;
        Bounds bounds;
        Rectangle drawRect;
        TimeSpan lastElapsed;
        float msSpeed;
        bool horizontal;
        int maxdist, distTraveled;

        public Attack(TileMap map, Entity owner, Texture2D sprite, Rectangle rect, int dmg, float msSpeed, int maxdist, bool horizontal=true) {
            Owner = owner;

            this.xp = 0;
            this.alive = true;
            this.sprite = sprite;
            this.drawRect = rect;
            this.msSpeed = msSpeed;
            this.horizontal = horizontal;
            this.dmg = dmg;

            if (horizontal) {
                if (msSpeed > 0) {
                    this.bounds = new Bounds(rect.X, rect.Y, rect.Width - (int) (rect.Width * 0.2), rect.Height);
                } else {
                    this.bounds = new Bounds(rect.X + (int) (rect.Width * 0.2), rect.Y, rect.Width - (int) (rect.Width * 0.2), rect.Height);
                }
            } else {
                if (msSpeed > 0) {
                    this.bounds = new Bounds(rect.X, rect.Y, rect.Width, rect.Height - (int) (rect.Height * 0.2));
                } else {
                    this.bounds = new Bounds(rect.X, rect.Y + (int) (rect.Height * 0.2), rect.Width, rect.Height - (int) (rect.Height * 0.2));
                }
            }
            this.maxdist = maxdist;
            this.distTraveled = 0;

            // Initial bounds text with all containing tiles
            update(map, new TimeSpan(0));
        }

        public void update(TileMap map, TimeSpan elapsed) {
            if (alive) {
                lastElapsed = elapsed;

                int vel = getRealSpeed();
                if (horizontal) {
                    drawRect.X += vel;
                    bounds.moveX(vel);
                } else {
                    drawRect.Y += vel;
                    bounds.moveY(vel);
                }

                distTraveled += Math.Abs(vel);

                if (distTraveled > maxdist || drawRect.Right < 0 || drawRect.Left >= map.getPixelWidth()) {
                    alive = false;
                } else {
                    // Test walls based on direction (left, right)
                    if (horizontal) {
                        int oldX = bounds.X, newX;
                        if (vel > 0) newX = map.checkBoundsXRight(bounds, Direction.Right);
                        else newX = map.checkBoundsXLeft(bounds, Direction.Left);

                        if (newX != oldX || bounds.Y != drawRect.Y) {
                            alive = false;
                            return;
                        }
                    } else {
                        int oldY = bounds.Y, newY;
                        if (vel > 0) newY = map.checkBoundsYDown(bounds, Direction.Right);
                        else newY = map.checkBoundsYUp(bounds, Direction.Right);

                        if (newY != oldY) {
                            alive = false;
                            return;
                        }
                    }

                    // Test collision with entites
                    foreach (Entity e in map.entityIterator()) {
                        if (!e.Alive) 
                            continue;

                        EntityHit eHit;
                        if (horizontal) {
                            eHit = e.EBounds.collide(new Point(bounds.Right, bounds.Center.Y));
                            // If not hit in front, check back
                            if (eHit.Part == EntityPart.None)
                                eHit = e.EBounds.collide(new Point(bounds.Left, bounds.Center.Y));
                        } else {
                            int y;
                            if (msSpeed > 0) y = bounds.Bottom;
                            else y = bounds.Top;

                            eHit = e.EBounds.collide(new Point(bounds.Left, y));
                            // If not hit on left, check right
                            if (eHit.Part == EntityPart.None)
                                eHit = e.EBounds.collide(new Point(bounds.Right, y));
                        }

                        if (eHit.Part != EntityPart.None) {
                            alive = false;
                            if (eHit.Part != EntityPart.Miss) {
                                float dmgReducer = 0;
                                if (eHit.PercFromCenter >= 0.75) {
                                    dmgReducer = 0.25f;
                                } else if (eHit.PercFromCenter >= 0) {
                                    dmgReducer = 1 - eHit.PercFromCenter;
                                }

                                int realDmg = e.hitInThe(eHit.Part, dmg, dmgReducer);
                                realDmg += e.slide(map, eHit.KnockBack);
                                
                                map.addHitText(e, realDmg);
                                if (!e.Alive)
                                    xp += e.XP_VALUE;
                            }
                            return;
                        }
                    }
                }
            }
        }

        public bool doFlipSprite() {
            return (horizontal && msSpeed < 0);
        }

        public Texture2D getSprite() {
            if (alive)
                return sprite;
            else
                return null;
        }

        protected int getRealSpeed() { return (int) MathHelp.constrain(msSpeed * lastElapsed.Milliseconds, -7, 7); }

        public int Damage { get { return dmg; } }
        public Rectangle Rectangle { get { return drawRect; } }
        public bool Alive { get { return alive; } }
        public bool HasXP { get { return xp > 0; } }

        public int getXP() {
            if (xp > 0) {
                xp--;
                return 1;
            } else {
                return 0;
            }
        }
    }
}
