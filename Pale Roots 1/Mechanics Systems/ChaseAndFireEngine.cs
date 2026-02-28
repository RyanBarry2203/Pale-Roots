using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    // This is the core engine that runs the actual gameplay.
    // It owns the level, the camera, the player, and all the NPC armies. It coordinates the update loop, 
    // handles the AI targeting logic, and manages the drawing pipeline.
    public class ChaseAndFireEngine
    {
        // Simple flags that can be toggled by other states (like BossBattleState) to alter the rules of the game on the fly.
        public bool SpawningBlocked { get; set; } = false;
        public float GlobalPlayerDamageMult { get; set; } = 1.0f;
        public float GlobalEnemyHealthMult { get; set; } = 1.0f;
        public bool IsBossArena { get; set; } = false;

        // Major subsystems that the engine relies on to function.
        // These are public so the parent Game1 or drawing states can access the camera matrix and map data.
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        public RenderPipeline Renderer { get; private set; }

        // Master lists holding every physical character currently alive in the game world.
        private Player _player;
        public List<Ally> _allies = new List<Ally>();
        public List<Enemy> _enemies = new List<Enemy>();
        private SpellManager _spellManager;

        // Variables controlling the initial setup and layout of the battlefield.
        private bool _battleStarted = false;
        private Vector2 _mapSize;
        private Vector2 _playerSpawnPos = new Vector2(500, 1230);
        private Vector2 _allySpawnOrigin = new Vector2(400, 1100);
        private Vector2 _enemySpawnOrigin = new Vector2(3200, 1230);

        // We load the massive sprite sheets for all characters exactly once here, 
        // then pass references to the individual characters when they spawn to save memory.
        private List<Dictionary<string, Texture2D>> _allOrcTypes = new List<Dictionary<string, Texture2D>>();
        private Dictionary<string, Texture2D> _allyTextures = new Dictionary<string, Texture2D>();

        // An internal timer used to limit how often the AI recalculates its targets. 
        // Doing complex distance math for 50 characters every single frame would lag the game heavily.
        private float _targetingTimer = 0f;

        // Simple trackers that the UIManager reads to display the progress bar.
        public int EnemiesKilled { get; private set; }
        public int AlliesLost { get; private set; }

        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            _mapSize = GameConstants.DefaultMapSize;

            // Generate the physical map boundaries and procedural set dressing.
            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            // Initialize our custom depth-sorting rendering API.
            Renderer = new RenderPipeline();

            // Instantiate the player character, loading their specific sprite sheet directly from Content.
            _player = new Player(
                game,
                game.Content.Load<Texture2D>("wizard_strip3"),
                _playerSpawnPos,
                3
            );
            _player.Name = "Hero";

            // Initialize the camera and instantly snap it to the center of the map.
            // We mathematically calculate the zoom scale based on the user's monitor resolution so the map always fits the screen perfectly.
            _camera = new Camera(Vector2.Zero, _mapSize);
            Viewport vp = game.GraphicsDevice.Viewport;
            float scaleX = (float)vp.Width / _mapSize.X;
            float scaleY = (float)vp.Height / _mapSize.Y;
            _camera.Zoom = Math.Min(scaleX, scaleY);
            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);

            // Load the massive visual effects sprite sheets required for the magic system.
            Texture2D txSmite = game.Content.Load<Texture2D>("Effects/Smite_spritesheet");
            Texture2D txNova = game.Content.Load<Texture2D>("Effects/HolyNova_spritesheet");
            Texture2D txFury = game.Content.Load<Texture2D>("Effects/HeavensFury_spritesheet");
            Texture2D txShield = game.Content.Load<Texture2D>("Effects/HolyShield_spritesheet");
            Texture2D txElectric = game.Content.Load<Texture2D>("Effects/Sprite-sheet");
            Texture2D txJustice = game.Content.Load<Texture2D>("Effects/SwordOfJustice_spritesheet");

            // Build the manager that handles all spell cooldowns and input.
            _spellManager = new SpellManager(this, txSmite, txNova, txFury, txShield, txElectric, txJustice);

            // Construct the initial wave of enemies and allies, and wire up the combat listener.
            InitializeArmies();
            SetupCombatEvents();
        }

        private void InitializeArmies()
        {
            // Load the animation dictionaries for the 3 different tiers of Orc enemies.
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

            // Load the animation dictionary for the standard player allies.
            _allyTextures["Walk"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Walk");
            _allyTextures["Attack"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Slash");
            _allyTextures["Idle"] = _gameOwnedBy.Content.Load<Texture2D>("Ally/Character_Idle");

            // Spawn a small starting squad of allies in a neat vertical column behind the player.
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = _allySpawnOrigin + new Vector2(0, i * 100);
                var ally = new Ally(_gameOwnedBy, _allyTextures, pos, 4);
                ally.Name = $"Soldier {i + 1}";
                _allies.Add(ally);
            }

            // Tell the engine to generate the first wave of enemies on the opposite side of the map.
            CreateEnemyFormation(10);
        }

        private void CreateEnemyFormation(int count)
        {
            // We use some math to arrange the enemies in a triangular, phalanx-style formation.
            int currentRow = 0;
            int enemiesInCurrentRow = 1;
            int currentSlotInRow = 0;
            float spacingX = 80f;
            float spacingY = 80f;

            for (int i = 0; i < count; i++)
            {
                // Calculate exactly where in the triangle this specific enemy should stand.
                float xPos = _enemySpawnOrigin.X + (currentRow * spacingX);
                float rowHeight = (enemiesInCurrentRow - 1) * spacingY;
                float yPos = (_enemySpawnOrigin.Y - (rowHeight / 2f)) + (currentSlotInRow * spacingY);
                int typeIndex = 0;

                // Make the very first enemy (the point of the triangle) a massive, high-damage Captain.
                // Randomize the rest of the ranks between basic Grunts and Warriors.
                if (i == 0)
                {
                    typeIndex = 2;
                }
                else
                {
                    typeIndex = CombatSystem.RandomInt(0, 2);
                }

                var enemy = new Enemy(_gameOwnedBy, _allOrcTypes[typeIndex], new Vector2(xPos, yPos), 4);

                // Assign stats based on the randomly chosen type.
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

                // If we filled up the current row of the triangle, move back and start a slightly larger row.
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
            // We subscribe to the static CombatSystem event. 
            // This means anytime *any* character dies anywhere in the game, this block of code automatically runs.
            CombatSystem.OnCombatantKilled += (killer, victim) =>
            {
                // If an enemy died, increase the player's score. 
                // Then immediately spawn either 1 or 2 new enemies off-screen to replace them.
                if (victim.Team == CombatTeam.Enemy)
                {
                    EnemiesKilled++;
                    int spawnCount = (CombatSystem.RandomInt(0, 100) < 80) ? 1 : 2;
                    SpawnReinforcements(CombatTeam.Enemy, spawnCount);
                }
                // If a friendly NPC died, track the loss and spawn 1 to 3 new soldiers to help the player.
                else if (victim.Team == CombatTeam.Player && victim != _player)
                {
                    AlliesLost++;
                    SpawnReinforcements(CombatTeam.Player, CombatSystem.RandomInt(1, 3));
                }
            };
        }

        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;

            // Before the battle officially starts, the player can freely run around the map without enemies attacking.
            if (!_battleStarted)
            {
                // We must still update the player so they can walk, passing in the empty enemy lists so collision math doesn't break.
                _player.Update(gameTime, _levelManager.CurrentLevel, _enemies, _levelManager.MapObjects);

                // Listen for a specific debug key to trigger the start of the massive battle.
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    _battleStarted = true;
                }
            }
            else
            {
                // Once the battle begins, hand control over to the main physics loop.
                UpdateBattle(gameTime, vp);
            }
        }

        private void UpdateBattle(GameTime gameTime, Viewport vp)
        {
            // If the camera was zoomed out (like during a cinematic or boss intro), 
            // smoothly interpolate the zoom back to standard 1.0 gameplay scale.
            _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

            // We update the player first. This ensures their X/Y position is perfectly accurate 
            // before we ask all the AI enemies to calculate angles and distances to them.
            _player.Update(gameTime, _levelManager.CurrentLevel, _enemies, _levelManager.MapObjects);

            // Update the procedural set dressing (like the animated dead tree).
            _levelManager.Update(gameTime, _player);

            _player.DamageMultiplier = GlobalPlayerDamageMult;

            // We use a timer to throttle how often the AI recalculates its targets. 
            // If the timer hits our predefined interval (usually around 1 second), we throw the 'scanNow' flag.
            _targetingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            bool scanNow = _targetingTimer >= GameConstants.TargetScanInterval;
            if (scanNow) _targetingTimer = 0;

            // Step the massive logic blocks forward.
            _spellManager.Update(gameTime);
            UpdateAllies(gameTime, scanNow);
            UpdateEnemies(gameTime, scanNow);

            // Clean out the master lists so we aren't drawing or calculating math for dead bodies.
            CleanupDead();

            // A safety net: if the player somehow managed to kill every single enemy on screen 
            // before the reinforcements could spawn, force a massive wave to keep the battle going.
            if (_enemies.Count == 0 && !SpawningBlocked)
            {
                SpawnReinforcements(CombatTeam.Enemy, 5);
            }

            // Finally, update the camera to lock onto wherever the player walked during this frame.
            _camera.follow(_player.CentrePos, vp);
        }

        private void UpdateAllies(GameTime gameTime, bool scanForTargets)
        {
            foreach (var ally in _allies)
            {
                // Skip logic for dead or dying allies.
                if (!ally.IsActive) continue;

                // If the engine threw the 'scanNow' flag, AND the ally doesn't currently have a target...
                if (scanForTargets && NeedsNewTarget(ally))
                {
                    // Search the entire list of enemies for the closest valid target.
                    var target = FindBestTarget(ally, _enemies.Cast<ICombatant>());
                    if (target != null)
                    {
                        // Lock on to them, and command the ally's State Machine to transition from Wander/Idle into Chase mode.
                        CombatSystem.AssignTarget(ally, target);
                        ally.ChangeState(new ChaseState());
                    }
                }

                // Regardless of whether they acquired a target this frame, tell the ally to step its active physics state forward.
                ally.Update(gameTime, _levelManager.MapObjects);
            }
        }

        private void UpdateEnemies(GameTime gameTime, bool scanForTargets)
        {
            // This functions almost identically to UpdateAllies.
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsActive) continue;

                if (scanForTargets && NeedsNewTarget(enemy))
                {
                    // Enemies have a slightly larger target pool. They compile a temporary list containing the Player 
                    // AND every single active Ally, then run the distance math against that combined list.
                    var potentialTargets = new List<ICombatant> { _player };
                    potentialTargets.AddRange(_allies.Cast<ICombatant>());

                    var target = FindBestTarget(enemy, potentialTargets);
                    if (target != null)
                    {
                        CombatSystem.AssignTarget(enemy, target);
                        enemy.ChangeState(new ChaseState());
                    }
                }
                enemy.Update(gameTime, _levelManager.MapObjects);
            }
        }

        private bool NeedsNewTarget(ICombatant combatant)
        {
            // The AI only needs to run the heavy distance math if they are completely idle, 
            // or if the target they were fighting died/turned invincible.
            return combatant.CurrentTarget == null ||
                   !CombatSystem.IsValidTarget(combatant, combatant.CurrentTarget);
        }

        private ICombatant FindBestTarget(ICombatant seeker, IEnumerable<ICombatant> candidates)
        {
            ICombatant best = null;
            float closestDistance = float.MaxValue;

            // Loop through every single potential target passed in by the AI.
            foreach (var candidate in candidates)
            {
                // Ignore dead or out-of-bounds targets.
                if (!CombatSystem.IsValidTarget(seeker, candidate))
                    continue;

                // We have a cap on how many characters can attack a single target at once.
                // This prevents 50 enemies from swarming a single ally and causing sprite clipping issues.
                if (candidate.AttackerCount >= GameConstants.MaxAttackersPerTarget)
                    continue;

                // Calculate the true pixel distance. If this candidate is closer than the last one we checked, 
                // store them as the new 'best' option.
                float distance = CombatSystem.GetDistance(seeker, candidate);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    best = candidate;
                }
            }
            return best;
        }

        private void CleanupDead()
        {
            // Wipes fully dead characters from the master lists so they stop taking up memory.
            _allies.RemoveAll(a => a.LifecycleState == Ally.ALLYSTATE.DEAD);
            _enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);
        }

        private void SpawnReinforcements(CombatTeam team, int count)
        {
            if (!SpawningBlocked)
            {
                for (int i = 0; i < count; i++)
                {
                    // Pick a random angle (0 to 360 degrees) and a distance far off-screen.
                    float angle = CombatSystem.RandomFloat(0, MathHelper.TwoPi);
                    float distance = CombatSystem.RandomFloat(900f, 1300f);

                    // Use standard trigonometry to calculate exactly where that spawn point is relative to the player.
                    Vector2 spawnPos = _player.Position + new Vector2(
                        (float)Math.Cos(angle) * distance,
                        (float)Math.Sin(angle) * distance
                    );

                    // Even though they spawn off-screen, we must ensure they don't accidentally spawn completely outside the map boundary.
                    float safeMarginX = 300f;
                    float safeMarginY = 600f;
                    spawnPos.X = MathHelper.Clamp(spawnPos.X, safeMarginX, _mapSize.X - safeMarginX);
                    spawnPos.Y = MathHelper.Clamp(spawnPos.Y, safeMarginY, _mapSize.Y - safeMarginY);

                    // If we are spawning a new Enemy...
                    if (team == CombatTeam.Enemy)
                    {
                        // Roll a quick percentage chance to decide if it's a Grunt, Warrior, or massive Captain.
                        int roll = CombatSystem.RandomInt(0, 100);
                        int typeIndex = 0;

                        if (roll >= 90) typeIndex = 2;
                        else if (roll >= 60) typeIndex = 1;
                        else typeIndex = 0;

                        var newEnemy = new Enemy(_gameOwnedBy, _allOrcTypes[typeIndex], spawnPos, 4);

                        if (typeIndex == 0) { newEnemy.Name = "Reinforcement Grunt"; newEnemy.AttackDamage = 10; }
                        if (typeIndex == 1) { newEnemy.Name = "Reinforcement Warrior"; newEnemy.AttackDamage = 20; }
                        if (typeIndex == 2) { newEnemy.Name = "Reinforcement Captain"; newEnemy.AttackDamage = 35; newEnemy.Scale = 3.5f; }

                        // Immediately lock them onto the player so they come charging onto the screen aggressively.
                        CombatSystem.AssignTarget(newEnemy, _player);
                        newEnemy.ChangeState(new ChaseState());
                        _enemies.Add(newEnemy);
                    }
                    // If we are spawning a friendly Ally...
                    else if (team == CombatTeam.Player)
                    {
                        var newAlly = new Ally(_gameOwnedBy, _allyTextures, spawnPos, 4);
                        newAlly.Name = "Reinforcement Soldier";

                        // See if there are any enemies currently alive. If there are, lock on and attack.
                        var bestTarget = FindBestTarget(newAlly, _enemies.Cast<ICombatant>());
                        if (bestTarget != null)
                        {
                            CombatSystem.AssignTarget(newAlly, bestTarget);
                            newAlly.ChangeState(new ChaseState());
                        }
                        // If the map is totally clear, just patrol around the player.
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

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // First, draw the completely static background map layer (the grass and dirt).
            _levelManager.Draw(spriteBatch);

            // Create a temporary list to hold every single physical sprite on the screen.
            List<Sprite> renderList = new List<Sprite>();

            // Add the player, then loop through all the master lists and add every active character and tree.
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

            // Hand this massive, unsorted list over to the RenderPipeline.
            // The pipeline will sort them mathematically by their Y-axis position to create the illusion of 3D depth, 
            // ensuring characters standing "in front" of a tree obscure it properly.
            Renderer.DrawDepthSorted(spriteBatch, renderList);

            // Finally, draw all the magical particle effects over the top of the sorted characters.
            _spellManager.Draw(spriteBatch);
        }

        // Quick helper functions used by the GameplayState to check if the battle is over.
        public Player GetPlayer() => _player;
        public int AllyCount => _allies.Count(a => a.IsAlive);
        public int EnemyCount => _enemies.Count(e => e.IsAlive);
        public bool IsBattleOver => _enemies.Count == 0 || !_player.IsAlive;
    }
}