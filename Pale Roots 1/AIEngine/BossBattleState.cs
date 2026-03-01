using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This state runs the boss fight level and controls the arena and rules, becauyse we push and pop this state and everything used in the state
    // is freshly initiliased, once we leave this state every morcel of data used in it is gone, making the architecture clean and efficient.
    public class BossBattleState : IGameState
    {
        private Game1 _game;

        // Engine instance dedicated to the boss arena.
        private ChaseAndFireEngine _bossEngine;
        private BlackHoleBoss _boss;

        // Track battle end and result.
        private bool _fightOver = false;
        private float _endTimer = 0f;
        private const float END_DELAY = 2.5f;
        private bool _playerWon = false;

        // Callback invoked when the battle finishes.
        private Action<bool> _onBattleEnd;

        public BossBattleState(Game1 game, Action<bool> onBattleEnd)
        {
            _game = game;
            _onBattleEnd = onBattleEnd;
        }

        public void LoadContent()
        {
            // Create the arena engine and clear existing entities.
            _bossEngine = new ChaseAndFireEngine(_game);
            _bossEngine.IsBossArena = true;
            _bossEngine._enemies.Clear();
            _bossEngine._allies.Clear();

            // Generate the boss arena layout.
            _bossEngine._levelManager.GenerateBossArena();

            // Load boss sprite sheets into a temporary dictionary.
            var bossTextures = new Dictionary<string, Texture2D>();
            bossTextures["Idle"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_idle");
            bossTextures["Walk"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_walk");
            bossTextures["Attack"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_attack");
            bossTextures["Hurt"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_hurt");
            bossTextures["Death"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_die");

            // Compute the center position for the arena and offset slightly.
            Vector2 mapCenter = new Vector2(1920, 1088 + 150);

            // Spawn the boss at the center of the room.
            _boss = new BlackHoleBoss(_game, bossTextures, mapCenter);

            // Constrain the boss to a circular area to avoid geometry issues.
            _boss.UseCircularBounds = true;
            _boss.CircularBoundsCenter = mapCenter;
            _boss.CircularBoundsRadius = 1400f;

            _bossEngine._enemies.Add(_boss);

            // Place the player to the left of the boss to avoid immediate collision.
            Player bossArenaPlayer = _bossEngine.GetPlayer();
            bossArenaPlayer.Position = mapCenter - new Vector2(400, 0);

            // Constrain the player to the arena circle to prevent corner exploits.
            bossArenaPlayer.UseCircularBounds = true;
            bossArenaPlayer.CircularBoundsCenter = mapCenter;
            bossArenaPlayer.CircularBoundsRadius = 950f;

            // Snap the camera to the arena center for a clean start.
            _bossEngine._camera.LookAt(mapCenter, _game.GraphicsDevice.Viewport);
            SpellManager bossArenaSpells = _bossEngine.GetSpellManager();

            // Unlock the player's abilities for the boss fight.
            bossArenaPlayer.IsDashUnlocked = true;
            bossArenaPlayer.IsHeavyAttackUnlocked = true;
            for (int i = 0; i < 6; i++)
            {
                bossArenaSpells.UnlockSpell(i);
            }
        }

        public void Update(GameTime gameTime)
        {
            // Ensure combat music plays during the fight.
            _game.AudioManager.HandleMusicState(GameState.Gameplay);

            // Allow the player to pause and open the menu.
            if (InputEngine.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                _game.StateManager.PushState(new MenuState(_game));
                return;
            }

            // Advance the engine and run boss specific logic.
            _bossEngine.Update(gameTime);
            _boss.UpdateBossLogic(gameTime, _bossEngine.GetPlayer());

            // Check for win or loss conditions each frame.
            if (!_fightOver)
            {
                if (!_boss.IsAlive)
                {
                    _fightOver = true;
                    _playerWon = true;
                }
                else if (!_bossEngine.GetPlayer().IsAlive)
                {
                    _fightOver = true;
                    _playerWon = false;
                }
            }
            else
            {
                // Wait briefly after a death so animations can finish, then end the fight.
                _endTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_endTimer >= END_DELAY)
                {
                    FinishFight(_playerWon);
                }
            }
        }

        private void FinishFight(bool playerWon)
        {
            // Transition to the boss transition state and pass the result.
            _fightOver = true;
            _game.StateManager.ChangeState(new BossTransitionState(_game, _bossEngine, false, playerWon, _onBattleEnd));
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw the world using the arena camera matrix.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _bossEngine._camera.CurrentCameraTranslation);
            _bossEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw the HUD without camera transformation and use fixed placeholders for score and timer.
            spriteBatch.Begin();
            _game.UIManager.DrawHUD(spriteBatch, graphicsDevice, _bossEngine, _game.SpellIcons, _game.DashIcon, _game.HeavyAttackIcon, 999, 0f);
            spriteBatch.End();
        }
    }
}