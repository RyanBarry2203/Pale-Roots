using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    class ChaseAndFireEngine
    {
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        Player p;
        List<Sprite> _allies = new List<Sprite>();
        List<ChargingBattleEnemy> _enemies = new List<ChargingBattleEnemy>();

        bool _battleStarted = false;
        Vector2 _mapSize = new Vector2(1920, 1920);
        Vector2 _enemySpawnOrigin = new Vector2(1600, 900);

        float _targetingTimer = 0f;
        float _targetingInterval = 500f; // Scan every 0.5s

        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            p = new Player(game, game.Content.Load<Texture2D>("wizard_strip3"), new Vector2(300, 900), 3);
            _camera = new Camera(Vector2.Zero, _mapSize);

            InitializeArmies();

            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;
            _camera.Zoom = (float)vp.Width / _mapSize.X;
            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);
        }

        private void InitializeArmies()
        {
            Texture2D allyTx = _gameOwnedBy.Content.Load<Texture2D>("wizard_strip3");
            Texture2D enemyTx = _gameOwnedBy.Content.Load<Texture2D>("Dragon_strip3");

            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = new Vector2(150, 780 + (i * 60));
                _allies.Add(new Sprite(_gameOwnedBy, allyTx, pos, 3, 0.70));
            }

            int countOfEnemies = 10;
            int currentRow = 0, enemiesInCurrentRow = 1, currentSlotInRow = 0;
            float spacingX = 80f, spacingY = 80f;

            for (int i = 0; i < countOfEnemies; i++)
            {
                float xPos = _enemySpawnOrigin.X + (currentRow * spacingX);
                float rowHeight = (enemiesInCurrentRow - 1) * spacingY;
                float yPos = (_enemySpawnOrigin.Y - (rowHeight / 2f)) + (currentSlotInRow * spacingY);

                _enemies.Add(new ChargingBattleEnemy(_gameOwnedBy, enemyTx, new Vector2(xPos, yPos), 3));

                currentSlotInRow++;
                if (currentSlotInRow >= enemiesInCurrentRow) { currentRow++; enemiesInCurrentRow++; currentSlotInRow = 0; }
            }
        }

        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;

            if (!_battleStarted)
            {
                p.Update(gameTime, _levelManager.CurrentLevel);
                if (Keyboard.GetState().IsKeyDown(Keys.D)) _battleStarted = true;
            }
            else
            {
                _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);
                p.Update(gameTime, _levelManager.CurrentLevel);

                _targetingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                bool scanNow = _targetingTimer >= _targetingInterval;
                if (scanNow) _targetingTimer = 0;

                // Update Allies
                foreach (var ally in _allies)
                {
                    if (scanNow && (ally.CurrentCombatPartner == null || !ally.CurrentCombatPartner.Visible))
                    {
                        Sprite bestE = null; float close = float.MaxValue;
                        foreach (var e in _enemies)
                        {
                            float d = Vector2.Distance(ally.Center, e.Center);
                            if (d < close && e.AttackerCount < 2) { close = d; bestE = e; }
                        }
                        if (bestE != null)
                        {
                            if (ally.CurrentCombatPartner != null) ally.CurrentCombatPartner.AttackerCount--;
                            ally.CurrentCombatPartner = bestE;
                            ally.CurrentCombatPartner.AttackerCount++;
                            ally.CurrentAIState = Enemy.AISTATE.Chasing;
                        }
                    }

                    if (ally.CurrentAIState == Enemy.AISTATE.Charging) ally.position.X += 3.0f;
                    else if (ally.CurrentAIState == Enemy.AISTATE.Chasing && ally.CurrentCombatPartner != null)
                    {
                        Vector2 dir = ally.CurrentCombatPartner.Center - ally.position;
                        if (dir != Vector2.Zero) { dir.Normalize(); ally.position += dir * 3f; }
                        if (Vector2.Distance(ally.Center, ally.CurrentCombatPartner.Center) < 70f) ally.CurrentAIState = Enemy.AISTATE.InCombat;
                    }
                    ally.Update(gameTime);
                }

                // Update Enemies
                foreach (var enemy in _enemies)
                {
                    if (scanNow && (enemy.CurrentCombatPartner == null || !enemy.CurrentCombatPartner.Visible))
                    {
                        Sprite bestA = (p.AttackerCount < 2) ? p : null;
                        float close = (bestA != null) ? Vector2.Distance(enemy.Center, p.Center) : float.MaxValue;

                        foreach (var a in _allies)
                        {
                            float d = Vector2.Distance(enemy.Center, a.Center);
                            if (d < close && a.AttackerCount < 2) { close = d; bestA = a; }
                        }
                        if (bestA != null)
                        {
                            if (enemy.CurrentCombatPartner != null) enemy.CurrentCombatPartner.AttackerCount--;
                            enemy.CurrentCombatPartner = bestA;
                            enemy.CurrentCombatPartner.AttackerCount++;
                            enemy.CurrentAIState = Enemy.AISTATE.Chasing;
                        }
                    }

                    if (enemy.CurrentAIState == Enemy.AISTATE.Charging) enemy.position.X -= 3.0f;
                    enemy.Update(gameTime);
                }
                _camera.follow(p.CentrePos, vp);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _levelManager.Draw(spriteBatch);
            p.Draw(spriteBatch);
            foreach (var ally in _allies) ally.Draw(spriteBatch);
            foreach (var enemy in _enemies) enemy.Draw(spriteBatch);
        }
    }
}