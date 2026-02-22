using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class OutroState : IGameState
    {
        private Game1 _game;

        public OutroState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            _game.AudioManager.HandleMusicState(GameState.Outro);
        }

        public void Update(GameTime gameTime)
        {
            _game.CutsceneManager.Update(gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || _game.CutsceneManager.IsFinished)
            {
                _game.StateManager.ChangeState(new CreditsState(_game));
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();
            _game.CutsceneManager.Draw(spriteBatch, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            spriteBatch.End();
        }
    }
}