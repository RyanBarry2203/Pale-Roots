using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Primary game state that runs the main gameplay loop and monitors win, loss, and level-up.
    public class GameplayState : IGameState
    {
        private Game1 _game;

        // Number of enemy kills required for final victory.
        private const int WIN_CONDITION_KILLS = 120;

        // Flags tracking end of game and outcome.
        private bool _isEnding = false;
        private bool _isVictory = false;

        // Timer to allow death or victory animations to finish before switching states.
        private float _endTimer = 0f;
        private const float END_DELAY = 2.5f;

        public GameplayState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Request the gameplay combat music from the audio manager.
            _game.AudioManager.HandleMusicState(GameState.Gameplay);
        }

        public void Update(GameTime gameTime)
        {
            // Re-assert the gameplay music in case another state changed it.
            _game.AudioManager.HandleMusicState(GameState.Gameplay);

            // Allow the player to pause by pushing the menu state.
            if (InputEngine.IsKeyPressed(Keys.Escape))
            {
                _game.StateManager.PushState(new MenuState(_game));
                return;
            }

            if (_game.GameEngine != null)
            {
                _game.GameEngine.Update(gameTime);

                // If the gameplay session is active, check win/lose and level-up conditions.
                if (!_isEnding)
                {
                    // Win condition based on total enemy kills.
                    if (_game.GameEngine.EnemiesKilled >= WIN_CONDITION_KILLS)
                    {
                        _isEnding = true;
                        _isVictory = true;
                    }
                    // Loss condition if the player died.
                    else if (!_game.GameEngine.GetPlayer().IsAlive)
                    {
                        _isEnding = true;
                        _isVictory = false;
                    }

                    // Check for mid-game level up based on kills.
                    if (!_isEnding && _game.GameEngine.EnemiesKilled >= _game.NextLevelThreshold)
                    {
                        // Generate upgrade options and push the level up state if any are available.
                        _game.CurrentUpgradeOptions = _game.UpgradeManager.GetRandomOptions(3);
                        if (_game.CurrentUpgradeOptions.Count > 0)
                        {
                            _game.StateManager.ChangeState(new LevelUpState(_game));
                        }

                        // Recalculate thresholds for the next level up.
                        _game.PreviousLevelThreshold = _game.NextLevelThreshold;
                        _game.LevelStep += 4;
                        _game.NextLevelThreshold += _game.LevelStep;
                    }
                }
                // When the game is ending, let the engine keep updating but run the end timer.
                else
                {
                    _endTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // After the delay, transition to the end game screen with the result.
                    if (_endTimer >= END_DELAY)
                    {
                        _game.StateManager.ChangeState(new EndGameState(_game, _isVictory));
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw the game world using the engine camera so the scene is positioned correctly.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw the HUD directly to the screen without camera transforms.
            spriteBatch.Begin();

            // Compute progress toward the next level up as a normalized value for the UI.
            float levelProgress = 0f;
            int killsNeeded = _game.NextLevelThreshold - _game.PreviousLevelThreshold;
            int killsEarned = _game.GameEngine.EnemiesKilled - _game.PreviousLevelThreshold;

            if (killsNeeded > 0)
            {
                levelProgress = MathHelper.Clamp((float)killsEarned / killsNeeded, 0f, 1f);
            }

            // Let the UIManager draw health, score, and level progress using the engine data.
            _game.UIManager.DrawHUD(spriteBatch, graphicsDevice, _game.GameEngine, _game.SpellIcons, _game.DashIcon, _game.HeavyAttackIcon, WIN_CONDITION_KILLS, levelProgress);
            spriteBatch.End();
        }
    }
}