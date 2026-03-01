using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Plays the final cinematic after the boss is defeated using the CutsceneManager.
    public class OutroState : IGameState
    {
        private Game1 _game;

        public OutroState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Switch the music to the outro track.
            _game.AudioManager.HandleMusicState(GameState.Outro);

            // Start the "Outro" cutscene sequence.
            _game.CutsceneManager.Play("Outro");
        }

        public void Update(GameTime gameTime)
        {
            _game.CutsceneManager.Update(gameTime);

            // Skip the cinematic on Space or when the cutscene finishes.
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _game.CutsceneManager.IsFinished)
            {
                // After the cinematic ends, show the credits screen.
                _game.StateManager.ChangeState(new CreditsState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // Tell the CutsceneManager to draw the current frame using the viewport size.
            _game.CutsceneManager.Draw(spriteBatch, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            spriteBatch.End();
        }
    }
}