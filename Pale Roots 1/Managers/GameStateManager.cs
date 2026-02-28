using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    // This is the master controller for the flow of the entire game.
    // By using a Stack (a Last-In, First-Out data structure), we can easily layer screens 
    // on top of each other—like opening a pause menu over the gameplay—and then remove them to resume the action.
    public class GameStateManager
    {
        // The core data structure holding our active screens. 
        // Whatever state is at the very top of this stack is what the player is currently interacting with.
        private Stack<IGameState> _stateStack = new Stack<IGameState>();

        // A quick helper property that lets other classes safely look at the top state 
        // without accidentally removing it from the stack.
        public IGameState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek() : null;

        public void ChangeState(IGameState newState)
        {
            // This is a hard reset for major transitions, like going from the Main Menu to a new Gameplay session.
            // We wipe out all previous states so they get garbage collected, push the new state, and initialize its assets.
            _stateStack.Clear();
            _stateStack.Push(newState);
            newState.LoadContent();
        }

        public void PushState(IGameState newState)
        {
            // This layers a new state directly on top of the current one without destroying it.
            // Because our Update method only ticks the top state, the underlying state is effectively "paused" in the background.
            _stateStack.Push(newState);
            newState.LoadContent();
        }

        public void PopState()
        {
            // This removes the top-most state, effectively closing it.
            if (_stateStack.Count > 0)
            {
                _stateStack.Pop();
            }

            // Whatever state was sitting underneath is now the CurrentState again.
            // Notice we do NOT call LoadContent() here, because that older state was just paused, not destroyed.
            // It still has all its textures and data perfectly intact.
        }

        public void Update(GameTime gameTime)
        {
            // We only ever run the logic for the state sitting at the very top of the stack.
            // This prevents a paused game from processing enemy AI or player movement.
            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // We only trigger the draw call for the top-most state.
            // If a top state wants to show the paused game behind it, like LevelUpState does, 
            // that specific state will hold a reference to the GameEngine and draw it manually.
            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Draw(gameTime, spriteBatch, graphicsDevice);
            }
        }
    }
}