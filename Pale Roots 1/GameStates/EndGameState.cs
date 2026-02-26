using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // This state handles both the Victory and Game Over screens.
    // It takes a boolean flag in the constructor so it knows whether to congratulate the player or tell them they died.
    public class EndGameState : IGameState
    {
        private Game1 _game;
        private bool _isVictory;

        public EndGameState(Game1 game, bool isVictory)
        {
            _game = game;
            _isVictory = isVictory;
        }

        public void LoadContent()
        {
            // Talk to the AudioManager to swap the music track based on whether we won or lost.
            _game.AudioManager.HandleMusicState(_isVictory ? GameState.Victory : GameState.GameOver);
        }

        public void Update(GameTime gameTime)
        {
            // We need the mouse to be visible so the player can actually click the menu buttons.
            _game.IsMouseVisible = true;
            MouseState ms = Mouse.GetState();

            // Calculate the exact center of the screen to align our invisible hitboxes.
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            int centerH = _game.GraphicsDevice.Viewport.Height / 2;

            // Define the hitboxes for the "Play Again/Continue" and "Quit" buttons.
            // These need to perfectly match where the UIManager draws the actual text.
            Rectangle playAgainRect = new Rectangle(centerW - 100, centerH, 200, 50);
            Rectangle quitRect = new Rectangle(centerW - 100, centerH + 70, 200, 50);

            // Listen for a left click and check if the mouse is hovering over either button.
            if (InputEngine.IsMouseLeftClick())
            {
                if (playAgainRect.Contains(ms.Position))
                {
                    if (_isVictory)
                    {
                        // If the player won, push them forward into the final cinematic cutscene.
                        _game.StateManager.ChangeState(new OutroState(_game));
                    }
                    else
                    {
                        // If they died, wipe their progress, reset the level, and throw them right back into the action.
                        _game.SoftResetGame();
                        _game.HasStarted = true;
                        _game.AudioManager.Stop();
                        _game.StateManager.ChangeState(new GameplayState(_game));
                    }

                    // Clear the input state so this click doesn't accidentally trigger a weapon swing or jump in the next state.
                    InputEngine.ClearState();
                }
                else if (quitRect.Contains(ms.Position))
                {
                    // Hard close the entire application.
                    _game.Exit();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            //frozen game background with black tint
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Next, draw the actual End Screen UI (like the victory text and the buttons) directly on top of the screen.
            spriteBatch.Begin();
            _game.UIManager.DrawEndScreen(spriteBatch, graphicsDevice, _isVictory);
            spriteBatch.End();
        }
    }
}