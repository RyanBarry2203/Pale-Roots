using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using RandomEnemy;

namespace Pale_Roots_1
{
    class ChaseAndFireEngine
    {
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        Player p;
        List<Sprite> _allies = new List<Sprite>();
        List<ChargingBattleEnemy> _enemies = new List<ChargingBattleEnemy>(); // Renamed from _allies

        bool _battleStarted = false;
        Vector2 _mapSize = new Vector2(1920, 1920);

        // CHANGE 1: Move Enemy Spawn to the vertical CENTER (900 instead of 300)
        Vector2 _enemySpawnOrigin = new Vector2(1600, 900);

        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            game.IsMouseVisible = true;

            // 1. Setup Map
            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            // 2. Setup Player
            // CHANGE 2: Spawn Player at Y=900 (Center of Map)
            p = new Player(game, game.Content.Load<Texture2D>("wizard_strip3"), new Vector2(300, 900), 3);

            // 3. Setup Camera
            _camera = new Camera(Vector2.Zero, _mapSize);

            // 4. Initialize Armies
            InitializeArmies();

            // 5. Force Camera to Center
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;
            float targetZoom = (float)vp.Width / _mapSize.X;
            _camera.Zoom = targetZoom;
            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);
        }

        private void InitializeArmies()
        {
            Texture2D allyTx = _gameOwnedBy.Content.Load<Texture2D>("wizard_strip3");
            Texture2D enemyTx = _gameOwnedBy.Content.Load<Texture2D>("Dragon_strip3");

            // --- ALLIES (Line Formation) ---
            for (int i = 0; i < 5; i++)
            {
                // CHANGE 3: Spawn Allies around Y=900
                // Starting at 780 + increments centers them nicely around the player
                Vector2 pos = new Vector2(150, 780 + (i * 60));
                _allies.Add(new Sprite(_gameOwnedBy, allyTx, pos, 3, 0.70));
            }

            // --- ENEMIES (Triangle Formation) ---
            int countOfEnemies = 10;

            int currentRow = 0;
            int enemiesInCurrentRow = 1;
            int currentSlotInRow = 0;

            float spacingX = 80f;
            float spacingY = 80f;

            for (int i = 0; i < countOfEnemies; i++)
            {
                // Triangle Logic
                float xPos = _enemySpawnOrigin.X + (currentRow * spacingX);

                // This logic automatically centers the row on _enemySpawnOrigin.Y (which is now 900)
                float rowHeight = (enemiesInCurrentRow - 1) * spacingY;
                float rowStart = _enemySpawnOrigin.Y - (rowHeight / 2f);
                float yPos = rowStart + (currentSlotInRow * spacingY);

                _enemies.Add(new ChargingBattleEnemy(_gameOwnedBy, enemyTx, new Vector2(xPos, yPos), 3));

                currentSlotInRow++;
                if (currentSlotInRow >= enemiesInCurrentRow)
                {
                    currentRow++;
                    enemiesInCurrentRow++;
                    currentSlotInRow = 0;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;

            if (!_battleStarted)
            {
                // STATE 1: CINEMATIC VIEW
                float targetZoom = (float)vp.Width / _mapSize.X;
                _camera.Zoom = targetZoom;
                _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);

                p.Update(gameTime, _levelManager.CurrentLevel);

                if (Keyboard.GetState().IsKeyDown(Keys.D))
                    _battleStarted = true;
            }
            else
            {
                // STATE 2: CHARGE & BATTLE
                _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

                p.Update(gameTime, _levelManager.CurrentLevel);

                // Move Allies
                foreach (var ally in _allies)
                {
                    // 1. TARGETING: Look for an enemy partner if we don't have one
                    // Note: You may need to cast your 'ally' to a specialized class (e.g. Ally) 
                    // to access these properties, or add them to the base Sprite class.
                    if (ally.CurrentCombatPartner == null || ally.CurrentCombatPartner.Visible == false)
                    {
                        Sprite bestTarget = null;
                        float closestDist = float.MaxValue;

                        // Search through enemies for the best "skirmish"
                        foreach (var enemy in _enemies)
                        {
                            float d = Vector2.Distance(ally.Center, enemy.Center);

                            // 2v1 RULE: Only target an enemy if they have fewer than 2 allies on them
                            if (d < closestDist && enemy.AttackerCount < 2)
                            {
                                closestDist = d;
                                bestTarget = enemy;
                            }
                        }

                        if (bestTarget != null)
                        {
                            // If the ally was already attacking someone else, decrement that count first
                            if (ally.CurrentCombatPartner != null) ally.CurrentCombatPartner.AttackerCount--;

                            ally.CurrentCombatPartner = bestTarget;
                            ally.CurrentCombatPartner.AttackerCount++; // "Lock" the slot on the enemy
                            ally.CurrentAIState = Enemy.AISTATE.Chasing;
                        }
                    }

                    // 2. STATE EXECUTION FOR ALLIES
                    switch (ally.CurrentAIState)
                    {
                        case Enemy.AISTATE.Charging:
                            ally.position.X += 3.0f; // Allies charge RIGHT
                                                     // If an enemy enters our personal "chase zone", switch to Chasing
                            if (ally.CurrentCombatPartner != null && Vector2.Distance(ally.Center, ally.CurrentCombatPartner.Center) < 200)
                                ally.CurrentAIState = Enemy.AISTATE.Chasing;
                            break;

                        case Enemy.AISTATE.Chasing:
                            // This uses the 'follow' method to maintain the 60px distance (The Battle Line)
                            ally.follow(ally.CurrentCombatPartner);
                            break;

                        case Enemy.AISTATE.InCombat:
                            // Placeholder for ally attack animations
                            // If the enemy backs away or the fight moves, go back to chasing
                            if (Vector2.Distance(ally.Center, ally.CurrentCombatPartner.Center) > 80f)
                                ally.CurrentAIState = Enemy.AISTATE.Chasing;
                            break;
                    }

                    ally.Update(gameTime);
                }

                // Move Enemies with Logic
                foreach (var enemy in _enemies)
                {
                    // 1. TARGETING LOGIC: Find a partner if the enemy doesn't have one or if their partner died
                    if (enemy.CurrentCombatPartner == null || enemy.CurrentCombatPartner.Visible == false)
                    {
                        Sprite bestTarget = null;
                        float closestDist = float.MaxValue;

                        // Check the Player first
                        if (p.AttackerCount < 2)
                        {
                            closestDist = Vector2.Distance(enemy.Center, p.Center);
                            bestTarget = p;
                        }

                        // Check Allies - only those with fewer than 2 attackers
                        foreach (var ally in _allies)
                        {
                            float d = Vector2.Distance(enemy.Center, ally.Center);
                            if (d < closestDist && ally.AttackerCount < 2)
                            {
                                closestDist = d;
                                bestTarget = ally;
                            }
                        }

                        if (bestTarget != null)
                        {
                            enemy.CurrentCombatPartner = bestTarget;
                            enemy.CurrentCombatPartner.AttackerCount++; // "Claim" one of the 2 slots
                            enemy.CurrentAIState = Enemy.AISTATE.Chasing;
                        }
                        else if (enemy.CurrentAIState != Enemy.AISTATE.Charging)
                        {
                            // If no one is available to fight, wander around
                            enemy.CurrentAIState = Enemy.AISTATE.Wandering;
                        }
                    }

                    // 2. STATE EXECUTION
                    switch (enemy.CurrentAIState)
                    {
                        case Enemy.AISTATE.Charging:
                            enemy.position.X -= 3.0f; // Continue the initial charge
                                                      // If a partner was found and is close enough, start chasing specifically
                            if (enemy.CurrentCombatPartner != null && enemy.inChaseZone(enemy.CurrentCombatPartner))
                                enemy.CurrentAIState = Enemy.AISTATE.Chasing;
                            break;

                        case Enemy.AISTATE.Chasing:
                            enemy.follow(enemy.CurrentCombatPartner);
                            break;

                        case Enemy.AISTATE.InCombat:
                            // Currently does nothing - this is where weapon animations will go next!
                            // If the target moves away, go back to chasing
                            if (Vector2.Distance(enemy.Center, enemy.CurrentCombatPartner.Center) > 80f)
                                enemy.CurrentAIState = Enemy.AISTATE.Chasing;
                            break;

                        case Enemy.AISTATE.Wandering:
                            // Placeholder: Could use your RandomEnemy logic here
                            enemy.position.X -= 1.0f;
                            break;
                    }

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