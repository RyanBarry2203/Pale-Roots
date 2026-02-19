using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Player: controllable ICombatant with movement, dash and melee attack state machine.
    // - Uses AnimationManager for visuals and GameConstants for shared tuning.
    // - Interacts with CombatSystem for damage and ChaseAndFireEngine/LevelManager for world queries.
    public class Player : Sprite, ICombatant
    {
        // ICombatant surface
        public string Name { get; set; } = "Hero";
        public float DamageMultiplier { get; set; } = 1.0f;
        public CombatTeam Team => CombatTeam.Player;

        // Cached mouse world position used for aiming attacks
        private Vector2 _mouseWorldPosition;

        // Dash timers exposed for UI
        public float DashTimer => _dashCooldownTimer;
        public float DashDuration => _dashCooldownDuration;

        // Health / combat members (MaxHealth/Health from Sprite base)
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => Health > 0;
        public bool IsActive => Visible;
        public ICombatant CurrentTarget { get; set; }
        public Vector2 Position => position;
        public Vector2 CentrePos => Center;

        // Movement & state
        private float _speed;
        private Vector2 _velocity;

        // Player state machine for movement/combat/interrupts
        public enum PlayerState
        {
            Idle,
            Run,
            Attack1,
            Attack2,
            Dash,
            Hurt,
            Dead
        }
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        // Facing and unlockables
        private Vector2 _facingDirection = new Vector2(0, 1);
        public bool IsHeavyAttackUnlocked { get; set; } = false;
        public bool IsDashUnlocked { get; set; } = false;

        // Dash variables
        private float _dashSpeed = 12f;
        private Vector2 _dashDirection;

        // Timers / combo buffering
        private float _stateTimer = 0f;
        private bool _comboBuffered = false;
        private float _cooldownTimer = 0f;

        // Enemies already hit this swing (prevents multi-hit per swing)
        private List<ICombatant> _enemiesHitThisAttack = new List<ICombatant>();

        // Animation / direction
        public enum Direction {Down = 0, Left = 0, Right = 2, Up = 3 }
        private Direction _currentDirection = Direction.Down;
        private int _currentDirectionIndex = 2;

        // Dash cooldown / invincibility flag during dash
        private float _dashCooldownTimer = 0f;
        private float _dashCooldownDuration = 800f;
        private bool _isInvincible = false;

        // Visual offset used when drawing (adjust to align sprite to logic position)
        private Vector2 _visualOffset = new Vector2(0, 32);

        // Per-state textures (loaded in ctor) and animation manager
        private Texture2D _txIdle;
        private Texture2D _txRun;
        private Texture2D _txAttack1;
        private Texture2D _txAttack2;
        private Texture2D _txHurt;
        private Texture2D _txDeath;
        private Texture2D _txDash;
        private AnimationManager _animManager;
        private SpriteEffects _flipEffect = SpriteEffects.None;

        // Shared 1x1 texture used for debug/health bar drawing
        private static Texture2D _healthBarTexture;

        // Constructor: load textures, set stats and register animations
        public Player(Game game, Texture2D texture, Vector2 startPosition, int frameCount)
            : base(game, texture, startPosition, frameCount, 1)
        {
            _speed = GameConstants.DefaultPlayerSpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            Scale = 3f;

            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            _animManager = new AnimationManager();

            // 1. Load texture sheets from Content
            _txIdle = game.Content.Load<Texture2D>("Player/Idle");
            _txRun = game.Content.Load<Texture2D>("Player/Run");
            _txAttack1 = game.Content.Load<Texture2D>("Player/Attack 1");
            _txAttack2 = game.Content.Load<Texture2D>("Player/Attack 2");
            _txHurt = game.Content.Load<Texture2D>("Player/Hurt");
            _txDeath = game.Content.Load<Texture2D>("Player/Death");
            _txDash = game.Content.Load<Texture2D>("Player/Dash");

            // 2. Register animations (use Idle width as standard frame width for some sheets)
            int standardWidth = _txIdle.Width / 7;

            _animManager.AddAnimation("Idle", new Animation(_txIdle, 7, 0, 150f, true));
            _animManager.AddAnimation("Run", new Animation(_txRun, 8, 0, 120f, true));
            _animManager.AddAnimation("Attack1", new Animation(_txAttack1, 10, 0, 100f, false, 1, standardWidth));
            _animManager.AddAnimation("Attack2", new Animation(_txAttack2, 10, 0, 100f, false, 1, standardWidth));
            _animManager.AddAnimation("Dash", new Animation(_txDash, 4, 0, 125f, false, 1, standardWidth));
            _animManager.AddAnimation("Hurt", new Animation(_txHurt, 3, 0, 150f, false, 1, standardWidth));
            _animManager.AddAnimation("Death", new Animation(_txDeath, 15, 0, 150f, false, 1, standardWidth));

            _animManager.Play("Idle");
        }

        // Main update called by the engine each frame.
        // - currentLayer used for collision/tile checks
        // - enemies list used for attack collision tests
        public void Update(GameTime gameTime, TileLayer currentLayer, List<Enemy> enemies)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Timers
            if (_cooldownTimer > 0) _cooldownTimer -= dt;
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= dt;

            // Compute mouse world position from screen center and player center
            MouseState mouseState = Mouse.GetState();
            Vector2 screenCenter = new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
            Vector2 mouseOffset = new Vector2(mouseState.X, mouseState.Y) - screenCenter;
            _mouseWorldPosition = this.Center + mouseOffset;

            // State machine dispatch
            switch (CurrentState)
            {
                case PlayerState.Idle:
                case PlayerState.Run:
                    HandleInput(currentLayer, true); // allow switching Run/Idle
                    CheckForCombatInput(enemies);
                    break;

                case PlayerState.Attack1:
                case PlayerState.Attack2:
                    HandleInput(currentLayer, false); // allow movement but don't change attack state
                    UpdateAttack(gameTime, enemies, (CurrentState == PlayerState.Attack1 ? 1 : 2));
                    break;

                case PlayerState.Dash:
                    UpdateDash(gameTime, currentLayer);
                    break;

                case PlayerState.Hurt:
                    HandleInput(currentLayer, false);
                    UpdateHurt(gameTime);
                    break;

                case PlayerState.Dead:
                    break;
            }

            UpdateAnimation(gameTime);
        }

        // Check input for combat actions (dash or mouse buttons)
        private void CheckForCombatInput(List<Enemy> enemies)
        {
            KeyboardState kState = Keyboard.GetState();
            MouseState mState = Mouse.GetState();

            // Dash input
            if (IsDashUnlocked && kState.IsKeyDown(Keys.LeftShift))
            {
                StartDash();
                return;
            }

            if (_cooldownTimer > 0) return;

            // Attack input: left = light, right = heavy (if unlocked)
            if (mState.LeftButton == ButtonState.Pressed)
            {
                StartAttack(enemies, 1);
            }
            else if (IsHeavyAttackUnlocked && mState.RightButton == ButtonState.Pressed)
            {
                StartAttack(enemies, 2);
            }
        }

        // Begin dash: set invincibility, direction and timers
        private void StartDash()
        {
            if (_dashCooldownTimer > 0) return;

            CurrentState = PlayerState.Dash;
            _dashSpeed = 20f;
            _stateTimer = 150f;

            _dashCooldownTimer = _dashCooldownDuration;
            _isInvincible = true;

            // If moving, dash that direction. Otherwise dash horizontally based on facing index.
            if (_velocity != Vector2.Zero)
                _dashDirection = Vector2.Normalize(_velocity);
            else
            {
                if (_currentDirectionIndex == 2) _dashDirection = new Vector2(-1, 0);
                else _dashDirection = new Vector2(1, 0);
            }
        }

        // Dash state update: move for duration then clear invincibility
        private void UpdateDash(GameTime gameTime, TileLayer layer)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _stateTimer -= dt;

            position += _dashDirection * _dashSpeed;

            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
                _velocity = Vector2.Zero;
                _isInvincible = false;
            }
        }

        // Hurt state countdown then return to Idle
        private void UpdateHurt(GameTime gameTime)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
            }
        }

        // Read WASD, set velocity and attempt movement against the tile layer.
        // updateState = false prevents state switches (used during attacks/hurt).
        private void HandleInput(TileLayer currentLayer, bool updateState)
        {
            Vector2 inputDirection = Vector2.Zero;
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W)) inputDirection.Y -= 1;
            if (state.IsKeyDown(Keys.S)) inputDirection.Y += 1;
            if (state.IsKeyDown(Keys.A)) inputDirection.X -= 1;
            if (state.IsKeyDown(Keys.D)) inputDirection.X += 1;

            if (inputDirection != Vector2.Zero)
            {
                inputDirection.Normalize();
                _facingDirection = inputDirection;
                _velocity = inputDirection * _speed;

                _currentDirectionIndex = GetDirectionFromVector(_facingDirection);

                Vector2 proposedPosition = position + _velocity;
                if (currentLayer != null && CanMoveTo(proposedPosition, currentLayer))
                {
                    position = proposedPosition;
                }
                else if (currentLayer == null)
                {
                    position = proposedPosition;
                }
            }
            else
            {
                _velocity = Vector2.Zero;
            }

            // Only change Run/Idle when allowed (prevents attack animation override).
            if (updateState)
            {
                if (_velocity != Vector2.Zero)
                    CurrentState = PlayerState.Run;
                else
                    CurrentState = PlayerState.Idle;
            }
        }

        // Map a direction vector to an animation row index
        private int GetDirectionFromVector(Vector2 dir)
        {
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
            {
                return (dir.X > 0) ? 3 : 2;
            }
            else
            {
                return (dir.Y > 0) ? 0 : 1;
            }
        }

        // Collision / tile movement check. Computes a small "feet" rectangle for passability tests.
        private bool CanMoveTo(Vector2 newPos, TileLayer layer)
        {
            float scale = (float)Scale;
            int playerW = (int)(spriteWidth * scale * 0.4f);
            int playerH = (int)(spriteHeight * scale * 0.2f);

            int playerX = (int)(newPos.X - (playerW / 2));
            int playerY = (int)(newPos.Y + (spriteHeight * scale / 2) - playerH);

            Rectangle futurePlayerBox = new Rectangle(playerX, playerY, playerW, playerH);

            float mapWidth = layer.Tiles.GetLength(1) * 64;
            float mapHeight = layer.Tiles.GetLength(0) * 64;

            // Basic bounds checks
            if (newPos.X < 0 || newPos.Y < 0) return false;
            if (newPos.X + (spriteWidth * Scale) > mapWidth) return false;
            if (newPos.Y + (spriteHeight * Scale) > mapHeight) return false;

            // Tile passability checks would go here (sample surrounding tiles using futurePlayerBox).
            // This function currently returns true (placeholder for your tile queries).
            return true;
        }

        // -----------------------
        // COMBAT
        // -----------------------

        // Initialize attack: set facing to mouse, start animation and reset hit list.
        private void StartAttack(List<Enemy> enemies, int attackNum)
        {
            Vector2 dirToMouse = _mouseWorldPosition - this.Center;
            dirToMouse.Normalize();
            _currentDirectionIndex = GetDirectionFromVector(dirToMouse);

            if (_currentDirectionIndex == 2) _flipEffect = SpriteEffects.FlipHorizontally;
            else _flipEffect = SpriteEffects.None;

            string animName = (attackNum == 1) ? "Attack1" : "Attack2";
            float frameSpeed = (attackNum == 1) ? 80f : 120f;
            float duration = 10 * frameSpeed;

            CurrentState = (attackNum == 1) ? PlayerState.Attack1 : PlayerState.Attack2;
            _stateTimer = duration;
            _comboBuffered = false;

            // Reset hit list so each swing can hit targets again
            _enemiesHitThisAttack.Clear();
        }

        // Per-frame attack update: perform hit checks and end attack when timer expires
        private void UpdateAttack(GameTime gameTime, List<Enemy> enemies, int attackNum)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _stateTimer -= dt;

            // Perform the hit detection each frame while in attack state
            if (attackNum == 1)
                PerformSwordHit(enemies, 1.0f, 1.0f);
            else
                PerformSwordHit(enemies, 2.0f, 1.5f);

            // End attack and set cooldown
            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
                _cooldownTimer = GameConstants.SwordCooldown;
                _debugSwordBox = Rectangle.Empty;
            }
        }

        private Rectangle _debugSwordBox;

        // Sword hit detection: builds a chest-relative hitbox and checks enemy intersections
        private void PerformSwordHit(List<Enemy> enemies, float damageMult, float knockbackMult)
        {
            // Chest position used as sword origin (updates as player moves)
            Vector2 chestPosition = new Vector2(position.X, position.Y - 50);

            // Cardinal attack directions based on currentDirectionIndex
            Vector2 attackDir = Vector2.Zero;
            if (_currentDirectionIndex == 0) attackDir = new Vector2(0, 1);  // Down
            if (_currentDirectionIndex == 1) attackDir = new Vector2(0, -1); // Up
            if (_currentDirectionIndex == 2) attackDir = new Vector2(-1, 0); // Left
            if (_currentDirectionIndex == 3) attackDir = new Vector2(1, 0);  // Right

            // Build a square hitbox out in front of the chest
            float reach = 80f * knockbackMult;
            Vector2 hitCenter = chestPosition + (attackDir * reach);
            int boxSize = (int)(80 * knockbackMult);

            Rectangle swordHitbox = new Rectangle(
                (int)(hitCenter.X - boxSize / 2),
                (int)(hitCenter.Y - boxSize / 2),
                boxSize,
                boxSize
            );

            // Keep debug rectangle for drawing/inspection
            _debugSwordBox = swordHitbox;

            // Collision test per enemy; skip already-hit targets this swing
            foreach (var enemy in enemies.ToArray())
            {
                if (!enemy.IsAlive) continue;
                if (_enemiesHitThisAttack.Contains(enemy)) continue;

                Rectangle enemyRect = new Rectangle(
                    (int)enemy.Position.X - 30,
                    (int)enemy.Position.Y - 80,
                    60,
                    80
                );

                if (swordHitbox.Intersects(enemyRect))
                {
                    int finalDamage = (int)(GameConstants.SwordDamage * damageMult * DamageMultiplier);
                    CombatSystem.DealDamage(this, enemy, finalDamage);

                    Vector2 kb = enemy.Position - this.position;
                    if (kb != Vector2.Zero) kb.Normalize();
                    enemy.ApplyKnockback(kb * GameConstants.SwordKnockback * knockbackMult);

                    _enemiesHitThisAttack.Add(enemy);
                }
            }
        }

        // -----------------------
        // ICombatant methods
        // -----------------------

        public void TakeDamage(int amount, ICombatant attacker)
        {
            // Respect invincibility frames (dash)
            if (_isInvincible) return;
            if (!IsAlive) return;

            Health -= amount;

            if (Health <= 0)
            {
                Die();
            }
            else
            {
                CurrentState = PlayerState.Hurt;
                _stateTimer = 300f;
            }
        }

        // Not used; CombatSystem.DealDamage is called directly from attack code
        public void PerformAttack() { }

        // Choose animation key and flip sprite based on state and velocity
        private void UpdateAnimation(GameTime gameTime)
        {
            string animKey = "Idle";

            switch (CurrentState)
            {
                case PlayerState.Idle: animKey = "Idle"; break;
                case PlayerState.Run: animKey = "Run"; break;
                case PlayerState.Attack1: animKey = "Attack1"; break;
                case PlayerState.Attack2: animKey = "Attack2"; break;
                case PlayerState.Dash: animKey = "Dash"; break;
                case PlayerState.Hurt: animKey = "Hurt"; break;
                case PlayerState.Dead: animKey = "Death"; break;
            }

            // Only flip during non-attack movement states so attack facing stays locked to click direction.
            if (CurrentState == PlayerState.Run || CurrentState == PlayerState.Idle || CurrentState == PlayerState.Dash)
            {
                if (_velocity.X < -0.1f) _flipEffect = SpriteEffects.FlipHorizontally;
                else if (_velocity.X > 0.1f) _flipEffect = SpriteEffects.None;
            }

            _animManager.Play(animKey);
            _animManager.Update(gameTime);
        }

        // Enter death state; keep sprite visible so death animation plays
        public void Die()
        {
            CurrentState = PlayerState.Dead;
            CombatSystem.ClearTarget(this);
        }

        // -----------------------
        // DRAW
        // -----------------------
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Visual offset corrects sprite drawing so feet align with logic position
            Vector2 visualOffset = new Vector2(0, 175);
            Vector2 drawPos = position + visualOffset;

            // Draw animation using AnimationManager
            _animManager.Draw(spriteBatch, drawPos, (float)Scale, _flipEffect, _currentDirectionIndex);

            // Debug drawing (commented out) shows hitboxes and logic dot; useful while tuning alignment.
            // Examples left in file for quick experiments.
        }
    }
}