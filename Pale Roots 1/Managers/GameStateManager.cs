using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class GameStateManager
    {
        private IGameState _currentState;

        public void ChangeState(IGameState newState)
        {
            _currentState = newState;
            _currentState.LoadContent(); // Initialize the new state
        }

        public void Update(GameTime gameTime)
        {
            if (_currentState != null)
            {
                _currentState.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (_currentState != null)
            {
                _currentState.Draw(gameTime, spriteBatch, graphicsDevice);
            }
        }
    }
}