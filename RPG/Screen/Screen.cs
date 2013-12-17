using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace RPG.Screen
{
    public abstract class Screen
    {
        protected ScreenManager screenManager;

        private bool draw, update;

        public SpriteBatch SpriteBatch { get { return screenManager.getSpriteBatch(); } }
        public GraphicsDeviceManager Graphics { get { return screenManager.getGraphics(); } }
        public ContentManager Content { get { return screenManager.getContent(); } }

        public Screen(ScreenManager screenManager) {
            this.screenManager = screenManager;
            setDraw(true);
            setUpdate(true);
        }

        public ScreenManager getScreenManager() { return screenManager; }

        public bool doDraw() { return this.draw; }

        public virtual void setDraw(bool val) {
            this.draw = val;
        }

        public bool doUpdate() { return this.update; }

        public virtual void setUpdate(bool val) {
            this.update = val;
            ScreenManager.oldKBState = ScreenManager.kbState; 
            ScreenManager.kbState = Keyboard.GetState();
        }

        public void setState(bool val) {
            this.setDraw(val);
            this.setUpdate(val);
        }

        public abstract void Update(GameTime gTime);
        public abstract void Draw(GameTime gTime);
        public abstract void LoadContent();
        public abstract void UnloadContent();
    }
}
