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
    class MainMenuHelpScreen : MenuScreen
    {
        public MainMenuHelpScreen(ScreenManager sm)
            : base(sm, new string[] { "Back" }, new Action[] { MenuItemFunctions.MainMenu }) {}

        public override void Draw(Microsoft.Xna.Framework.GameTime gTime) {
            SpriteBatch.DrawString(ScreenManager.Font, "Move  => Arrows", new Vector2(93, 23), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Space =>   Jump", new Vector2(93, 53), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Block =>     Up", new Vector2(93, 83), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Duck  =>   Down", new Vector2(93, 113), Color.White);

            SpriteBatch.DrawString(ScreenManager.Font, "I     <= Inventory", new Vector2(383, 23), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Q-E   <= Attack", new Vector2(383, 53), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "1-9   <= Select", new Vector2(383, 83), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Enter <= Interact", new Vector2(383, 113), Color.White);

            base.Draw(gTime);
        }
    }
}
