using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.GameObjects;
using RPG.Screen;
using RPG.Helpers;
using RPG.Tiles;

namespace RPG.Entities
{
    public class EntityAIs
    {
        /*
        public static bool Skeleton_King(Entity e, TileMap map) {
            int r = ScreenManager.Rand.Next(500);
            if (r < 15) {
                e["run"] = false;
                e.setXSpeedPerMs(Math.Sign(e.getSpeedX()) * -Entity.SPEED_PER_MS);
            } else if (r < 150) {
                NearEntity nEntity = map.getNearestEntity(e, e.EBounds.Center);

                // Too close, run
                if (nEntity.Distance < AttackFactory.RAISE_DEATH_WIDTH * 3.75) {
                    e["run"] = true;
                    EntityAIs.Run(e, nEntity.Entity);

                // Right range, attack
                } else if (nEntity.Distance < AttackFactory.RAISE_DEATH_WIDTH * 7.25) {
                    e["run"] = false;
                    if (nEntity.Entity.EBounds.Center.X > e.EBounds.Center.X)
                        e.setXSpeedPerMs(0.05f);
                    else
                        e.setXSpeedPerMs(-0.05f);
                    e.attack(map, EntityPart.Body, AttackFactory.Raise_Death);
                }
            }

            return true;
        }

        public static bool Wraith(Entity e, TileMap map) {
            int r = ScreenManager.Rand.Next(500);
            if (r < 55) {
                Entity targ = EntityAIs.GetClosest(e, map);
                EntityAIs.Face(e, targ);
                e["head"] = (targ.State == EntityState.Blocking || targ.EBounds.BodyRect.Top > e.EBounds.BodyRect.Top);
            } else if (r < 80) {
                if (e["head"]) {
                    e.attack(map, EntityPart.Head, AttackFactory.Iceball);
                } else {
                    e.attack(map, EntityPart.Body, AttackFactory.Iceball);
                }
            } else if (r < 90) {
                e.setXSpeedPerMs(-e.getSpeedX());
            } else if (r < 100) {
                e.block();
            } else if (r < 110) {
                e.jump();
            } else if (r < 120) {
                e.duck();
            }

            return true;
        }

        public static bool Basic(Entity e, TileMap map) {
            int r = ScreenManager.Rand.Next(500);
            if (r < 10) {
                EntityAIs.Face(e, EntityAIs.GetClosest(e, map));
            } else if (r < 75) {
                if (r < 25) e.attack(map, EntityPart.Head, AttackFactory.Scurge_Shot);
                else if (r < 40) e.attack(map, EntityPart.Body, AttackFactory.Scurge_Shot);
                else e.attack(map, EntityPart.Legs, AttackFactory.Scurge_Shot);
            } else if (r < 85) {
                e.jump();
            } else if (r < 95) {
                e.duck();
            } else if (r < 105) {
                e.block();
            }

            return true;
        }
         */
    }
}
