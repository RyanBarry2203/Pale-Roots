using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // This is the primary state where the actual game happens. 
    // It sits on top of the GameEngine, feeding it updates and monitoring the player's 
    // progress to figure out when they should level up, win, or die.
    public class GameplayState : IGameState
    {
        private Game1 _game;

        // The total number of enemies the player needs to defeat to trigger the final victory.
        private const int WIN_CONDITION_KILLS = 120;

        // Flags to track when the game is over and what the outcome was.
        private bool _isEnding = false;
        private bool _isVictory = false;

        // When the player wins or dies, we let the game run for a few more seconds 
        // so death animations can play out before snapping to the End Game screen.
        private float _endTimer = 0f;
        private const float END_DELAY = 2.5f;

        public GameplayState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Ensure the AudioManager is playing the main combat track.
            _game.AudioManager.HandleMusicState(GameState.Gameplay);
        }

        public void Update(GameTime gameTime)
        {
            // We constantly re-assert the music state here just in case another state 
            // temporarily interrupted it (like pausing).
            _game.AudioManager.HandleMusicState(GameState.Gameplay);

            // Listen for the escape key to pause the game by pushing the MenuState on top of this one.
            if (InputEngine.IsKeyPressed(Keys.Escape))
            {
                _game.StateManager.PushState(new MenuState(_game));
                return;
            }

            if (_game.GameEngine != null)
            {
                _game.GameEngine.Update(gameTime);

                // If the game is still actively being played...
                if (!_isEnding)
                {
                    // Check if the player has hit the final kill count to win the game.
                    if (_game.GameEngine.EnemiesKilled >= WIN_CONDITION_KILLS)
                    {
                        _isEnding = true;
                        _isVictory = true;
                    }
                    // Or check if the player's health dropped to zero, meaning they lost.
                    else if (!_game.GameEngine.GetPlayer().IsAlive)
                    {
                        _isEnding = true;
                        _isVictory = false;
                    }

                    // If neither of the game-ending conditions were met, check if the player 
                    // has killed enough enemies to trigger a mid-game level up.
                    if (!_isEnding && _game.GameEngine.EnemiesKilled >= _game.NextLevelThreshold)
                    {
                        // Ask the UpgradeManager to generate 3 random power-ups to choose from.
                        _game.CurrentUpgradeOptions = _game.UpgradeManager.GetRandomOptions(3);
                        if (_game.CurrentUpgradeOptions.Count > 0)
                        {
                            // Temporarily switch to the LevelUpState so the player can pick their reward.
                            _game.StateManager.ChangeState(new LevelUpState(_game));
                        }

                        // Recalculate the math for the next level up. 
                        // We increase the 'LevelStep' so each subsequent level takes progressively more kills to reach.
                        _game.PreviousLevelThreshold = _game.NextLevelThreshold;
                        _game.LevelStep += 4;
                        _game.NextLevelThreshold += _game.LevelStep;
                    }
                }
                // If the game is ending (either by winning or losing)...
                else
                {
                    // Keep updating the GameEngine (so characters keep falling over or enemies keep moving), 
                    // but start ticking up the timer.
                    _endTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Once the short delay finishes, permanently transition to the EndGameState.
                    if (_endTimer >= END_DELAY)
                    {
                        _game.StateManager.ChangeState(new EndGameState(_game, _isVictory));
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // First, draw the entire game world (player, enemies, map) using the camera's translation matrix.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Next, we draw the HUD elements directly to the screen (no camera matrix).
            spriteBatch.Begin();

            // Calculate exactly how far the player has progressed toward their next level up, 
            // returning a percentage between 0.0 and 1.0 so the UI can draw a progress bar.
            float levelProgress = 0f;
            int killsNeeded = _game.NextLevelThreshold - _game.PreviousLevelThreshold;
            int killsEarned = _game.GameEngine.EnemiesKilled - _game.PreviousLevelThreshold;

            if (killsNeeded > 0)
            {
                levelProgress = MathHelper.Clamp((float)killsEarned / killsNeeded, 0f, 1f);
            }

            // Pass all the calculated data over to the UIManager to actually draw the health bars, score, and skill icons.
            _game.UIManager.DrawHUD(spriteBatch, graphicsDevice, _game.GameEngine, _game.SpellIcons, _game.DashIcon, _game.HeavyAttackIcon, WIN_CONDITION_KILLS, levelProgress);
            spriteBatch.End();
        }
    }
}