using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This state acts as an entirely separate level just for the boss fight.
    // By keeping it isolated from GameplayState, we can strictly control the environment, camera, and rules.
    public class BossBattleState : IGameState
    {
        private Game1 _game;

        // spin up a totally fresh instance of our core engine just for this fight.
        private ChaseAndFireEngine _bossEngine;
        private BlackHoleBoss _boss;

        // Tracking the flow of the battle so we know when to trigger the ending sequence.
        private bool _fightOver = false;
        private float _endTimer = 0f;
        private const float END_DELAY = 2.5f;
        private bool _playerWon = false;

        // A delegate we can call once the fight is totally wrapped up, returning us to the main Game.
        private Action<bool> _onBattleEnd;

        public BossBattleState(Game1 game, Action<bool> onBattleEnd)
        {
            _game = game;
            _onBattleEnd = onBattleEnd;
        }

        public void LoadContent()
        {
            // Initialize the arena engine and clear out any leftover junk so we start with a clean slate.
            _bossEngine = new ChaseAndFireEngine(_game);
            _bossEngine.IsBossArena = true;
            _bossEngine._enemies.Clear();
            _bossEngine._allies.Clear();

            // Ask the LevelManager to prodecurally generate our new level.
            _bossEngine._levelManager.GenerateBossArena();

            // Load all the sprite sheets required for the boss into a temporary dictionary.
            var bossTextures = new Dictionary<string, Texture2D>();
            bossTextures["Idle"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_idle");
            bossTextures["Walk"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_walk");
            bossTextures["Attack"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_attack");
            bossTextures["Hurt"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_hurt");
            bossTextures["Death"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_die");

            // Calculate the absolute center of the generated tilemap using our 64x64 tile size.
            Vector2 mapCenter = new Vector2(1920, 1088 + 150);

            // Spawn the boss directly in the middle of the room.
            _boss = new BlackHoleBoss(_game, bossTextures, mapCenter);

            // Lock the boss inside the mathematical circle so it doesn't get stuck on jagged tree hitboxes.
            _boss.UseCircularBounds = true;
            _boss.CircularBoundsCenter = mapCenter;
            _boss.CircularBoundsRadius = 1400f;

            _bossEngine._enemies.Add(_boss);

            // Spawn the player offset to the left so they don't immediately collide with the boss on frame 1.
            Player bossArenaPlayer = _bossEngine.GetPlayer();
            bossArenaPlayer.Position = mapCenter - new Vector2(400, 0);

            // Lock the player inside the exact same mathematical circle to prevent corner cheese from playtesting.
            bossArenaPlayer.UseCircularBounds = true;
            bossArenaPlayer.CircularBoundsCenter = mapCenter;
            bossArenaPlayer.CircularBoundsRadius = 1100f;

            // Instantly snap the camera to the center so the player doesn't see the camera panning over the black void during setup.
            _bossEngine._camera.LookAt(mapCenter, _game.GraphicsDevice.Viewport);
            SpellManager bossArenaSpells = _bossEngine.GetSpellManager();

            // We temporarily unlock the player's full arsenal.
            bossArenaPlayer.IsDashUnlocked = true;
            bossArenaPlayer.IsHeavyAttackUnlocked = true;
            for (int i = 0; i < 6; i++)
            {
                bossArenaSpells.UnlockSpell(i);
            }
        }

        public void Update(GameTime gameTime)
        {
            // Talk to the AudioManager to make sure the intense combat music is looping.
            _game.AudioManager.HandleMusicState(GameState.Gameplay);

            // Listen for the pause input to stack the MenuState on top of the boss fight.
            if (InputEngine.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                _game.StateManager.PushState(new MenuState(_game));
                return;
            }

            // Step the core physics and collision engine forward, then explicitly run the custom boss logic.
            _bossEngine.Update(gameTime);
            _boss.UpdateBossLogic(gameTime, _bossEngine.GetPlayer());

            // Check our win/loss conditions every frame.
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
                // Once someone dies, run a short delay timer so the death animations can play out 
                // before we violently yank the player out of the arena.
                _endTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_endTimer >= END_DELAY)
                {
                    FinishFight(_playerWon);
                }
            }
        }

        private void FinishFight(bool playerWon)
        {
            // Hand control over to the transition state, passing along whether we won or lost 
            // so it knows what text/effects to display.
            _fightOver = true;
            _game.StateManager.ChangeState(new BossTransitionState(_game, _bossEngine, false, playerWon, _onBattleEnd));
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Pass the camera's translation matrix to the sprite batch so the game world renders in the right spot.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _bossEngine._camera.CurrentCameraTranslation);
            _bossEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw the HUD directly to the screen (no camera matrix). 
            // We pass in 999 for the score and 0 for the timer since standard level tracking doesn't matter in the boss arena.
            spriteBatch.Begin();
            _game.UIManager.DrawHUD(spriteBatch, graphicsDevice, _bossEngine, _game.SpellIcons, _game.DashIcon, _game.HeavyAttackIcon, 999, 0f);
            spriteBatch.End();
        }
    }
}