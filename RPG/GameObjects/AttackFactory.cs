﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Sprites;
using RPG.Entities;
using RPG.Tiles;

namespace RPG.GameObjects
{
    public class AttackFactory
    {
        public static int RAISE_DEATH_WIDTH = 27;

        public static Func<Entity, EntityPart, TileMap, Attack> IdToAttackFactory(AttackSpriteId id) {
            switch (id) {
                case AttackSpriteId.Fireball:    return AttackFactory.Fireball;
                case AttackSpriteId.Iceball:     return AttackFactory.Iceball;
                case AttackSpriteId.Raise_Death: return AttackFactory.Raise_Death;
                case AttackSpriteId.Scurge_Shot: return AttackFactory.Scurge_Shot;
                default: return AttackFactory.None;
            }
        }

        public static Attack None(Entity e, EntityPart part, TileMap map) {
            return null;
        }

        public static Attack Fireball(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = 20, height = 8;

            // Changes based on entity state
            int x;
            int y = e.EBounds.getFireFrom(part);
            float speed = 0.175f;
            if (e.isFacingForward()) {
                x = e.EBounds.Right + 1;
            } else {
                speed *= -1;
                x = e.EBounds.Left - width - 1;
            }
            
            return new Attack(map, e, map.gameScreen.SprAttack[AttackSpriteId.Fireball],
                        new Rectangle(x, y - (height / 2), width, height), (int) (100 * e.Stats.AttackPower), speed, 200);
        }

        public static Attack Iceball(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = 20, height = 8;

            // Changes based on entity state
            int x;
            int y = e.EBounds.getFireFrom(part);
            float speed = 0.16f;
            if (e.isFacingForward()) {
                x = e.EBounds.Right + 1;
            } else {
                speed *= -1;
                x = e.EBounds.Left - width - 1;
            }
            
            return new Attack(map, e, map.gameScreen.SprAttack[AttackSpriteId.Iceball],
                        new Rectangle(x, y - (height / 2), width, height), (int) (120 * e.Stats.AttackPower), speed, 200);
        }

        public static Attack Scurge_Shot(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = 20, height = 8;

            // Changes based on entity state
            int x;
            int y = e.EBounds.getFireFrom(part);
            float speed = 0.15f;
            if (e.isFacingForward()) {
                x = e.EBounds.Right + 1;
            } else {
                speed *= -1;
                x = e.EBounds.Left - width - 1;
            }
            
            return new Attack(map, e, map.gameScreen.SprAttack[AttackSpriteId.Scurge_Shot], 
                        new Rectangle(x, y - (height / 2), width, height), (int) (80 * e.Stats.AttackPower), speed, 200);
        }

        public static Attack Raise_Death(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = AttackFactory.RAISE_DEATH_WIDTH, height = 8;

            // Changes based on entity state
            int x;
            int y = e.EBounds.Top - (height * 2);
            float speed = 0.175f;
            if (e.isFacingForward()) {
                x = e.EBounds.Right + (AttackFactory.RAISE_DEATH_WIDTH * 5);
            } else {
                x = e.EBounds.Left - (AttackFactory.RAISE_DEATH_WIDTH * 5);
            }
            
            return new Attack(map, e, map.gameScreen.SprAttack[AttackSpriteId.Raise_Death], 
                        new Rectangle(x, y - (height / 2), width, height), (int) (80 * e.Stats.AttackPower), speed, TileMap.SPRITE_SIZE, false);
        }
    }
}
