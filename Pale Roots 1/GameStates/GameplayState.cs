using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class GameplayState : IGameState
    {
        private Game1 _game;
        private const int WIN_CONDITION_KILLS = 1;

        public GameplayState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            _game.AudioManager.HandleMusicState(GameState.Gameplay);
        }

        public void Update(GameTime gameTime)
        {
            if (InputEngine.IsKeyPressed(Keys.Escape))
            {
                _game.StateManager.ChangeState(new MenuState(_game));
                return;
            }

            if (_game.GameEngine != null)
            {
                _game.GameEngine.Update(gameTime);

                // Win/Loss
                if (_game.GameEngine.EnemiesKilled >= WIN_CONDITION_KILLS)
                    _game.StateManager.ChangeState(new EndGameState(_game, true));
                else if (!_game.GameEngine.GetPlayer().IsAlive)
                    _game.StateManager.ChangeState(new EndGameState(_game, false));

                // Level Up
                if (_game.GameEngine.EnemiesKilled >= _game.NextLevelThreshold)
                {
                    _game.CurrentUpgradeOptions = _game.UpgradeManager.GetRandomOptions(3);
                    if (_game.CurrentUpgradeOptions.Count > 0)
                    {
                        _game.StateManager.ChangeState(new LevelUpState(_game));
                    }
                    _game.LevelStep += 4;
                    _game.NextLevelThreshold += _game.LevelStep;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            _game.UIManager.DrawHUD(spriteBatch, graphicsDevice, _game.GameEngine, _game.SpellIcons, _game.DashIcon, _game.HeavyAttackIcon, WIN_CONDITION_KILLS);
            spriteBatch.End();
        }
    }
}