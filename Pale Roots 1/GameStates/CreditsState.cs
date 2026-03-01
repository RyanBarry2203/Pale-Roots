using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Displays the scrolling end-game credits and returns the player to the menu.
    public class CreditsState : IGameState
    {
        private Game1 _game;

        // Tracks the vertical position of the entire block of text as it moves up the screen.
        private float _creditsScrollY;

        // Hardcoded credits text displaying the work of a very talented developer.
        private string _creditsText =
            "PALE ROOTS\n\n" +
            "A Game by Ryan Barry\n\n" +
            "PROGRAMMING\nRyan Barry\n\n" +
            "ART ASSETS\nItch Artists\n\n" +
            "MUSIC\nRyan Barry\n\n" +
            "LEVEL DESIGN\nRyan Barry\n\n" +
            "UI DESIGN\nRyan Barry\n\n" +
            "GAME DESIGN\nRyan Barry\n\n" +
            "SPECIAL THANKS\nNeil Gannon\nPaul Powell\n\n\n" +
            "Thank you for playing!";

        public CreditsState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Position the credits text at the bottom of the viewport to start scrolling upward.
            _creditsScrollY = _game.GraphicsDevice.Viewport.Height;

            // Ask the audio manager to switch to the credits music.
            _game.AudioManager.HandleMusicState(GameState.Credits);
        }

        public void Update(GameTime gameTime)
        {
            // Move the credits up at a fixed speed using the elapsed time.
            _creditsScrollY -= 60f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // If the player skips or the credits finish, reset the game and return to the menu.
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _creditsScrollY < -1500f)
            {
                _game.HasStarted = false;
                _game.SoftResetGame();
                _game.StateManager.ChangeState(new MenuState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Prepare the sprite batch and clear the screen to black.
            spriteBatch.Begin();
            graphicsDevice.Clear(Color.Black);

            // Split the credits into lines for per-line measurement and drawing.
            string[] lines = _creditsText.Split('\n');

            // Start drawing at the master scroll Y and advance by the font line spacing.
            float currentY = _creditsScrollY;
            float lineHeight = _game.UiFont.LineSpacing;

            // Draw each non-empty line centered horizontally.
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Vector2 lineSize = _game.UiFont.MeasureString(line);
                    Vector2 linePos = new Vector2((graphicsDevice.Viewport.Width / 2) - (lineSize.X / 2), currentY);

                    spriteBatch.DrawString(_game.UiFont, line, linePos, Color.White);
                }

                // Move down to the next line position.
                currentY += lineHeight;
            }

            spriteBatch.End();
        }
    }
}