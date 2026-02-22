using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class CreditsState : IGameState
    {
        private Game1 _game;
        private float _creditsScrollY;

        private string _creditsText =
            "PALE ROOTS\n\n" +
            "A Game by Ryan Barry\n\n" +
            "PROGRAMMING\nRyan Barry\n\n" +
            "ART ASSETS\nItch Artists\n\n" +
            "MUSIC\nRyan Barry\n\n" +
            "SPECIAL THANKS\nPaul Powell\nNeil Gannon\n\n\n" +
            "Thank you for playing!";

        public CreditsState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            _creditsScrollY = _game.GraphicsDevice.Viewport.Height;
            _game.AudioManager.HandleMusicState(GameState.Credits);
        }

        public void Update(GameTime gameTime)
        {
            _creditsScrollY -= 60f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _creditsScrollY < -1500f)
            {
                _game.HasStarted = false;
                _game.SoftResetGame();
                _game.StateManager.ChangeState(new MenuState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();
            graphicsDevice.Clear(Color.Black);

            string[] lines = _creditsText.Split('\n');
            float currentY = _creditsScrollY;
            float lineHeight = _game.UiFont.LineSpacing;

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Vector2 lineSize = _game.UiFont.MeasureString(line);
                    Vector2 linePos = new Vector2((graphicsDevice.Viewport.Width / 2) - (lineSize.X / 2), currentY);
                    spriteBatch.DrawString(_game.UiFont, line, linePos, Color.White);
                }
                currentY += lineHeight;
            }
            spriteBatch.End();
        }
    }
}