using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // This state handles the opening cinematic right before the actual gameplay begins.
    // It relies on the CutsceneManager to do the heavy lifting for the visuals and timing.
    public class IntroState : IGameState
    {
        private Game1 _game;

        public IntroState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Tell the audio manager to start playing the specific intro music track.
            _game.AudioManager.HandleMusicState(GameState.Intro);

            // Queue up the "Intro" animation sequence in our cutscene manager.
            _game.CutsceneManager.Play("Intro");
        }

        public void Update(GameTime gameTime)
        {
            _game.CutsceneManager.Update(gameTime);

            // Check if the player wants to skip the cinematic by hitting the Spacebar, 
            // or if the cutscene simply finished playing on its own.
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _game.CutsceneManager.IsFinished)
            {
                // Flag the game as officially started and transition straight into the main action.
                _game.HasStarted = true;
                _game.StateManager.ChangeState(new GameplayState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // Pass the screen dimensions down to the cutscene manager so it can scale and draw the cinematic properly.
            _game.CutsceneManager.Draw(spriteBatch, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            spriteBatch.End();
        }
    }
}