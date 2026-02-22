using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class LevelUpState : IGameState
    {
        private Game1 _game;
        private float _inputDelay = 0.5f; // Safety delay so player doesn't instantly click

        public LevelUpState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            _game.AudioManager.HandleMusicState(GameState.LevelUp);
        }

        public void Update(GameTime gameTime)
        {
            _game.IsMouseVisible = true;

            if (_inputDelay > 0)
            {
                _inputDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            if (InputEngine.IsMouseLeftClick())
            {
                MouseState ms = Mouse.GetState();
                Rectangle screen = _game.GraphicsDevice.Viewport.Bounds;
                int cardWidth = 200;
                int cardHeight = 300;
                int spacing = 50;
                int totalWidth = (_game.CurrentUpgradeOptions.Count * cardWidth) + ((_game.CurrentUpgradeOptions.Count - 1) * spacing);
                int startX = (screen.Width / 2) - (totalWidth / 2);
                int startY = (screen.Height / 2) - (cardHeight / 2);

                for (int i = 0; i < _game.CurrentUpgradeOptions.Count; i++)
                {
                    Rectangle cardRect = new Rectangle(startX + (i * (cardWidth + spacing)), startY, cardWidth, cardHeight);
                    if (cardRect.Contains(ms.Position))
                    {
                        _game.CurrentUpgradeOptions[i].ApplyAction.Invoke();
                        _game.StateManager.ChangeState(new GameplayState(_game));
                        InputEngine.ClearState();
                        break;
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw the game frozen in the background
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw the UI on top
            spriteBatch.Begin();
            _game.UIManager.DrawLevelUpScreen(spriteBatch, graphicsDevice, _game.CurrentUpgradeOptions, _game.UpgradeManager);
            spriteBatch.End();
        }
    }
}