using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class TutorialState : IGameState
    {
        private Game1 _game;
        private Rectangle _backBtnRect;

        public TutorialState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            _backBtnRect = new Rectangle(centerW - 100, _game.GraphicsDevice.Viewport.Height - 80, 200, 50);
        }

        public void Update(GameTime gameTime)
        {
            if (InputEngine.IsMouseLeftClick() || InputEngine.IsKeyPressed(Keys.Escape))
            {
                MouseState ms = Mouse.GetState();
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

            // 1. Draw the actual Menu Background
            if (_game.MenuBackground != null)
            {
                spriteBatch.Draw(_game.MenuBackground, graphicsDevice.Viewport.Bounds, Color.White);
            }

            // 2. Draw a dark semi-transparent overlay so the cards pop
            Texture2D pixel = _game.UiPixel;
            spriteBatch.Draw(pixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.85f);

            // 3. Draw Instructions
            string instructions = "HOW TO PLAY\n\n\nUse W,A,S,D to move. Left Click to Light Attack.\nYou attack in the direction of your cursor\nDefeat enemies to level up and unlock the abilities below.\nSurvive the War and reclaim your throne.";
            Vector2 instSize = _game.UiFont.MeasureString(instructions);
            spriteBatch.DrawString(_game.UiFont, instructions, new Vector2(graphicsDevice.Viewport.Width / 2 - instSize.X / 2, 30), Color.White);

            List<UpgradeManager.UpgradeOption> allCards = _game.UpgradeManager.GetAllUpgrades()
                .FindAll(card => card.Type != UpgradeManager.UpgradeType.BossTrigger);

            int cardW = 220; // Increased width
            int cardH = 320; // Increased height
            int spacingX = 30;
            int spacingY = 40;

            int cols = 5; // 5 cards per row
            int startY = 180;

            Point mousePos = Mouse.GetState().Position;

            for (int i = 0; i < allCards.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;

                // Center the row dynamically based on how many cards are in it
                int cardsInThisRow = (allCards.Count - (row * cols));
                if (cardsInThisRow > cols) cardsInThisRow = cols;

                int rowStartX = (graphicsDevice.Viewport.Width / 2) - (((cardsInThisRow * cardW) + ((cardsInThisRow - 1) * spacingX)) / 2);

                Rectangle cardRect = new Rectangle(rowStartX + (col * (cardW + spacingX)), startY + (row * (cardH + spacingY)), cardW, cardH);

                bool isHovered = cardRect.Contains(mousePos);
                _game.UpgradeManager.DrawCard(spriteBatch, cardRect, allCards[i], isHovered, _game.UiFont);
            }

            // 5. Draw Back Button
            bool backHover = _backBtnRect.Contains(mousePos);
            spriteBatch.Draw(pixel, _backBtnRect, backHover ? Color.DarkRed : Color.Maroon);

            // Draw Button Border
            int b = 2;
            Color bc = backHover ? Color.White : Color.Black;
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Y, _backBtnRect.Width, b), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Bottom - b, _backBtnRect.Width, b), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.X, _backBtnRect.Y, b, _backBtnRect.Height), bc);
            spriteBatch.Draw(pixel, new Rectangle(_backBtnRect.Right - b, _backBtnRect.Y, b, _backBtnRect.Height), bc);

            Vector2 backSize = _game.UiFont.MeasureString("BACK");
            spriteBatch.DrawString(_game.UiFont, "BACK", new Vector2(_backBtnRect.Center.X - backSize.X / 2, _backBtnRect.Center.Y - backSize.Y / 2), Color.White);

            spriteBatch.End();
        }
    }
}