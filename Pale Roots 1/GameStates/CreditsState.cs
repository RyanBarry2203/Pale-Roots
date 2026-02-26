using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // This state handles the end-game scrolling credits sequence.
    // It implements IGameState so our StateManager can easily swap to it once the player beats the game.
    public class CreditsState : IGameState
    {
        private Game1 _game;

        // Tracks the vertical position of the entire block of text as it moves up the screen.
        private float _creditsScrollY;

        // hardcoded string of credits.
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
            // Start the text exactly at the bottom edge of the window so it scrolls up into view.
            _creditsScrollY = _game.GraphicsDevice.Viewport.Height;

            // Tell the AudioManager to switch off the combat music and play the relaxing credits theme.
            _game.AudioManager.HandleMusicState(GameState.Credits);
        }

        public void Update(GameTime gameTime)
        {
            // Move the text upwards at a steady rate of 60 pixels per second, 
            // using delta time so the speed is consistent regardless of framerate.
            _creditsScrollY -= 60f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // If the player presses Spacebar to skip, OR if the text has scrolled completely off the top of the screen...
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _creditsScrollY < -1500f)
            {
                // wipe the active gameplay session clean so the player doesn't load back into a finished arena.
                _game.HasStarted = false;
                _game.SoftResetGame();

                // Finally, push them back out to the main menu.
                _game.StateManager.ChangeState(new MenuState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();
            graphicsDevice.Clear(Color.Black);

            // Break our giant text block into an array of individual lines.
            string[] lines = _creditsText.Split('\n');

            // We use this local variable to track where to draw each specific line, 
            // starting from the master scroll position.
            float currentY = _creditsScrollY;
            float lineHeight = _game.UiFont.LineSpacing;

            // Loop through every single line in the array.
            foreach (string line in lines)
            {
                // Only bother doing the math and drawing if the line actually has text in it.
                if (!string.IsNullOrWhiteSpace(line))
                {
                    // Measure how wide the text is so we can calculate the exact center of the screen on the X axis.
                    Vector2 lineSize = _game.UiFont.MeasureString(line);
                    Vector2 linePos = new Vector2((graphicsDevice.Viewport.Width / 2) - (lineSize.X / 2), currentY);

                    spriteBatch.DrawString(_game.UiFont, line, linePos, Color.White);
                }

                // Nudge the Y position down by one line height before we loop and draw the next line.
                currentY += lineHeight;
            }

            spriteBatch.End();
        }
    }
}