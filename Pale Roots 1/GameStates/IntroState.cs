using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Plays the opening cinematic before gameplay begins.
    // Uses CutsceneManager to run and draw the cutscene.
    public class IntroState : IGameState
    {
        private Game1 _game;

        public IntroState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Start the intro music track.
            _game.AudioManager.HandleMusicState(GameState.Intro);

            // Start the "Intro" cutscene in the CutsceneManager.
            _game.CutsceneManager.Play("Intro");
        }

        public void Update(GameTime gameTime)
        {
            _game.CutsceneManager.Update(gameTime);

            // Skip to gameplay if the player presses Space or the cutscene finished.
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _game.CutsceneManager.IsFinished)
            {
                // Mark the game started and change to the GameplayState.
                _game.HasStarted = true;
                _game.StateManager.ChangeState(new GameplayState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // Draw the cutscene scaled to the current viewport size.
            _game.CutsceneManager.Draw(spriteBatch, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            spriteBatch.End();
        }
    }
}