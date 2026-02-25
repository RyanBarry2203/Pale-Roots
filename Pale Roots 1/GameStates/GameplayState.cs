using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class GameplayState : IGameState
    {
        private Game1 _game;
        private const int WIN_CONDITION_KILLS = 150;

        private bool _isEnding = false;
        private bool _isVictory = false;
        private float _endTimer = 0f;
        private const float END_DELAY = 2.5f;

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
            _game.AudioManager.HandleMusicState(GameState.Gameplay);

            if (InputEngine.IsKeyPressed(Keys.Escape))
            {
                _game.StateManager.PushState(new MenuState(_game));
                return;
            }

            if (_game.GameEngine != null)
            {
                _game.GameEngine.Update(gameTime);

                // Win/Loss
                if (!_isEnding)
                {
                    if (_game.GameEngine.EnemiesKilled >= WIN_CONDITION_KILLS)
                    {
                        _isEnding = true;
                        _isVictory = true;
                    }
                    else if (!_game.GameEngine.GetPlayer().IsAlive)
                    {
                        _isEnding = true;
                        _isVictory = false;
                    }

                    if (!_isEnding && _game.GameEngine.EnemiesKilled >= _game.NextLevelThreshold)
                    {
                        _game.CurrentUpgradeOptions = _game.UpgradeManager.GetRandomOptions(3);
                        if (_game.CurrentUpgradeOptions.Count > 0)
                        {
                            _game.StateManager.ChangeState(new LevelUpState(_game));
                        }
                        _game.PreviousLevelThreshold = _game.NextLevelThreshold;
                        _game.LevelStep += 4;
                        _game.NextLevelThreshold += _game.LevelStep;
                    }
                }
                else
                {
                    // 3. The game is over, but we keep updating the engine to play animations!
                    _endTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_endTimer >= END_DELAY)
                    {
                        _game.StateManager.ChangeState(new EndGameState(_game, _isVictory));
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();

            float levelProgress = 0f;
            int killsNeeded = _game.NextLevelThreshold - _game.PreviousLevelThreshold;
            int killsEarned = _game.GameEngine.EnemiesKilled - _game.PreviousLevelThreshold;

            if (killsNeeded > 0)
            {
                levelProgress = MathHelper.Clamp((float)killsEarned / killsNeeded, 0f, 1f);
            }

            _game.UIManager.DrawHUD(spriteBatch, graphicsDevice, _game.GameEngine, _game.SpellIcons, _game.DashIcon, _game.HeavyAttackIcon, WIN_CONDITION_KILLS, levelProgress);
            spriteBatch.End();
        }
    }
}