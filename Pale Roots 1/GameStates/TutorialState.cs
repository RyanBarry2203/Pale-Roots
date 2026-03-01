using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Shows controls and a grid of all upgrade cards, and lets the player return to the previous screen.
    public class TutorialState : IGameState
    {
        private Game1 _game;

        // Hitbox for the back button at the bottom of the screen.
        private Rectangle _backBtnRect;

        public TutorialState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Position the back button centered at the bottom of the viewport.
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            _backBtnRect = new Rectangle(centerW - 100, _game.GraphicsDevice.Viewport.Height - 80, 200, 50);
        }

        public void Update(GameTime gameTime)
        {
            // Exit the tutorial on back button click or Escape key.
            if (InputEngine.IsMouseLeftClick() || InputEngine.IsKeyPressed(Keys.Escape))
            {
                MouseState ms = Mouse.GetState();

                // If Escape pressed or the click was inside the back button, pop this state.
                if (_backBtnRect.Contains(ms.Position) || InputEngine.IsKeyPressed(Keys.Escape))
                {
                    _game.StateManager.PopState();
                    InputEngine.ClearState();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // Draw the menu background if available.
            if (_game.MenuBackground != null)
            {
                spriteBatch.Draw(_game.MenuBackground, graphicsDevice.Viewport.Bounds, Color.White);
            }

            // Draw a dark overlay so the tutorial text and cards are readable.
            Texture2D pixel = _game.UiPixel;
            spriteBatch.Draw(pixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.85f);

            // Draw control instructions centered near the top of the screen.
            string instructions = "HOW TO PLAY\n\n\nUse W,A,S,D to move. Left Click to Light Attack.\nYou attack in the direction of your cursor\nDefeat enemies to level up and unlock the abilities below.\nSurvive the War and reclaim your throne.";
            Vector2 instSize = _game.UiFont.MeasureString(instructions);
            spriteBatch.DrawString(_game.UiFont, instructions, new Vector2(graphicsDevice.Viewport.Width / 2 - instSize.X / 2, 30), Color.White);

            // Get all upgrade cards except hidden boss trigger cards... because its an "easter egg"
            List<UpgradeManager.UpgradeOption> allCards = _game.UpgradeManager.GetAllUpgrades()
                .FindAll(card => card.Type != UpgradeManager.UpgradeType.BossTrigger);

            // Card layout and spacing values.
            int cardW = 220;
            int cardH = 320;
            int spacingX = 30;
            int spacingY = 40;

            // Grid configuration.
            int cols = 5;
            int startY = 300;

            Point mousePos = Mouse.GetState().Position;

            // Draw each upgrade card in a centered grid and pass hover state to the manager.
            for (int i = 0; i < allCards.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;

                int cardsInThisRow = (allCards.Count - (row * cols));
                if (cardsInThisRow > cols) cardsInThisRow = cols;

                int rowStartX = (graphicsDevice.Viewport.Width / 2) - (((cardsInThisRow * cardW) + ((cardsInThisRow - 1) * spacingX)) / 2);

                Rectangle cardRect = new Rectangle(rowStartX + (col * (cardW + spacingX)), startY + (row * (cardH + spacingY)), cardW, cardH);

                bool isHovered = cardRect.Contains(mousePos);
                _game.UpgradeManager.DrawCard(spriteBatch, cardRect, allCards[i], isHovered, _game.UiFont);
            }

            // Draw the back button and its border, highlighting when hovered.
            bool backHover = _backBtnRect.Contains(mousePos);
            spriteBatch.Draw(pixel, _backBtnRect, backHover ? Color.DarkRed : Color.Maroon);

            int b = 2;
            Color bc = backHover ? Color.White : Color.Black;
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Y, _backBtnRect.Width, b), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Bottom - b, _backBtnRect.Width, b), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Y, b, _backBtnRect.Height), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.Right - b, _backBtnRect.Y, b, _backBtnRect.Height), bc);

            // Center and draw the back button label...quite an ugly button but it gets the job done:(
            Vector2 backSize = _game.UiFont.MeasureString("BACK");
            spriteBatch.DrawString(_game.UiFont, "BACK", new Vector2(_backBtnRect.Center.X - backSize.X / 2, _backBtnRect.Center.Y - backSize.Y / 2), Color.White);

            spriteBatch.End();
        }
    }
}