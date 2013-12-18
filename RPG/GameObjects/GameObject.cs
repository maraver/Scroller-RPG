using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Entities;
using RPG.Sprites;
using RPG.GameObjects;
using RPG.Screen;
using RPG.Tiles;
using RPG.Helpers;

namespace RPG.GameObjects
{
    [Serializable()]
    public class GameObject : ISerializable
    {
        public bool SlatedToRemove;

        protected GameScreen gameScreen;

        public Bounds Bounds { get; protected set; } // Top Left Tile
        public readonly Sprite Sprite;
        protected bool isOnFloor;

        public GameObject(GameScreen screen, Texture2D tex, int x, int y, int width=TileMap.SPRITE_SIZE/2, int height=TileMap.SPRITE_SIZE/2) 
            : this(screen, new Sprite(tex), x, y, width, height) { }
        
        public GameObject(GameScreen screen, Sprite s, int x, int y, int width=TileMap.SPRITE_SIZE, int height=TileMap.SPRITE_SIZE) {
            gameScreen = screen;
            Sprite = s;
            Bounds = new Bounds(x, y, width, height);
            isOnFloor = true;
        }

        public GameObject(SerializationInfo info, StreamingContext cntxt) {
            gameScreen = (GameScreen) ScreenManager.getScreen(ScreenId.Game);

            SlatedToRemove = (bool) info.GetValue("GameObject_SlatedToRemove", typeof(bool));
            Bounds = (Bounds) info.GetValue("GameObject_Bounds", typeof(Bounds));
            Sprite = GameScreen.sprEntities[(EntitySpriteId) info.GetValue("GameObject_EntitySpriteId", typeof(EntitySpriteId))].Clone();
            isOnFloor = (bool) info.GetValue("GameObject_IsOnFloor", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("GameObject_SlatedToRemove", SlatedToRemove);
            info.AddValue("GameObject_Bounds", Bounds);
            // TODO support multiple sprites
            info.AddValue("GameObject_EntitySpriteId", EntitySpriteId.Warrior);
            info.AddValue("GameObject_IsOnFloor", isOnFloor);
        }

        public virtual void update(TimeSpan elapsed) {
            if (SlatedToRemove) return;

            isOnFloor = Map.isRectOnFloor(Bounds, Direction.Stopped);

            if (!isOnFloor)
                Bounds.moveY(2);
        }

        public virtual void draw(SpriteBatch spriteBatch, Point offset, TimeSpan elapsed) {
            if (SlatedToRemove) return;

            Rectangle pRect = Bounds.Rect;
            pRect.Offset(offset);
            spriteBatch.Draw(Sprite.Base, pRect, Color.White);
        }
        
        public Vector2 Location { get { return Bounds.Location; } }
        public TileMap Map { get { return gameScreen.TileMap; } }
    }
}
