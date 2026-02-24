using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    public class GameStateManager
    {
        // We use a Stack now instead of just one variable
        private Stack<IGameState> _stateStack = new Stack<IGameState>();

        // The active state is always the one on top
        public IGameState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek() : null;

        public void ChangeState(IGameState newState)
        {
            // Clear everything and start fresh (Normal behavior for Menu -> Game)
            _stateStack.Clear();
            _stateStack.Push(newState);
            newState.LoadContent();
        }

        public void PushState(IGameState newState)
        {
            // Pause the current state (by simply not updating it anymore)
            // Add the new state on top
            _stateStack.Push(newState);
            newState.LoadContent();
        }

        public void PopState()
        {
            // Remove the top state (The Boss Fight)
            if (_stateStack.Count > 0)
            {
                _stateStack.Pop();
            }
            // The state below (Gameplay) is now the CurrentState again. 
            // We do NOT call LoadContent() because it's already loaded and paused!
        }

        public void Update(GameTime gameTime)
        {
            // Only update the top-most state
            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Only draw the top-most state
            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Draw(gameTime, spriteBatch, graphicsDevice);
            }
        }
    }
}