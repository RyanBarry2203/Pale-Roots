using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    // Engine that runs the battle mode: owns level, camera, player/allies/enemies and coordinates update/draw/targeting.
    public class ChaseAndFireEngine
    {
        // Simple runtime tuning flags
        public bool SpawningBlocked { get; set; } = false;
        public float GlobalPlayerDamageMult { get; set; } = 1.0f;
        public float GlobalEnemyHealthMult { get; set; } = 1.0f;

        // Major subsystems (public so Game1 can access camera matrix before drawing)
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        // Runtime entity lists
        private Player _player;
        public List<Ally> _allies = new List<Ally>();
        public List<Enemy> _enemies = new List<Enemy>();

        private SpellManager _spellManager;

        // Battle state + spawn/origin positions
        private bool _battleStarted = false;
        private Vector2 _mapSize;
        private Vector2 _playerSpawnPos = new Vector2(500, 1230);
        private Vector2 _allySpawnOrigin = new Vector2(400, 1100);
        private Vector2 _enemySpawnOrigin = new Vector2(3200, 1230);

        // Shared texture dictionaries for enemy/ally types
        private List<Dictionary<string, Texture2D>> _allOrcTypes = new List<Dictionary<string, Texture2D>>();
        private Dictionary<string, Texture2D> _allyTextures = new Dictionary<string, Texture2D>();

        // Target-scan timer (we do perception checks at intervals)
        private float _targetingTimer = 0f;

        // Lightweight counters for UI
        public int EnemiesKilled { get; private set; }
        public int AlliesLost { get; private set; }

        // Constructor: create level, player, camera, load spells and armies.
        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            _mapSize = GameConstants.DefaultMapSize;

            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            // Create player (textures are loaded from Content by Game1 and passed here)
            _player = new Player(
                game,
                game.Content.Load<Texture2D>("wizard_strip3"),
                _playerSpawnPos,
                3
            );
            _player.Name = "Hero";

            // Camera: center on map and set sane zoom to fit viewport
            _camera = new Camera(Vector2.Zero, _mapSize);
            Viewport vp = game.GraphicsDevice.Viewport;
            float scaleX = (float)vp.Width / _mapSize.X;
            float scaleY = (float)vp.Height / _mapSize.Y;
            _camera.Zoom = Math.Min(scaleX, scaleY);
            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);

            // Load spell effect atlases and create the SpellManager
            Texture2D txSmite = game.Content.Load<Texture2D>("Effects/Smite_spritesheet");
            Texture2D txNova = game.Content.Load<Texture2D>("Effects/HolyNova_spritesheet");
            Texture2D txFury = game.Content.Load<Texture2D>("Effects/HeavensFury_spritesheet");
            Texture2D txShield = game.Content.Load<Texture2D>("Effects/HolyShield_spritesheet");
            Texture2D txElectric = game.Content.Load<Texture2D>("Effects/Sprite-sheet"); // placeholder name
            Texture2D txJustice = game.Content.Load<Texture2D>("Effects/SwordOfJustice_spritesheet");

            _spellManager = new SpellManager(this, txSmite, txNova, txFury, txShield, txElectric, txJustice);

            // Prepare armies and wire event handlers
            InitializeArmies();
            SetupCombatEvents();
        }

        // Load enemy/ally texture sets and create initial units
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

            // Ally shared atlas
            _allyTextures["Walk"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Walk");
            _allyTextures["Attack"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Slash");
            _allyTextures["Idle"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Idle");

            // Spawn a column of allies
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = _allySpawnOrigin + new Vector2(0, i * 100);
                var ally = new Ally(_gameOwnedBy, _allyTextures, pos, 4);
                ally.Name = $"Soldier {i + 1}";
                _allies.Add(ally);
            }

            // Spawn initial enemy formation
            CreateEnemyFormation(10);
        }

        // Arrange enemies in a simple triangular formation and apply type stats.
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

                // Leader is tougher, others randomized
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

        // Subscribe to CombatSystem events to react to kills and damage.
        private void SetupCombatEvents()
        {
            CombatSystem.OnCombatantKilled += (killer, victim) =>
            {
                if (victim.Team == CombatTeam.Enemy)
                {
                    EnemiesKilled++;
                    // Spawn small reinforcements for pacing
                    SpawnReinforcements(CombatTeam.Enemy, 2);
                }
                else if (victim.Team == CombatTeam.Player && victim != _player)
                {
                    AlliesLost++;
                    SpawnReinforcements(CombatTeam.Player, 2);
                }
            };

            CombatSystem.OnDamageDealt += (attacker, target, damage) =>
            {
                // Lightweight hook for SFX/VFX/analytics; keep handlers small.
            };
        }

        // Per-frame update called by Game1.Update
        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;

            if (!_battleStarted)
            {
                // Pre-battle: let player roam and inspect the level
                _player.Update(gameTime, _levelManager.CurrentLevel, _enemies);

                // Quick demo input to start battle
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

        // Main battle loop: update camera, player, level, spells, allies and enemies.
        private void UpdateBattle(GameTime gameTime, Viewport vp)
        {
            // Smoothly move camera zoom toward 1.0
            _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

            // Update player first so others react to player's new state this same frame
            _player.Update(gameTime, _levelManager.CurrentLevel, _enemies);

            // Update level objects
            _levelManager.Update(gameTime, _player);

            _player.DamageMultiplier = GlobalPlayerDamageMult;

            // Handle periodic target scanning
            _targetingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            bool scanNow = _targetingTimer >= GameConstants.TargetScanInterval;
            if (scanNow) _targetingTimer = 0;

            _spellManager.Update(gameTime);

            UpdateAllies(gameTime, scanNow);
            UpdateEnemies(gameTime, scanNow);

            CleanupDead();

            // Keep camera centered on the player this frame
            _camera.follow(_player.CentrePos, vp);
        }

        // Update allies: optionally find targets then update AI/animations
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
                        ally.CurrentAIState = Enemy.AISTATE.Chasing;
                    }
                }

                ally.Update(gameTime, _levelManager.MapObjects);
            }
        }

        // Update enemies: optionally assign target (player or allies) then update with map obstacles
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
                        enemy.CurrentAIState = Enemy.AISTATE.Chasing;
                    }
                }

                enemy.Update(gameTime, _levelManager.MapObjects);
            }
        }

        // Simple validity check: needs a new target if none or current target is invalid
        private bool NeedsNewTarget(ICombatant combatant)
        {
            return combatant.CurrentTarget == null ||
                   !CombatSystem.IsValidTarget(combatant, combatant.CurrentTarget);
        }

        // Greedy nearest-target selection that respects max attackers per target
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

        // Remove dead entries from runtime lists
        private void CleanupDead()
        {
            _allies.RemoveAll(a => a.LifecycleState == Ally.ALLYSTATE.DEAD);
            _enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);
        }

        // Spawn reinforcements around the map (uses shared RNG utilities)
        private void SpawnReinforcements(CombatTeam team, int count)
        {
            Vector2 center = new Vector2(_mapSize.X / 2, _mapSize.Y / 2);
            float spawnRadius = 1800f;

            if (!SpawningBlocked)
            {
                for (int i = 0; i < count; i++)
                {
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

                        if (typeIndex == 0) { newEnemy.Name = "Reinforcement Grunt"; newEnemy.AttackDamage = 10; }
                        if (typeIndex == 1) { newEnemy.Name = "Reinforcement Warrior"; newEnemy.AttackDamage = 20; }
                        if (typeIndex == 2) { newEnemy.Name = "Reinforcement Captain"; newEnemy.AttackDamage = 35; newEnemy.Scale = 3.5f; }

                        // Immediately assign player as target so they behave aggressively on spawn
                        CombatSystem.AssignTarget(newEnemy, _player);
                        newEnemy.CurrentAIState = Enemy.AISTATE.Chasing;

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
                            newAlly.CurrentAIState = Enemy.AISTATE.Chasing;
                        }

                        _allies.Add(newAlly);
                    }
                }
            }
        }

        // Expose SpellManager for external UI or systems
        public SpellManager GetSpellManager() => _spellManager;

        // Draw: level first, then gather sprites, sort by bottom-Y and draw in that order.
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

            // Painter's algorithm ordering by sprite bottom Y for simple depth illusion
            renderList.Sort((a, b) =>
            {
                float aY = a.position.Y + (a.spriteHeight * (float)a.Scale);
                float bY = b.position.Y + (b.spriteHeight * (float)b.Scale);
                return aY.CompareTo(bY);
            });

            foreach (var sprite in renderList)
            {
                sprite.Draw(spriteBatch);
            }
            _spellManager.Draw(spriteBatch);
        }

        // Small helpers for external queries
        public Player GetPlayer() => _player;
        public int AllyCount => _allies.Count(a => a.IsAlive);
        public int EnemyCount => _enemies.Count(e => e.IsAlive);
        public bool IsBattleOver => _enemies.Count == 0 || !_player.IsAlive;
    }
}
