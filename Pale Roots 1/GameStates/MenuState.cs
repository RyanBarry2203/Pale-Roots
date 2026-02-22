using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class MenuState : IGameState
    {
        private Game1 _game;
        private Rectangle _playBtnRect;
        private Rectangle _quitBtnRect;

        public MenuState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            int centerH = _game.GraphicsDevice.Viewport.Height / 2;

            _playBtnRect = new Rectangle(centerW - 120, centerH - 40, 240, 50);
            _quitBtnRect = new Rectangle(centerW - 120, centerH + 40, 240, 50);

            // Tell the Audio Manager we are in the menu
            _game.AudioManager.HandleMusicState(GameState.Menu);
        }

        public void Update(GameTime gameTime)
        {
            _game.IsMouseVisible = true;
            MouseState ms = Mouse.GetState();
            Point mousePoint = new Point(ms.X, ms.Y);

            if (InputEngine.IsMouseLeftClick())
            {
                if (_playBtnRect.Contains(mousePoint))
                {
                    
                    if (!_game.HasStarted) 
                    {
                        _game.StateManager.ChangeState(new IntroState(_game)); 
                    }
                    else 
                    {
                        _game.StateManager.ChangeState(new GameplayState(_game)); 
                    }
                    
                }
                else if (_quitBtnRect.Contains(mousePoint))
                {
                    _game.Exit();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            if (_game.MenuBackground != null)
            {
                spriteBatch.Draw(_game.MenuBackground, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
            }
            else
            {
                // If there's no background, draw the gameplay level engine behind the menu
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
                _game.GameEngine.Draw(gameTime, spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin();
            }

            // Let the UIManager handle the heavy lifting
            _game.UIManager.DrawMenu(spriteBatch, graphicsDevice, _playBtnRect, _quitBtnRect, _game.HasStarted);

            spriteBatch.End();
        }
    }
}