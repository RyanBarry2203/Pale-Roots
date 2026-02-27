using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This state acts as an informational screen that can be stacked on top of the Menu.
    // It teaches the player the basic controls and dynamically lists all the possible upgrades they can find.
    public class TutorialState : IGameState
    {
        private Game1 _game;

        // The hitbox for the button that returns the player to the previous screen.
        private Rectangle _backBtnRect;

        public TutorialState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Calculate the bottom center of the screen so our back button is always perfectly anchored.
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            _backBtnRect = new Rectangle(centerW - 100, _game.GraphicsDevice.Viewport.Height - 80, 200, 50);
        }

        public void Update(GameTime gameTime)
        {
            // Listen for the player attempting to exit the tutorial, either by clicking or hitting Escape.
            if (InputEngine.IsMouseLeftClick() || InputEngine.IsKeyPressed(Keys.Escape))
            {
                MouseState ms = Mouse.GetState();

                // If they hit escape, OR if their mouse was inside the Back button when they clicked...
                if (_backBtnRect.Contains(ms.Position) || InputEngine.IsKeyPressed(Keys.Escape))
                {
                    // Pop this specific state off the StateManager's stack, automatically returning them to whatever screen they were on before.
                    _game.StateManager.PopState();
                    InputEngine.ClearState();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // 1. Draw the master menu background image
            if (_game.MenuBackground != null)
            {
                spriteBatch.Draw(_game.MenuBackground, graphicsDevice.Viewport.Bounds, Color.White);
            }

            // 2. Draw a dark, mostly opaque layer over the entire screen. 
            // This ensures the tutorial text and cards stand out clearly against the busy background art.
            Texture2D pixel = _game.UiPixel;
            spriteBatch.Draw(pixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.85f);

            // 3. Draw the hardcoded control instructions perfectly centered near the top of the screen.
            string instructions = "HOW TO PLAY\n\n\nUse W,A,S,D to move. Left Click to Light Attack.\nYou attack in the direction of your cursor\nDefeat enemies to level up and unlock the abilities below.\nSurvive the War and reclaim your throne.";
            Vector2 instSize = _game.UiFont.MeasureString(instructions);
            spriteBatch.DrawString(_game.UiFont, instructions, new Vector2(graphicsDevice.Viewport.Width / 2 - instSize.X / 2, 30), Color.White);

            // Ask the UpgradeManager for a master list of every single possible upgrade in the game,
            // but explicitly filter out the hidden 'BossTrigger' cards since those aren't real abilities.
            List<UpgradeManager.UpgradeOption> allCards = _game.UpgradeManager.GetAllUpgrades()
                .FindAll(card => card.Type != UpgradeManager.UpgradeType.BossTrigger);

            // Define the size and spacing for the grid of cards we are about to draw.
            int cardW = 220;
            int cardH = 320;
            int spacingX = 30;
            int spacingY = 40;

            // Define the layout of the grid.
            int cols = 5;
            int startY = 300;

            Point mousePos = Mouse.GetState().Position;

            // Loop through the entire list of available upgrades and draw them in a neatly centered grid.
            for (int i = 0; i < allCards.Count; i++)
            {
                // Calculate which specific row and column this current card belongs in based on its index.
                int row = i / cols;
                int col = i % cols;

                // Figure out exactly how many cards are sitting in this specific row.
                // If it's the last row and not completely full, we need this number so we can center those remaining cards.
                int cardsInThisRow = (allCards.Count - (row * cols));
                if (cardsInThisRow > cols) cardsInThisRow = cols;

                // Calculate the starting X position for this specific row so the entire block of cards is perfectly centered on the screen.
                int rowStartX = (graphicsDevice.Viewport.Width / 2) - (((cardsInThisRow * cardW) + ((cardsInThisRow - 1) * spacingX)) / 2);

                // Build the mathematical hitbox for where this specific card will be drawn.
                Rectangle cardRect = new Rectangle(rowStartX + (col * (cardW + spacingX)), startY + (row * (cardH + spacingY)), cardW, cardH);

                // Check if the player's mouse is currently hovering over this specific card, 
                // and pass that information to the UpgradeManager so it can draw a highlight effect.
                bool isHovered = cardRect.Contains(mousePos);
                _game.UpgradeManager.DrawCard(spriteBatch, cardRect, allCards[i], isHovered, _game.UiFont);
            }

            // 5. Draw the interactive Back Button at the bottom of the screen.
            bool backHover = _backBtnRect.Contains(mousePos);

            // Draw the main button body, changing color if the mouse is hovering over it.
            spriteBatch.Draw(pixel, _backBtnRect, backHover ? Color.DarkRed : Color.Maroon);

            // Draw a quick 2-pixel border around the button, turning it white if hovered.
            int b = 2;
            Color bc = backHover ? Color.White : Color.Black;
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Y, _backBtnRect.Width, b), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Bottom - b, _backBtnRect.Width, b), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Y, b, _backBtnRect.Height), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.Right - b, _backBtnRect.Y, b, _backBtnRect.Height), bc);

            // Finally, measure and center the text inside the button.
            Vector2 backSize = _game.UiFont.MeasureString("BACK");
            spriteBatch.DrawString(_game.UiFont, "BACK", new Vector2(_backBtnRect.Center.X - backSize.X / 2, _backBtnRect.Center.Y - backSize.Y / 2), Color.White);

            spriteBatch.End();
        }
    }
}