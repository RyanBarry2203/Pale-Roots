using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
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
            _game.AudioManager.HandleMusicState(_isVictory ? GameState.Victory : GameState.GameOver);
        }

        public void Update(GameTime gameTime)
        {
            _game.IsMouseVisible = true;
            MouseState ms = Mouse.GetState();
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            int centerH = _game.GraphicsDevice.Viewport.Height / 2;

            Rectangle playAgainRect = new Rectangle(centerW - 100, centerH, 200, 50);
            Rectangle quitRect = new Rectangle(centerW - 100, centerH + 70, 200, 50);

            if (InputEngine.IsMouseLeftClick())
            {
                if (playAgainRect.Contains(ms.Position))
                {
                    if (_isVictory)
                    {
                        _game.StartOutroSequence();
                        _game.StateManager.ChangeState(new OutroState(_game));
                    }
                    else
                    {
                        _game.SoftResetGame();
                        _game.HasStarted = true;
                        _game.AudioManager.Stop();
                        _game.StateManager.ChangeState(new GameplayState(_game));
                    }
                    InputEngine.ClearState();
                }
                else if (quitRect.Contains(ms.Position))
                {
                    _game.Exit();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw background game engine
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw End Screen UI
            spriteBatch.Begin();
            _game.UIManager.DrawEndScreen(spriteBatch, graphicsDevice, _isVictory);
            spriteBatch.End();
        }
    }
}