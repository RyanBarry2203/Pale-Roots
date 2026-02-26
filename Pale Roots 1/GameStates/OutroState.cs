using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // This state handles the final cinematic sequence that plays right after the player defeats the boss.
    // It works almost exactly like our IntroState, acting as a lightweight wrapper that lets the CutsceneManager do the heavy lifting.
    public class OutroState : IGameState
    {
        private Game1 _game;

        public OutroState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Tell the audio manager to fade into our specific ending music track.
            _game.AudioManager.HandleMusicState(GameState.Outro);

            // Queue up the "Outro" animation sequence so it's ready to draw on the first frame.
            _game.CutsceneManager.Play("Outro");
        }

        public void Update(GameTime gameTime)
        {
            _game.CutsceneManager.Update(gameTime);

            // Listen for the player pressing Spacebar to skip the cinematic, 
            // or simply check if the cutscene has reached its final frame natively.
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _game.CutsceneManager.IsFinished)
            {
                // Once the cinematic is done, swap this state out and roll the credits.
                _game.StateManager.ChangeState(new CreditsState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // Hand the drawing logic over to the cutscene manager, passing in the current screen dimensions 
            // so it knows exactly how to scale the images to fit the viewport.
            _game.CutsceneManager.Draw(spriteBatch, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            spriteBatch.End();
        }
    }
}