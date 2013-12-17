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

using RPG.Sprites;
using RPG.GameObjects;
using RPG.Tiles;

namespace RPG.Screen
{
    public enum ScreenId { MainMenu, Game, Pause, Inventory, Popup, MainMenuHelp, Input };

    public class ScreenManager : Microsoft.Xna.Framework.Game
    {
        public static readonly TimeSpan TargElapsedTime = new TimeSpan(0, 0, 0, 0, 40);

        public static ScreenManager Instance;
        public static Random Rand = new Random();
        public static Texture2D WhiteRect;
        public static SpriteFont Font, Small_Font;
        public static KeyboardState oldKBState, kbState;
        public static Color AdditiveColor = new Color(.25f, .25f, .25f, .9f);

        private static Dictionary<ScreenId, Screen> screens;
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public ScreenManager() {
            Instance = this;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            TargetElapsedTime = TargElapsedTime;

            screens = new Dictionary<ScreenId, Screen>();

            // Set screen size
            graphics.PreferredBackBufferWidth = TileMap.SPRITE_SIZE * 20;
            graphics.PreferredBackBufferHeight = TileMap.SPRITE_SIZE * 5;
        }

        public static IEnumerable<Screen> screenIterator() {
            return screens.Values.AsEnumerable<Screen>();
        }

        public void addScreen(ScreenId id, Screen s, bool update = true, bool draw = true) {
            s.setUpdate(update);
            s.setDraw(draw);
            s.LoadContent();
            screens.Add(id, s);
        }

        protected override void Initialize() {
            kbState = oldKBState = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            WhiteRect = new Texture2D(graphics.GraphicsDevice, 1, 1);
            WhiteRect.SetData<Color>(new Color[] { Color.White });

            Font = Content.Load<SpriteFont>("Courier");
            Small_Font = Content.Load<SpriteFont>("Courier_Small");

            addScreen(ScreenId.MainMenu, new MainMenuScreen(this), true, true);
            addScreen(ScreenId.Input, new InputScreen(this), false, false);
            addScreen(ScreenId.Game, new GameScreen(this), false, false);
            addScreen(ScreenId.Pause, new PauseScreen(this), false, false);
            addScreen(ScreenId.MainMenuHelp, new MainMenuHelpScreen(this), false, false);
            addScreen(ScreenId.Inventory, new InventoryScreen(this), false, false);
        }

        protected override void UnloadContent() {
            spriteBatch.Dispose();
            WhiteRect.Dispose();

            foreach (Screen s in screens.Values)
                s.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            kbState = Keyboard.GetState();

            foreach (Screen s in screens.Values)
                if (s.doUpdate())
                    s.Update(gameTime);

            oldKBState = kbState;
            
            base.Update(gameTime);

            // Debug
            if (gameTime.IsRunningSlowly)
                Console.WriteLine("WARINING: Game running slow!");
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            foreach (Screen s in screens.Values)
                if (s.doDraw())
                    s.Draw(gameTime);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public ContentManager getContent() { return Content; }
        public GraphicsDeviceManager getGraphics() { return graphics; }
        public SpriteBatch getSpriteBatch() { return spriteBatch; }

        public static Screen getScreen(ScreenId id) { return screens[id]; }

        public int Width  { get { return graphics.PreferredBackBufferWidth; } }
        public int Height { get { return graphics.PreferredBackBufferHeight; } }
    }
}
