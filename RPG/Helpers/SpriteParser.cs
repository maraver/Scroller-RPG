using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPG.Helpers
{
    class SpriteParser
    {
        /**
         * Start spritebatch and return it
         */
        private static SpriteBatch StartSpriteBatch(Texture2D sprite, int size) {
            GraphicsDevice graphics = sprite.GraphicsDevice;
            RenderTarget2D targ = new RenderTarget2D(graphics, size, size);
            SpriteBatch spriteBatch = new SpriteBatch(graphics);

            graphics.SetRenderTarget(targ);
            graphics.Clear(Color.Transparent);

            spriteBatch.Begin();

            return spriteBatch;
        }

        /**
         * Stop spritebatch and return created texture
         */
        private static Texture2D StopSpriteBatch(SpriteBatch spriteBatch) {
            spriteBatch.End();

            Texture2D target = (Texture2D) spriteBatch.GraphicsDevice.GetRenderTargets()[0].RenderTarget;

            // Reset
            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            return target;
        }

        public static Texture2D ParseSpriteSheet(Texture2D ss, int idx, int size) {
            return SpriteParser.ParseSpriteSheet(ss, idx, 0, size);
        }

        public static Texture2D ParseSpriteSheet(Texture2D ss, int idxX, int idxY, int size) {
            SpriteBatch spriteBatch = StartSpriteBatch(ss, size);

            spriteBatch.Draw(ss, Vector2.Zero, new Rectangle(idxX * size, idxY * size, size, size), Color.White);
            
            return StopSpriteBatch(spriteBatch);
        }

        public static Texture2D Flip(Texture2D sprite) {            
            SpriteBatch spriteBatch = StartSpriteBatch(sprite, sprite.Width);

            Rectangle rect = new Rectangle(0, 0, sprite.Width, sprite.Height);
            spriteBatch.Draw(sprite, rect, rect, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            
            return StopSpriteBatch(spriteBatch);
        }
    }
}
