using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Shows the victory or game over screen based on a flag.
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
            // Switch the music track for victory or game over.
            _game.AudioManager.HandleMusicState(_isVictory ? GameState.Victory : GameState.GameOver);
        }

        public void Update(GameTime gameTime)
        {
            // Make the mouse visible so the player can click UI buttons.
            _game.IsMouseVisible = true;
            MouseState ms = Mouse.GetState();

            // Compute the screen center for button hitboxes.
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            int centerH = _game.GraphicsDevice.Viewport.Height / 2;

            // Define hitboxes that match the UIManager's button layout.
            Rectangle playAgainRect = new Rectangle(centerW - 100, centerH, 200, 50);
            Rectangle quitRect = new Rectangle(centerW - 100, centerH + 70, 200, 50);

            // Handle a left mouse click on the UI buttons.
            if (InputEngine.IsMouseLeftClick())
            {
                if (playAgainRect.Contains(ms.Position))
                {
                    if (_isVictory)
                    {
                        // Move to the outro cinematic when the player won.
                        _game.StateManager.ChangeState(new OutroState(_game));
                    }
                    else
                    {
                        // Reset the game and resume gameplay when the player died.
                        _game.SoftResetGame();
                        _game.HasStarted = true;
                        _game.AudioManager.Stop();
                        _game.StateManager.ChangeState(new GameplayState(_game));
                    }

                    // Clear input so the click does not affect the next state.
                    InputEngine.ClearState();
                }
                else if (quitRect.Contains(ms.Position))
                {
                    // Exit the application.
                    _game.Exit();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw the frozen game world in the background using the engine camera.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw the end screen UI on top of the background.
            spriteBatch.Begin();
            _game.UIManager.DrawEndScreen(spriteBatch, graphicsDevice, _isVictory);
            spriteBatch.End();
        }
    }
}