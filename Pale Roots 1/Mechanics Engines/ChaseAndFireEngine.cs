using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    /*
     * ChaseAndFireEngine.cs — high level runtime coordinator
     *
     * Role:
     * - This class is the central "engine" for the battle mode. It owns the level, camera,
     *   player, allied units and enemy units and coordinates updates, drawing and target assignment.
     *
     * Under the hood (concepts & runtime model):
     * - Game loop: MonoGame/XNA calls Game.Update and Game.Draw on a fixed per-frame cadence.
     *   ChaseAndFireEngine is invoked by Game1 from those callbacks. The engine receives a
     *   GameTime instance which contains elapsed time (used for deterministic time-based updates).
     *
     * - Content & resources: Texture2D and other assets are loaded through the game's Content
     *   manager and passed into entity constructors. Textures are references to GPU resources;
     *   keep them shared where possible to reduce memory and texture bindings.
     *
     * - Responsibility separation:
     *   * LevelManager owns tilemap and static/animated map objects.
     *   * Player, Enemy, Ally, WorldObject implement behavior and rendering (via AnimationManager).
     *   * CombatSystem is a static coordinator: validation, damage, attacker bookkeeping and events.
     *   * Camera builds a transformation matrix that is applied to SpriteBatch.Begin to move/zoom world.
     *
     * - Data flow:
     *   * Update() -> entity.Update(gameTime) updates physics/AI/animations.
     *   * Engine periodically scans for targets and calls CombatSystem.AssignTarget to mutate target relationships.
     *   * CombatSystem emits events (OnDamageDealt, OnCombatantKilled) that the engine subscribes to for high-level responses (spawn reinforcements, UI, sound).
     *
     * - Concurrency: MonoGame is single-threaded by default for game logic and drawing. Avoid touching shared game objects from background threads without synchronization.
     *
     * Important design notes:
     * - Entities expose small, explicit contracts (ICombatant) used by systems to remain decoupled.
     * - Depth sorting is achieved by sorting render candidates by their world Y (bottom of sprite) so objects render in correct visual order.
     * - Movement/AI use utility methods on sprites (MoveToward, SnapToFace) that check collisions using WorldObject collision boxes.
     */

    public class ChaseAndFireEngine
    {
        public bool SpawningBlocked { get; set; } = false;
        public float GlobalPlayerDamageMult { get; set; } = 1.0f;
        public float GlobalEnemyHealthMult { get; set; } = 1.0f;

        // References to major subsystems and the owning Game instance.
        // Keeping these public allows Game1 to access the camera matrix when starting SpriteBatch.
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        // Runtime entity collections owned by the engine.
        private Player _player;
        public List<Ally> _allies = new List<Ally>();
        public List<Enemy> _enemies = new List<Enemy>();

        private SpellManager _spellManager;

        // Battle state and configuration values used at runtime.
        private bool _battleStarted = false;
        private Vector2 _mapSize;
        private Vector2 _playerSpawnPos = new Vector2(500, 1230);
        private Vector2 _allySpawnOrigin = new Vector2(400, 1100);
        private Vector2 _enemySpawnOrigin = new Vector2(3200, 1230);

        // Texture dictionaries for creating animated enemies/allies.
        // Each dictionary holds the various named animation sheets (Idle, Walk, Attack, etc.).
        private List<Dictionary<string, Texture2D>> _allOrcTypes = new List<Dictionary<string, Texture2D>>();
        private Dictionary<string, Texture2D> _allyTextures = new Dictionary<string, Texture2D>();

        // Target scanning timer: we scan for targets at intervals, not every frame.
        private float _targetingTimer = 0f;

        // Simple counters exposed for UI and telemetry.
        public int EnemiesKilled { get; private set; }
        public int AlliesLost { get; private set; }

        /*
         * Constructor
         *
         * What it does:
         * - Stores reference to Game for resource access.
         * - Initializes the tilemap and world via LevelManager.LoadLevel.
         * - Creates the player and sets up the Camera.
         * - Loads texture atlases for allies and several types of enemies, then creates initial teams.
         * - Subscribes to CombatSystem events that the engine cares about.
         *
         * Why this is here:
         * - The engine must create the initial world and its actors after the game's Content system exists.
         */
        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            _mapSize = GameConstants.DefaultMapSize;

            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            // Player creation: the Player constructor expects textures already loaded from Content.
            _player = new Player(
                game,
                game.Content.Load<Texture2D>("wizard_strip3"),
                _playerSpawnPos,
                3
            );
            _player.Name = "Hero";

            // Camera setup: the engine sets an initial view centered on the map.
            _camera = new Camera(Vector2.Zero, _mapSize);
            Viewport vp = game.GraphicsDevice.Viewport;

            // Choose a zoom that fits the world to the viewport while preserving aspect ratio.
            float scaleX = (float)vp.Width / _mapSize.X;
            float scaleY = (float)vp.Height / _mapSize.Y;
            _camera.Zoom = Math.Min(scaleX, scaleY);

            // LookAt calculates and stores the camera translation matrix (used by SpriteBatch.Begin).
            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);

            Texture2D txSmite = game.Content.Load<Texture2D>("Effects/Smite_spritesheet");
            Texture2D txNova = game.Content.Load<Texture2D>("Effects/HolyNova_spritesheet");
            Texture2D txFury = game.Content.Load<Texture2D>("Effects/HeavensFury_spritesheet");
            Texture2D txShield = game.Content.Load<Texture2D>("Effects/HolyShield_spritesheet");
            // Note: I am guessing the names based on your screenshot. 
            // If "Electricity" uses a different file, swap it here.
            Texture2D txElectric = game.Content.Load<Texture2D>("Effects/Sprite-sheet"); // Placeholder if you don't have electric sheet yet
            Texture2D txJustice = game.Content.Load<Texture2D>("Effects/SwordOfJustice_spritesheet");

            // 2. Initialize Manager with all textures
            _spellManager = new SpellManager(this, txSmite, txNova, txFury, txShield, txElectric, txJustice);

            // Load armies and register event handlers.
            InitializeArmies();
            SetupCombatEvents();
        }

        /*
         * InitializeArmies
         *
         * Responsibilities:
         * - Load sets of textures for enemy "types" and a shared atlas for allies.
         * - Create initial ally instances and populate the enemy list using CreateEnemyFormation.
         *
         * Implementation notes:
         * - Textures are stored in dictionaries keyed by animation name; entities then construct Animation objects using them.
         * - Reuse the same dictionary for every instance of the same type to keep memory and GPU bindings efficient.
         */
        private void InitializeArmies()
        {
            // Load multiple orc type texture dictionaries (orcs 1..3)
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

            // Ally textures are loaded once and shared across Ally instances.
            _allyTextures["Walk"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Walk");
            _allyTextures["Attack"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Slash");
            _allyTextures["Idle"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Idle");

            // Create allies in a vertical formation (columns) and store them in the _allies list.
            // The engine owns these references and will Update/Draw them each frame.
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = _allySpawnOrigin + new Vector2(0, i * 100);
                var ally = new Ally(_gameOwnedBy, _allyTextures, pos, 4);
                ally.Name = $"Soldier {i + 1}";
                _allies.Add(ally);
            }

            // Create a formation of enemies.
            CreateEnemyFormation(10);
        }

        /*
         * CreateEnemyFormation
         *
         * Algorithm:
         * - Arrange enemies in a simple triangular formation using row/slot counters.
         * - Pick an enemy "type" (texture dictionary) and set per-type stats (name, attack damage).
         *
         * Practical notes:
         * - Spawning uses world coordinates not tile indices; spacing constants determine visual density.
         * - The Enemy object handles its own animation and AI; the engine simply holds references.
         */
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

                // Guarantee a tougher leader at index 0 for variety.
                if (i == 0)
                {
                    typeIndex = 2;
                }
                else
                {
                    // RandomInt(maxExclusive) pattern: returns 0..(max-1)
                    typeIndex = CombatSystem.RandomInt(0, 2);
                }

                var enemy = new Enemy(_gameOwnedBy, _allOrcTypes[typeIndex], new Vector2(xPos, yPos), 4);

                // Apply type-specific stats that alter gameplay behavior.
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

        /*
         * SetupCombatEvents
         *
         * Event-driven design:
         * - CombatSystem exposes events that other systems can subscribe to.
         * - Here the engine subscribes to OnCombatantKilled and OnDamageDealt to update counters and spawn reinforcements.
         *
         * Reasoning:
         * - Centralizing combat resolution in CombatSystem keeps rules consistent.
         * - Events decouple effects (spawn, sounds, UI) from the core damage code path.
         */
        private void SetupCombatEvents()
        {
            CombatSystem.OnCombatantKilled += (killer, victim) =>
            {
                if (victim.Team == CombatTeam.Enemy)
                {
                    EnemiesKilled++;
                    // When an enemy dies, spawn reinforcements to keep the fight dynamic.
                    SpawnReinforcements(CombatTeam.Enemy, 2);
                }
                else if (victim.Team == CombatTeam.Player && victim != _player)
                {
                    AlliesLost++;
                    SpawnReinforcements(CombatTeam.Player, 2);
                }
            };

            // OnDamageDealt can be used for VFX/SFX/UI. Keep the handler light; heavy work should be asynchronous.
            CombatSystem.OnDamageDealt += (attacker, target, damage) =>
            {
                // Hook point: play hit sound, show floating text, accumulate analytics, etc.
            };
        }

        /*
         * Update — called each frame by Game1
         *
         * Responsibilities:
         * - Decide high-level mode (pre-battle movement or battle logic).
         * - Update the player and, when in battle, forward to UpdateBattle which orchestrates all actors.
         *
         * Key API use:
         * - Keyboard.GetState: immediate-mode input reading; keep polling code centralized to avoid input duplication.
         */
        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;
                    
            if (!_battleStarted)
            {
                // Pre-battle: allow the player to move around the level and inspect the world.
                _player.Update(gameTime, _levelManager.CurrentLevel, _enemies);

                // Starting battle is a simple input gate for this demo (press D).
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

        /*
         * UpdateBattle
         *
         * This is the per-frame update for the active battle:
         * - Smooth camera zooming using linear interpolation (Lerp).
         * - Update player, level objects, allies and enemies.
         * - Perform periodic target scans rather than checking every frame for performance and to simulate perception intervals.
         * - Clean up dead entities and keep the camera focused on the player.
         *
         * Time-based operations:
         * - Convert GameTime.ElapsedGameTime to milliseconds to keep timing consistent across platforms.
         */
        private void UpdateBattle(GameTime gameTime, Viewport vp)
        {
            // Smoothly lerp the camera's zoom toward 1.0 (battle zoom) using MathHelper.Lerp.
            _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

            // Always update the player first so allies/enemies can respond to the player's new state the same frame.
            _player.Update(gameTime, _levelManager.CurrentLevel, _enemies);

            // LevelManager updates animated world objects and enemy references it owns.
            _levelManager.Update(gameTime, _player);

            // Target scanning: accumulate time and only run a full scan at defined intervals.
            _targetingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            bool scanNow = _targetingTimer >= GameConstants.TargetScanInterval;
            if (scanNow) _targetingTimer = 0;

            _spellManager.Update(gameTime);

            UpdateAllies(gameTime, scanNow);
            UpdateEnemies(gameTime, scanNow);

            CleanupDead();

            // Update camera matrix so rendering uses the player's position this frame.
            _camera.follow(_player.CentrePos, vp);
        }

        /*
         * UpdateAllies
         *
         * For each ally:
         * - If it's time to scan for targets, use FindBestTarget to assign a target through CombatSystem.
         * - Then call ally.Update(...) to run AI, animation, and movement logic.
         *
         * Note: CombatSystem.AssignTarget maintains attacker bookkeeping (AttackerCount) so the engine doesn't need to.
         */
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

        /*
         * UpdateEnemies
         *
         * For each enemy:
         * - Optionally assign a new target (player or allies) using FindBestTarget.
         * - Update the enemy with the current list of world obstacles for collision-aware movement.
         */
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

        /*
         * NeedsNewTarget
         *
         * Decouples the validation logic from Update loops: a combatant needs a new target if it has none or the current one is invalid.
         * Uses CombatSystem.IsValidTarget which checks life/active state and team relationships.
         */
        private bool NeedsNewTarget(ICombatant combatant)
        {
            return combatant.CurrentTarget == null ||
                   !CombatSystem.IsValidTarget(combatant, combatant.CurrentTarget);
        }

        /*
         * FindBestTarget
         *
         * Target selection algorithm:
         * - Iterate candidates and skip invalid ones (dead, neutral, not an enemy).
         * - Skip candidates already under too many attackers (MaxAttackersPerTarget) to avoid overkill.
         * - Select the closest valid candidate using CombatSystem.GetDistance which uses Center points.
         *
         * This is a greedy nearest-neighbor selection — fast and predictable.
         */
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

        /*
         * CleanupDead
         *
         * Remove dead entities from runtime lists to avoid updating/drawing them further.
         * Lists use RemoveAll with predicate to keep code concise and avoid manual iteration removal problems.
         */
        private void CleanupDead()
        {
            _allies.RemoveAll(a => a.LifecycleState == Ally.ALLYSTATE.DEAD);
            _enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);
        }

        /*
         * SpawnReinforcements
         *
         * Spawns either enemies or allies around the map edge:
         * - Chooses a point on a circle (center + cos/sin * radius) to place reinforcements.
         * - Randomly picks a type (rarity distribution) for enemies and assigns immediate behavior (hunt player).
         * - For allies, attempts to find a nearby enemy to attack; otherwise they will wander.
         *
         * Under the hood:
         * - Uses CombatSystem.RandomFloat/RandomInt for deterministic-seeming randomness via a single shared RNG instance.
         * - Immediately assigns targets with CombatSystem.AssignTarget so attacker bookkeeping is consistent.
         */
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

                        // Force immediate aggression: assign player as target and set AI state to chase.
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

        /*
         * Draw
         *
         * Rendering pipeline:
         * - Level tiles are drawn first by LevelManager.Draw.
         * - Everything else (player, allies, enemies, world objects) is gathered into a single list then depth-sorted by bottom Y.
         * - Sorting by Y makes entities closer to the "bottom" of the screen draw last, producing a simple but effective depth illusion.
         *
         * Practical notes:
         * - The Camera's matrix should already be applied to SpriteBatch.Begin by the caller to ensure world coordinates transform to screen.
         * - Entities implement their own Draw methods which ultimately call SpriteBatch.Draw with their current source rectangle and origin.
         */
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the base tile layer first.
            _levelManager.Draw(spriteBatch);

            // Gather renderable objects into one list for depth sorting.
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

            // Depth sort by sprite bottom Y (position.Y + spriteHeight * scale). This produces painter's algorithm ordering.
            renderList.Sort((a, b) =>
            {
                float aY = a.position.Y + (a.spriteHeight * (float)a.Scale);
                float bY = b.position.Y + (b.spriteHeight * (float)b.Scale);
                return aY.CompareTo(bY);
            });

            // Draw every sprite in sorted order. Each sprite's Draw handles its own animation and local origin.
            foreach (var sprite in renderList)
            {
                sprite.Draw(spriteBatch);
            }
            _spellManager.Draw(spriteBatch);
        }

        // Public accessors the rest of the game can query for status.
        public Player GetPlayer() => _player;
        public int AllyCount => _allies.Count(a => a.IsAlive);
        public int EnemyCount => _enemies.Count(e => e.IsAlive);
        public bool IsBattleOver => _enemies.Count == 0 || !_player.IsAlive;
    }
}
