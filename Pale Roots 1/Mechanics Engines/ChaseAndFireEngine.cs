using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    /// <summary>
    /// Main game engine handling the battle between player/allies and enemies.
    /// 
    /// CLEANED UP:
    /// - Uses new Ally class instead of plain Sprites
    /// - Uses CombatSystem for target assignment
    /// - Uses GameConstants instead of magic numbers
    /// - Subscribes to combat events for effects
    /// </summary>
    public class ChaseAndFireEngine
    {
        // ===================
        // REFERENCES
        // ===================
        
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        // ===================
        // ENTITIES
        // ===================
        
        private Player _player;
        private List<Ally> _allies = new List<Ally>();
        private List<Enemy> _enemies = new List<Enemy>();

        // ===================
        // BATTLE STATE
        // ===================

        private bool _battleStarted = false;
        private Vector2 _mapSize;
        private Vector2 _playerSpawnPos = new Vector2(500, 1230);
        private Vector2 _allySpawnOrigin = new Vector2(400, 1100);
        private Vector2 _enemySpawnOrigin = new Vector2(3200, 1230);

        private List<Dictionary<string, Texture2D>> _allOrcTypes = new List<Dictionary<string, Texture2D>>();
        private Dictionary<string, Texture2D> _allyTextures = new Dictionary<string, Texture2D>();

        // ===================
        // TARGETING
        // ===================

        private float _targetingTimer = 0f;

        // ===================
        // STATS (for UI)
        // ===================
        
        public int EnemiesKilled { get; private set; }
        public int AlliesLost { get; private set; }

        // ===================
        // CONSTRUCTOR
        // ===================
        
        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            _mapSize = GameConstants.DefaultMapSize;
            
            // Initialize level
            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            Texture2D swordTx = game.Content.Load<Texture2D>("sword");

            // Create player
            _player = new Player(
                game, 
                game.Content.Load<Texture2D>("wizard_strip3"), 
                _playerSpawnPos, 
                3
            );
            _player.Name = "Hero";

            // Setup camera
            _camera = new Camera(Vector2.Zero, _mapSize);
            Viewport vp = game.GraphicsDevice.Viewport;

            float scaleX = (float)vp.Width / _mapSize.X;
            float scaleY = (float)vp.Height / _mapSize.Y;


            _camera.Zoom = Math.Min(scaleX, scaleY);


            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);

            // Create armies
            InitializeArmies();
            
            // Subscribe to combat events
            SetupCombatEvents();
        }

        // ===================
        // INITIALIZATION
        // ===================
        
        private void InitializeArmies()
        {

            //_orcTextures["Idle"] = _gameOwnedBy.Content.Load<Texture2D>("RealEnemyFolder/orc1_idle_full");
            //_orcTextures["Walk"] = _gameOwnedBy.Content.Load<Texture2D>("RealEnemyFolder/orc1_run_full");
            //_orcTextures["Attack"] = _gameOwnedBy.Content.Load<Texture2D>("RealEnemyFolder/orc1_attack_full");
            //_orcTextures["Hurt"] = _gameOwnedBy.Content.Load<Texture2D>("RealEnemyFolder/orc1_hurt_full");
            //_orcTextures["Death"] = _gameOwnedBy.Content.Load<Texture2D>("RealEnemyFolder/orc1_death_full");

            for (int i = 1; i <= 3; i++)
            {
                Dictionary<string, Texture2D> newOrcDict = new Dictionary<string, Texture2D>();

                newOrcDict["Idle"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_idle_full");
                newOrcDict["Walk"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_run_full");
                newOrcDict["Attack"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_attack_full");
                newOrcDict["Hurt"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_hurt_full");
                newOrcDict["Death"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_death_full");

                _allOrcTypes.Add(newOrcDict);
            }

            // --- LOAD ALLY TEXTURES ---
            _allyTextures["Walk"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Walk");
            _allyTextures["Attack"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Slash");
            _allyTextures["Idle"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Idle");

            // Create 5 allies in a column
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = _allySpawnOrigin + new Vector2(0, i * 100);
                var ally = new Ally(_gameOwnedBy, _allyTextures, pos, 4);
                ally.Name = $"Soldier {i + 1}";
                _allies.Add(ally);
            }

            // Create 10 enemies in a triangle formation
            CreateEnemyFormation(10);
        }

        private void CreateEnemyFormation(int count)
        {
            int currentRow = 0;
            int enemiesInCurrentRow = 1;
            int currentSlotInRow = 0;
            float spacingX = 80f;
            float spacingY = 80f;

            for (int i = 0; i < count; i++)
            {
                float xPos = _enemySpawnOrigin.X + (currentRow * spacingX);
                float rowHeight = (enemiesInCurrentRow - 1) * spacingY;
                float yPos = (_enemySpawnOrigin.Y - (rowHeight / 2f)) + (currentSlotInRow * spacingY);

                int typeIndex = 0;

                if (i == 0)
                {
                    typeIndex = 2; 
                }
                else
                {
                    typeIndex = CombatSystem.RandomInt(0, 2);
                }


                var enemy = new Enemy(_gameOwnedBy, _allOrcTypes[typeIndex], new Vector2(xPos, yPos), 4);

                if (typeIndex == 0) 
                {
                    enemy.Name = $"Orc Grunt {i}";
                    enemy.AttackDamage = 10;
                }
                else if (typeIndex == 1) 
                {
                    enemy.Name = $"Orc Warrior {i}";
                    enemy.AttackDamage = 20;
                }
                else 
                {
                    enemy.Name = $"Orc Captain {i}";
                    enemy.AttackDamage = 35;
                    enemy.Scale = 3.5f; 
                }

                _enemies.Add(enemy);

                currentSlotInRow++;
                if (currentSlotInRow >= enemiesInCurrentRow)
                {
                    currentRow++;
                    enemiesInCurrentRow++;
                    currentSlotInRow = 0;
                }
            }
        }

        private void SetupCombatEvents()
        {
            // Track kills AND Spawn Reinforcements
            CombatSystem.OnCombatantKilled += (killer, victim) =>
            {
                if (victim.Team == CombatTeam.Enemy)
                {
                    EnemiesKilled++;
                    // Enemy dies -> 2 new Enemies appear
                    SpawnReinforcements(CombatTeam.Enemy, 2);
                }
                else if (victim.Team == CombatTeam.Player && victim != _player)
                {
                    AlliesLost++;
                    // Ally dies -> 2 new Allies appear
                    SpawnReinforcements(CombatTeam.Player, 2);
                }
            };

            // Keep the rest of your event logic...
            CombatSystem.OnDamageDealt += (attacker, target, damage) =>
            {
                // Play hit sound, spawn damage number, etc.
            };
        }

        // ===================
        // UPDATE
        // ===================

        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;

            if (!_battleStarted)
            {
                // Pre-battle: just move player around
                _player.Update(gameTime, _levelManager.CurrentLevel, _enemies);

                // Press D to start battle
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    _battleStarted = true;
                }
            }
            else
            {
                UpdateBattle(gameTime, vp);
            }
        }

        private void UpdateBattle(GameTime gameTime, Viewport vp)
        {
            // Zoom in to battle view
            //_camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

            // Update player
            _player.Update(gameTime, _levelManager.CurrentLevel, _enemies);

            _levelManager.Update(gameTime, _player);

            // Targeting scan
            _targetingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            bool scanNow = _targetingTimer >= GameConstants.TargetScanInterval;
            if (scanNow) _targetingTimer = 0;

            // Update allies
            UpdateAllies(gameTime, scanNow);

            // Update enemies  
            UpdateEnemies(gameTime, scanNow);

            // Clean up dead entities
            CleanupDead();

            // Camera follows player
            _camera.follow(_player.CentrePos, vp);
        }

        private void UpdateAllies(GameTime gameTime, bool scanForTargets)
        {
            foreach (var ally in _allies)
            {
                if (!ally.IsActive) continue;

                // Assign targets periodically
                if (scanForTargets && NeedsNewTarget(ally))
                {
                    var target = FindBestTarget(ally, _enemies.Cast<ICombatant>());
                    if (target != null)
                    {
                        CombatSystem.AssignTarget(ally, target);
                        ally.CurrentAIState = Enemy.AISTATE.Chasing;
                    }
                }

                ally.Update(gameTime, _levelManager.MapObjects);
            }
        }

        private void UpdateEnemies(GameTime gameTime, bool scanForTargets)
        {
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsActive) continue;

                // Assign targets periodically
                if (scanForTargets && NeedsNewTarget(enemy))
                {
                    // Enemies can target player OR allies
                    var potentialTargets = new List<ICombatant> { _player };
                    potentialTargets.AddRange(_allies.Cast<ICombatant>());
                    
                    var target = FindBestTarget(enemy, potentialTargets);
                    if (target != null)
                    {
                        CombatSystem.AssignTarget(enemy, target);
                        enemy.CurrentAIState = Enemy.AISTATE.Chasing;
                    }
                }

                enemy.Update(gameTime, _levelManager.MapObjects);
            }
        }

        // ===================
        // TARGETING HELPERS
        // ===================
        
        private bool NeedsNewTarget(ICombatant combatant)
        {
            return combatant.CurrentTarget == null || 
                   !CombatSystem.IsValidTarget(combatant, combatant.CurrentTarget);
        }

        private ICombatant FindBestTarget(ICombatant seeker, IEnumerable<ICombatant> candidates)
        {
            ICombatant best = null;
            float closestDistance = float.MaxValue;

            foreach (var candidate in candidates)
            {
                // Skip invalid targets
                if (!CombatSystem.IsValidTarget(seeker, candidate))
                    continue;
                    
                // Skip over-targeted entities
                if (candidate.AttackerCount >= GameConstants.MaxAttackersPerTarget)
                    continue;

                float distance = CombatSystem.GetDistance(seeker, candidate);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        // ===================
        // CLEANUP
        // ===================
        
        private void CleanupDead()
        {
            // Remove dead allies
            _allies.RemoveAll(a => a.LifecycleState == Ally.ALLYSTATE.DEAD);
            
            // Remove dead enemies
            _enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);
        }

        private void SpawnReinforcements(CombatTeam team, int count)
        {
            Vector2 center = new Vector2(_mapSize.X / 2, _mapSize.Y / 2);
            float spawnRadius = 1800f; // Edge of the map (Trees)

            for (int i = 0; i < count; i++)
            {
                // 1. Pick random spot on edge
                float angle = CombatSystem.RandomFloat(0, MathHelper.TwoPi);
                Vector2 spawnPos = center + new Vector2(
                    (float)Math.Cos(angle) * spawnRadius,
                    (float)Math.Sin(angle) * spawnRadius
                );

                if (team == CombatTeam.Enemy)
                {
                    int roll = CombatSystem.RandomInt(0, 100);
                    int typeIndex = 0;

                    if (roll >= 90) typeIndex = 2;     
                    else if (roll >= 60) typeIndex = 1; 
                    else typeIndex = 0;                

                    var newEnemy = new Enemy(_gameOwnedBy, _allOrcTypes[typeIndex], spawnPos, 4);

                    // Apply Stats
                    if (typeIndex == 0) { newEnemy.Name = "Reinforcement Grunt"; newEnemy.AttackDamage = 10; }
                    if (typeIndex == 1) { newEnemy.Name = "Reinforcement Warrior"; newEnemy.AttackDamage = 20; }
                    if (typeIndex == 2) { newEnemy.Name = "Reinforcement Captain"; newEnemy.AttackDamage = 35; newEnemy.Scale = 3.5f; }

                    CombatSystem.AssignTarget(newEnemy, _player);
                    newEnemy.CurrentAIState = Enemy.AISTATE.Chasing;

                    _enemies.Add(newEnemy);
                }
                else if (team == CombatTeam.Player)
                {
                    var newAlly = new Ally(_gameOwnedBy, _allyTextures, spawnPos, 4);
                    newAlly.Name = "Reinforcement Soldier";

                    // Allies need to find a target, or they will wander. 
                    // Let's make them hunt the nearest enemy if possible.
                    var bestTarget = FindBestTarget(newAlly, _enemies.Cast<ICombatant>());
                    if (bestTarget != null)
                    {
                        CombatSystem.AssignTarget(newAlly, bestTarget);
                        newAlly.CurrentAIState = Enemy.AISTATE.Chasing;
                    }

                    _allies.Add(newAlly);
                }
            }
        }

        // ===================
        // DRAW
        // ===================

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // 1. DRAW BACKGROUND (Floor) FIRST
            // This is always at the very bottom.
            _levelManager.Draw(spriteBatch);

            // 2. PREPARE A SORTABLE LIST OF EVERYTHING ELSE
            // We put Player, Enemies, Allies, and Trees into one bucket.
            List<Sprite> renderList = new List<Sprite>();

            // Add Player
            if (_player.Visible) renderList.Add(_player);

            // Add Allies
            foreach (var ally in _allies)
            {
                if (ally.Visible) renderList.Add(ally);
            }

            // Add Enemies
            foreach (var enemy in _enemies)
            {
                if (enemy.Visible) renderList.Add(enemy);
            }

            // Add World Objects (Trees, Rocks) from LevelManager
            foreach (var obj in _levelManager.MapObjects)
            {
                if (obj.Visible) renderList.Add(obj);
            }

            // 3. SORT BY Y POSITION (Depth Sorting)
            // Objects with a higher Y (lower on screen) are drawn LAST (on top).
            // This allows the player to walk "behind" a tree base.
            renderList.Sort((a, b) => {
                // We compare the bottom of the sprites (Y + Height) for accuracy
                float aY = a.position.Y + (a.spriteHeight * (float)a.Scale);
                float bY = b.position.Y + (b.spriteHeight * (float)b.Scale);
                return aY.CompareTo(bY);
            });

            // 4. DRAW EVERYTHING IN ORDER
            foreach (var sprite in renderList)
            {
                sprite.Draw(spriteBatch);
            }
            //foreach (var obj in _levelManager.MapObjects)
            //{
            //    obj.DrawDebug(spriteBatch);
            //}
        }

        // ===================
        // PUBLIC ACCESSORS
        // ===================

        public Player GetPlayer() => _player;
        public int AllyCount => _allies.Count(a => a.IsAlive);
        public int EnemyCount => _enemies.Count(e => e.IsAlive);
        public bool IsBattleOver => _enemies.Count == 0 || !_player.IsAlive;
    }
}
