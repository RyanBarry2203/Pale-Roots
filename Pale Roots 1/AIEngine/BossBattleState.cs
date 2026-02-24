using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class BossBattleState : IGameState
    {
        private Game1 _game;
        private ChaseAndFireEngine _bossEngine;
        private BlackHoleBoss _boss;
        private bool _fightOver = false;

        // The callback to run when the fight ends
        private Action<bool> _onBattleEnd;

        public BossBattleState(Game1 game, Action<bool> onBattleEnd)
        {
            _game = game;
            _onBattleEnd = onBattleEnd;
        }

        public void LoadContent()
        {
            _bossEngine = new ChaseAndFireEngine(_game);
            _bossEngine._enemies.Clear();
            _bossEngine._allies.Clear();

            // Generate the Organic Arena
            _bossEngine._levelManager.GenerateBossArena();

            // Load Boss Texture
            // Ensure you have your Dictionary setup here from the previous step
            var bossTextures = new Dictionary<string, Texture2D>();
            bossTextures["Idle"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_idle");
            bossTextures["Walk"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_walk");
            bossTextures["Attack"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_attack");
            bossTextures["Hurt"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_hurt");
            bossTextures["Death"] = _game.Content.Load<Texture2D>("Boss_Sheets/Golem_1_die");

            // CALCULATE CENTER OF MAP (60 tiles * 64px / 2 = 1920, 34 tiles * 64px / 2 = 1088)
            Vector2 mapCenter = new Vector2(1920, 1088);

            // Spawn Boss in exact center
            _boss = new BlackHoleBoss(_game, bossTextures, mapCenter);
            _bossEngine._enemies.Add(_boss);

            // Spawn Player slightly to the left of center
            _bossEngine.GetPlayer().Position = mapCenter - new Vector2(400, 0);

            // Snap Camera to center immediately so we don't see the void for 1 frame
            _bossEngine._camera.LookAt(mapCenter, _game.GraphicsDevice.Viewport);

            Player bossArenaPlayer = _bossEngine.GetPlayer();
            SpellManager bossArenaSpells = _bossEngine.GetSpellManager();

            // Grant Temporary God Mode for the Boss Fight
            bossArenaPlayer.IsDashUnlocked = true;
            bossArenaPlayer.IsHeavyAttackUnlocked = true;
            for (int i = 0; i < 6; i++)
            {
                bossArenaSpells.UnlockSpell(i);
            }
        }

        public void Update(GameTime gameTime)
        {
            // 1. MUSIC: Ensure the Audio Manager knows we are in Gameplay mode (Combat Music)
            _game.AudioManager.HandleMusicState(GameState.Gameplay);

            // 2. PAUSE: Check for Escape key to open Menu
            if (InputEngine.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                // Push the MenuState on top. 
                // Because we use a Stack now, the Boss Fight will freeze exactly here.
                _game.StateManager.PushState(new MenuState(_game));
                return;
            }

            if (_fightOver) return;

            _bossEngine.Update(gameTime);
            _boss.UpdateBossLogic(gameTime, _bossEngine.GetPlayer());

            // Win Condition
            if (!_boss.IsAlive)
            {
                FinishFight(true);
            }
            // Lose Condition
            else if (!_bossEngine.GetPlayer().IsAlive)
            {
                FinishFight(false);
            }
        }

        private void FinishFight(bool playerWon)
        {
            _fightOver = true;
            // Transition out, passing the result
            _game.StateManager.ChangeState(new BossTransitionState(_game, false, playerWon, _onBattleEnd));
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _bossEngine._camera.CurrentCameraTranslation);
            _bossEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            _game.UIManager.DrawHUD(spriteBatch, graphicsDevice, _bossEngine, _game.SpellIcons, _game.DashIcon, _game.HeavyAttackIcon, 999);
            spriteBatch.End();
        }
    }
}