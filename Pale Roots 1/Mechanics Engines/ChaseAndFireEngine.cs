using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        private Vector2 _playerSpawnPos = new Vector2(300, 900);
        private Vector2 _allySpawnOrigin = new Vector2(150, 780);
        private Vector2 _enemySpawnOrigin = new Vector2(1600, 900);

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
                swordTx,
                _playerSpawnPos, 
                3
            );
            _player.Name = "Hero";

            // Setup camera
            _camera = new Camera(Vector2.Zero, _mapSize);
            Viewport vp = game.GraphicsDevice.Viewport;
            _camera.Zoom = (float)vp.Width / _mapSize.X;
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
            Texture2D allyTx = _gameOwnedBy.Content.Load<Texture2D>("wizard_strip3");
            Texture2D enemyTx = _gameOwnedBy.Content.Load<Texture2D>("Dragon_strip3");

            // Create 5 allies in a column
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = _allySpawnOrigin + new Vector2(0, i * 60);
                var ally = new Ally(_gameOwnedBy, allyTx, pos, 3);
                ally.Name = $"Soldier {i + 1}";
                _allies.Add(ally);
            }

            // Create 10 enemies in a triangle formation
            CreateEnemyFormation(enemyTx, 10);
        }

        private void CreateEnemyFormation(Texture2D texture, int count)
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

                var enemy = new Enemy(_gameOwnedBy, texture, new Vector2(xPos, yPos), 3);
                enemy.Name = $"Dragon {i + 1}";
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
            // Track kills
            CombatSystem.OnCombatantKilled += (killer, victim) =>
            {
                if (victim.Team == CombatTeam.Enemy)
                    EnemiesKilled++;
                else if (victim.Team == CombatTeam.Player && victim != _player)
                    AlliesLost++;
            };

            // Could add more events: damage numbers, sounds, particles, etc.
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
            _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

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

                ally.Update(gameTime);
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

                enemy.Update(gameTime);
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
