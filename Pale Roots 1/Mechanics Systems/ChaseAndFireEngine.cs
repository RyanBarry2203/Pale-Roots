using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    // Engine that runs the battle mode and coordinates level, camera, player, allies, and enemies.
    // abit of a god class but not as bad as it was before the refactor,
    // and it centralizes the battle logic in one place instead of spreading it across the player and enemy classes.
    public class ChaseAndFireEngine
    {
        // Runtime tuning flags.
        public bool SpawningBlocked { get; set; } = false;
        public float GlobalPlayerDamageMult { get; set; } = 1.0f;
        public float GlobalEnemyHealthMult { get; set; } = 1.0f;
        public bool IsBossArena { get; set; } = false;

        // Major subsystems accessible to the game.
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        // Rendering pipeline.
        public RenderPipeline Renderer { get; private set; }

        // Runtime entity lists.
        private Player _player;
        public List<Ally> _allies = new List<Ally>();
        public List<Enemy> _enemies = new List<Enemy>();
        private SpellManager _spellManager;

        // Battle state and spawn positions.
        private bool _battleStarted = false;
        private Vector2 _mapSize;
        private Vector2 _playerSpawnPos = new Vector2(500, 1230);
        private Vector2 _allySpawnOrigin = new Vector2(400, 1100);
        private Vector2 _enemySpawnOrigin = new Vector2(3200, 1230);

        private List<Dictionary<string, Texture2D>> _allOrcTypes = new List<Dictionary<string, Texture2D>>();
        private Dictionary<string, Texture2D> _allyTextures = new Dictionary<string, Texture2D>();

        // Timer used to pace target scanning.
        private float _targetingTimer = 0f;

        // Lightweight counters for UI.
        public int EnemiesKilled { get; private set; }
        public int AlliesLost { get; private set; }

        // Constructor initializes level, player, camera, spells, and armies.
        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            _mapSize = GameConstants.DefaultMapSize;

            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            // Initialize rendering API.
            Renderer = new RenderPipeline();

            // Create player and set name.
            _player = new Player(
                game,
                game.Content.Load<Texture2D>("wizard_strip3"),
                _playerSpawnPos,
                3
            );
            _player.Name = "Hero";

            // Configure camera to center on the map.
            _camera = new Camera(Vector2.Zero, _mapSize);
            Viewport vp = game.GraphicsDevice.Viewport;
            float scaleX = (float)vp.Width / _mapSize.X;
            float scaleY = (float)vp.Height / _mapSize.Y;
            _camera.Zoom = Math.Min(scaleX, scaleY);
            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);

            // Load spell textures and create the spell manager.
            Texture2D txSmite = game.Content.Load<Texture2D>("Effects/Smite_spritesheet");
            Texture2D txNova = game.Content.Load<Texture2D>("Effects/HolyNova_spritesheet");
            Texture2D txFury = game.Content.Load<Texture2D>("Effects/HeavensFury_spritesheet");
            Texture2D txShield = game.Content.Load<Texture2D>("Effects/HolyShield_spritesheet");
            Texture2D txElectric = game.Content.Load<Texture2D>("Effects/Sprite-sheet");
            Texture2D txJustice = game.Content.Load<Texture2D>("Effects/SwordOfJustice_spritesheet");

            _spellManager = new SpellManager(this, txSmite, txNova, txFury, txShield, txElectric, txJustice);

            // Prepare armies
            InitializeArmies();
            SetupCombatEvents();
        }

        // Load enemy and ally texture sets and create initial units.
        private void InitializeArmies()
        {
            for (int i = 1; i <= 3; i++)
            {
                var newOrcDict = new Dictionary<string, Texture2D>();
                newOrcDict["Idle"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_idle_full");
                newOrcDict["Walk"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_run_full");
                newOrcDict["Attack"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_attack_full");
                newOrcDict["Hurt"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_hurt_full");
                newOrcDict["Death"] = _gameOwnedBy.Content.Load<Texture2D>($"RealEnemyFolder/orc{i}_death_full");
                _allOrcTypes.Add(newOrcDict);
            }

            // Load ally textures.
            _allyTextures["Walk"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Walk");
            _allyTextures["Attack"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Slash");
            _allyTextures["Idle"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Idle");

            // Spawn initial allies.
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = _allySpawnOrigin + new Vector2(0, i * 100);
                var ally = new Ally(_gameOwnedBy, _allyTextures, pos, 4);
                ally.Name = $"Soldier {i + 1}";
                _allies.Add(ally);
            }

            // Spawn initial enemy formation.
            CreateEnemyFormation(10);
        }

        // Arrange enemies in a triangular formation and apply type specific stats.
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

                // set leader and randomize others.
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

        // combat events to handle kills and friendly losses.
        private void SetupCombatEvents()
        {
            // React to the global combatant killed event.
            CombatSystem.OnCombatantKilled += (killer, victim) =>
            {
                // If an enemy died, increment kills and spawn reinforcements.
                if (victim.Team == CombatTeam.Enemy)
                {
                    EnemiesKilled++;
                    int spawnCount = (CombatSystem.RandomInt(0, 100) < 85) ? 1 : 2;
                    SpawnReinforcements(CombatTeam.Enemy, spawnCount);
                }
                // If an ally died, track the loss and spawn new allies.
                else if (victim.Team == CombatTeam.Player && victim != _player)
                {
                    AlliesLost++;
                    SpawnReinforcements(CombatTeam.Player, CombatSystem.RandomInt(1, 3));
                }
            };
        }

        // update called by Game1.
        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;

            // If battle has not started, allow free player movement.
            if (!_battleStarted)
            {
                _player.Update(gameTime, _levelManager.CurrentLevel, _enemies, _levelManager.MapObjects);

                // Debug key to start the battle.
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    _battleStarted = true;
                }
            }
            else
            {
                // Run the main battle update logic.
                UpdateBattle(gameTime, vp);
            }
        }

        // Main battle loop that updates camera, player, level, spells, allies, and enemies.
        private void UpdateBattle(GameTime gameTime, Viewport vp)
        {
            // Smoothly approach target zoom.
            _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

            // Update player first so others react this frame.
            _player.Update(gameTime, _levelManager.CurrentLevel, _enemies, _levelManager.MapObjects);

            // Update level and assign player reference where needed.
            _levelManager.Update(gameTime, _player);

            _player.DamageMultiplier = GlobalPlayerDamageMult;

            // Handle periodic target scanning.
            _targetingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            bool scanNow = _targetingTimer >= GameConstants.TargetScanInterval;
            if (scanNow) _targetingTimer = 0;

            _spellManager.Update(gameTime);
            UpdateAllies(gameTime, scanNow);
            UpdateEnemies(gameTime, scanNow);
            CleanupDead();

            // Auto spawn enemies if none remain and spawning is allowed.
            if (_enemies.Count == 0 && !SpawningBlocked)
            {
                SpawnReinforcements(CombatTeam.Enemy, 5);
            }

            // Keep the camera centered on the player.
            _camera.follow(_player.CentrePos, vp);
        }

        // Update allies and assign targets when needed.
        private void UpdateAllies(GameTime gameTime, bool scanForTargets)
        {
            foreach (var ally in _allies)
            {
                if (!ally.IsActive) continue;

                if (scanForTargets && NeedsNewTarget(ally))
                {
                    var target = FindBestTarget(ally, _enemies.Cast<ICombatant>());
                    if (target != null)
                    {
                        CombatSystem.AssignTarget(ally, target);
                        // switch the ally into chase behavior.
                        ally.ChangeState(new ChaseState());
                    }
                }
                ally.Update(gameTime, _levelManager.MapObjects);
            }
        }

        // Update enemies and assign targets when appropriate.
        private void UpdateEnemies(GameTime gameTime, bool scanForTargets)
        {
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsActive) continue;

                if (scanForTargets && NeedsNewTarget(enemy))
                {
                    var potentialTargets = new List<ICombatant> { _player };
                    potentialTargets.AddRange(_allies.Cast<ICombatant>());

                    var target = FindBestTarget(enemy, potentialTargets);
                    if (target != null)
                    {
                        CombatSystem.AssignTarget(enemy, target);
                        // switch the enemy into chase behavior.
                        enemy.ChangeState(new ChaseState());
                    }
                }
                enemy.Update(gameTime, _levelManager.MapObjects);
            }
        }

        // Determine if a combatant needs a new target.
        private bool NeedsNewTarget(ICombatant combatant)
        {
            return combatant.CurrentTarget == null ||
                   !CombatSystem.IsValidTarget(combatant, combatant.CurrentTarget);
        }

        // Select the nearest valid target that is not already saturated with attackers.
        private ICombatant FindBestTarget(ICombatant seeker, IEnumerable<ICombatant> candidates)
        {
            ICombatant best = null;
            float closestDistance = float.MaxValue;

            foreach (var candidate in candidates)
            {
                if (!CombatSystem.IsValidTarget(seeker, candidate))
                    continue;

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

        // Remove dead entries from lists.
        private void CleanupDead()
        {
            _allies.RemoveAll(a => a.LifecycleState == Ally.ALLYSTATE.DEAD);
            _enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);
        }

        // Spawn reinforcements around the player at a random offscreen position.
        private void SpawnReinforcements(CombatTeam team, int count)
        {
            if (!SpawningBlocked)
            {
                for (int i = 0; i < count; i++)
                {
                    // pick random angle and distance for offscreen spawn.
                    float angle = CombatSystem.RandomFloat(0, MathHelper.TwoPi);
                    float distance = CombatSystem.RandomFloat(900f, 1300f);

                    Vector2 spawnPos = _player.Position + new Vector2(
                        (float)Math.Cos(angle) * distance,
                        (float)Math.Sin(angle) * distance
                    );

                    // Clamp spawn inside the map edges.
                    float safeMarginX = 300f;
                    float safeMarginY = 600f;
                    spawnPos.X = MathHelper.Clamp(spawnPos.X, safeMarginX, _mapSize.X - safeMarginX);
                    spawnPos.Y = MathHelper.Clamp(spawnPos.Y, safeMarginY, _mapSize.Y - safeMarginY);

                    if (team == CombatTeam.Enemy)
                    {
                        int roll = CombatSystem.RandomInt(0, 100);
                        int typeIndex = 0;

                        if (roll >= 90) typeIndex = 2;
                        else if (roll >= 60) typeIndex = 1;
                        else typeIndex = 0;

                        var newEnemy = new Enemy(_gameOwnedBy, _allOrcTypes[typeIndex], spawnPos, 4);

                        if (typeIndex == 0) { newEnemy.Name = "Reinforcement Grunt"; newEnemy.AttackDamage = 10; }
                        if (typeIndex == 1) { newEnemy.Name = "Reinforcement Warrior"; newEnemy.AttackDamage = 20; }
                        if (typeIndex == 2) { newEnemy.Name = "Reinforcement Captain"; newEnemy.AttackDamage = 35; newEnemy.Scale = 3.5f; }

                        // Lock them onto the player and start chasing.
                        CombatSystem.AssignTarget(newEnemy, _player);
                        newEnemy.ChangeState(new ChaseState());
                        _enemies.Add(newEnemy);
                    }
                    else if (team == CombatTeam.Player)
                    {
                        var newAlly = new Ally(_gameOwnedBy, _allyTextures, spawnPos, 4);
                        newAlly.Name = "Reinforcement Soldier";

                        var bestTarget = FindBestTarget(newAlly, _enemies.Cast<ICombatant>());
                        if (bestTarget != null)
                        {
                            CombatSystem.AssignTarget(newAlly, bestTarget);
                            newAlly.ChangeState(new ChaseState());
                        }
                        else
                        {
                            newAlly.ChangeState(new WanderState());
                        }
                        _allies.Add(newAlly);
                    }
                }
            }
        }
        public SpellManager GetSpellManager() => _spellManager;

        // Draw level then sprites sorted by depth and finally spell effects.
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _levelManager.Draw(spriteBatch);

            List<Sprite> renderList = new List<Sprite>();

            if (_player.Visible) renderList.Add(_player);

            foreach (var ally in _allies)
            {
                if (ally.Visible) renderList.Add(ally);
            }

            foreach (var enemy in _enemies)
            {
                if (enemy.Visible) renderList.Add(enemy);
            }

            foreach (var obj in _levelManager.MapObjects)
            {
                if (obj.Visible) renderList.Add(obj);
            }

            // Use the render pipeline to draw depth sorted sprites.
            Renderer.DrawDepthSorted(spriteBatch, renderList);

            _spellManager.Draw(spriteBatch);
        }

        // Helper queries.
        public Player GetPlayer() => _player;
        public int AllyCount => _allies.Count(a => a.IsAlive);
        public int EnemyCount => _enemies.Count(e => e.IsAlive);
        public bool IsBattleOver => _enemies.Count == 0 || !_player.IsAlive;
    }
}