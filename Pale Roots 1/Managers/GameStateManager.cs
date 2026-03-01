using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    public class GameStateManager
    {
        // Stack holds the active game states with the top being the current one.
        private Stack<IGameState> _stateStack = new Stack<IGameState>();

        // CurrentState returns the state on top of the stack or null if empty.
        public IGameState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek() : null;

        public void ChangeState(IGameState newState)
        {
            // Clear the stack and push the new state.
            _stateStack.Clear();
            _stateStack.Push(newState);
            newState.LoadContent();
        }

        public void PushState(IGameState newState)
        {
            // Push a new state on top and load its content.
            _stateStack.Push(newState);
            newState.LoadContent();
        }

        public void PopState()
        {
            // Remove the top state from the stack.
            if (_stateStack.Count > 0)
            {
                _stateStack.Pop();
            }
            // Do not reload the resumed state here because it was already loaded.
        }

        public void Update(GameTime gameTime)
        {
            // Update only the state on top of the stack.
            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw only the state on top of the stack.
            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Draw(gameTime, spriteBatch, graphicsDevice);
            }
        }
    }
}