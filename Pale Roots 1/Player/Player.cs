using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class Player : Sprite, ICombatant
    {
        // ===================
        // ICombatant Properties
        // ===================
        public string Name { get; set; } = "Hero";
        public CombatTeam Team => CombatTeam.Player;

        private Vector2 _mouseWorldPosition;

        //public int Health
        //{
        //    get => _health;
        //    set => _health = Math.Max(0, value);
        //}

        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => Health > 0;
        public bool IsActive => Visible;
        public ICombatant CurrentTarget { get; set; }
        public Vector2 Position => position;
        public Vector2 CentrePos => Center;

        // ===================
        // MOVEMENT & SWORD FIELDS
        // ===================
        private float _speed;
        private Vector2 _velocity;

        // SWORD FIELDS
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
        private Vector2 _facingDirection = new Vector2(0, 1);

        // Dash Variables
        private float _dashSpeed = 12f;
        private Vector2 _dashDirection;

        private float _stateTimer = 0f;   
        private bool _comboBuffered = false;

        //private float _swingTimer = 0f;
        private float _cooldownTimer = 0f;

        private List<ICombatant> _enemiesHitThisAttack = new List<ICombatant>();


        // animation stuff

        public enum Direction {Down = 0, Left = 0, Right = 2, Up = 3 }
        private Direction _currentDirection = Direction.Down;
        private int _currentDirectionIndex = 2;

        private float _dashCooldownTimer = 0f;
        private float _dashCooldownDuration = 800f;
        private bool _isInvincible = false;
        private Vector2 _visualOffset = new Vector2(0, 32);

        private Texture2D _txIdle;
        private Texture2D _txRun;
        private Texture2D _txAttack1;
        private Texture2D _txAttack2; // Optional, if you want combo attacks
        private Texture2D _txHurt;
        private Texture2D _txDeath;
        private Texture2D _txDash;

        private AnimationManager _animManager;
        private SpriteEffects _flipEffect = SpriteEffects.None;

        // --- FIX: CENTERED PIVOT ---
        // (0, -32) moves the pivot from the feet (Center) up to the Chest/Neck.
        // It is centered on the X axis (0) so it swings evenly left and right.

        // HEALTH BAR TEXTURE
        private static Texture2D _healthBarTexture;

        // ===================
        // CONSTRUCTOR
        // ===================
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

            // 1. LOAD TEXTURES
            _txIdle = game.Content.Load<Texture2D>("Player/Idle");
            _txRun = game.Content.Load<Texture2D>("Player/Run");
            _txAttack1 = game.Content.Load<Texture2D>("Player/Attack 1");
            _txAttack2 = game.Content.Load<Texture2D>("Player/Attack 2");
            _txHurt = game.Content.Load<Texture2D>("Player/Hurt");
            _txDeath = game.Content.Load<Texture2D>("Player/Death");
            _txDash = game.Content.Load<Texture2D>("Player/Dash");

            // 2. REGISTER ANIMATIONS (THE MODULAR FIX)

            // We assume the IDLE sheet is the "Correct" size.
            // Idle has 7 frames.
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

        // ===================
        // UPDATE
        // ===================

        public void Update(GameTime gameTime, TileLayer currentLayer, List<Enemy> enemies)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // 1. Update Timers
            if (_cooldownTimer > 0) _cooldownTimer -= dt;
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= dt;

            // 2. Update Mouse Position
            MouseState mouseState = Mouse.GetState();
            Vector2 screenCenter = new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
            Vector2 mouseOffset = new Vector2(mouseState.X, mouseState.Y) - screenCenter;
            _mouseWorldPosition = this.Center + mouseOffset;

            // 3. STATE MACHINE
            // IN FILE: Player.cs inside Update()

            switch (CurrentState)
            {
                case PlayerState.Idle:
                case PlayerState.Run:
                    HandleInput(currentLayer, true); // TRUE: Allowed to switch between Run/Idle
                    CheckForCombatInput(enemies);
                    break;

                case PlayerState.Attack1:
                case PlayerState.Attack2:
                    HandleInput(currentLayer, false); // FALSE: Move, but DO NOT change state. Keep Attacking.
                    UpdateAttack(gameTime, enemies, (CurrentState == PlayerState.Attack1 ? 1 : 2));
                    break;

                case PlayerState.Dash:
                    UpdateDash(gameTime, currentLayer);
                    break;

                case PlayerState.Hurt:
                    HandleInput(currentLayer, false); // FALSE: Move, but keep Hurt animation playing.
                    UpdateHurt(gameTime);
                    break;

                case PlayerState.Dead:
                    break;
            }

            UpdateAnimation(gameTime);
        }

        private void CheckForCombatInput(List<Enemy> enemies)
        {
            KeyboardState kState = Keyboard.GetState();
            MouseState mState = Mouse.GetState();

            if (_cooldownTimer <= 0 && (kState.IsKeyDown(Keys.Space) || mState.LeftButton == ButtonState.Pressed))
            {
                StartAttack(enemies, 1);
            }
            else if (_cooldownTimer <= 0 && kState.IsKeyDown(Keys.LeftShift))
            {
                StartDash();
            }
        }

        // IN FILE: Player.cs

        private void StartDash()
        {

            if (_dashCooldownTimer > 0) return;

            CurrentState = PlayerState.Dash;

            _dashSpeed = 20f;
            _stateTimer = 150f;

            _dashCooldownTimer = _dashCooldownDuration; 
            _isInvincible = true; 

            // Determine direction
            if (_velocity != Vector2.Zero)
                _dashDirection = Vector2.Normalize(_velocity);
            else
            {
                // Dash in facing direction if standing still
                if (_currentDirectionIndex == 2) _dashDirection = new Vector2(-1, 0);
                else _dashDirection = new Vector2(1, 0);
            }
        }

        private void UpdateDash(GameTime gameTime, TileLayer layer)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _stateTimer -= dt;

            // Move
            position += _dashDirection * _dashSpeed;

            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
                _velocity = Vector2.Zero;
                _isInvincible = false; // GOD MODE OFF
            }
        }
        private void UpdateHurt(GameTime gameTime)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Optional: Apply a little knockback friction here if you want

            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
            }
        }

        // IN FILE: Player.cs
        // Replace your HandleInput method with this updated version

        // IN FILE: Player.cs

        // CHANGE: Added 'bool updateState' parameter
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

                // Sync direction index
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

            // CRITICAL FIX: Only change the state if we are allowed to!
            // This prevents the Attack animation from being overwritten by Run/Idle.
            if (updateState)
            {
                if (_velocity != Vector2.Zero)
                    CurrentState = PlayerState.Run;
                else
                    CurrentState = PlayerState.Idle;
            }
        }

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

        private bool CanMoveTo(Vector2 newPos, TileLayer layer)
        {

            // FEET BOX MATH (CENTERED)
            float scale = (float)Scale;
            int playerW = (int)(spriteWidth * scale * 0.4f);
            int playerH = (int)(spriteHeight * scale * 0.2f);

            // X: Center - Half Box
            int playerX = (int)(newPos.X - (playerW / 2));

            // Y: Bottom of sprite - Box Height
            int playerY = (int)(newPos.Y + (spriteHeight * scale / 2) - playerH);

            Rectangle futurePlayerBox = new Rectangle(playerX, playerY, playerW, playerH);

            float mapWidth = layer.Tiles.GetLength(1) * 64;
            float mapHeight = layer.Tiles.GetLength(0) * 64;

            // Check Left/Top
            if (newPos.X < 0 || newPos.Y < 0) return false;

            // Check Right/Bottom Account for sprite size
            if (newPos.X + (spriteWidth * Scale) > mapWidth) return false;
            if (newPos.Y + (spriteHeight * Scale) > mapHeight) return false;

            // Tile Passability Check
            float feetY = newPos.Y + (spriteHeight * (float)Scale);
            float centerX = newPos.X + (spriteWidth * (float)Scale) / 2.0f;




            return true;
        }

        // ===================
        // COMBAT LOGIC
        // ===================

        private void StartAttack(List<Enemy> enemies, int attackNum)
        {
            // 1. Direction Logic
            Vector2 dirToMouse = _mouseWorldPosition - this.Center;
            dirToMouse.Normalize();
            _currentDirectionIndex = GetDirectionFromVector(dirToMouse);

            if (_currentDirectionIndex == 2) _flipEffect = SpriteEffects.FlipHorizontally;
            else _flipEffect = SpriteEffects.None;

            // 2. Set State & Timer
            string animName = (attackNum == 1) ? "Attack1" : "Attack2";
            float frameSpeed = (attackNum == 1) ? 80f : 120f;
            float duration = 10 * frameSpeed;

            CurrentState = (attackNum == 1) ? PlayerState.Attack1 : PlayerState.Attack2;
            _stateTimer = duration;
            _comboBuffered = false;

            // 3. RESET THE HIT LIST
            // New swing, so we can hit everyone again
            _enemiesHitThisAttack.Clear();

            // NOTE: We removed PerformSwordHit() from here! 
            // We will call it in Update instead.
        }


        private void UpdateAttack(GameTime gameTime, List<Enemy> enemies, int attackNum)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _stateTimer -= dt;

            // 1. PERFORM HIT CHECK (Every Frame!)
            // This ensures the hitbox moves WITH the player.
            if (attackNum == 1)
                PerformSwordHit(enemies, 1.0f, 1.0f);
            else
                PerformSwordHit(enemies, 2.0f, 1.5f);

            // 2. COMBO LOGIC
            float duration = (attackNum == 1 ? 800f : 1200f);
            float progress = 1f - (_stateTimer / duration);

            if (attackNum == 1 && (Keyboard.GetState().IsKeyDown(Keys.Space) || Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                if (progress > 0.5f) _comboBuffered = true;
            }

            if (_stateTimer <= 0)
            {
                if (attackNum == 1 && _comboBuffered)
                {
                    StartAttack(enemies, 2);
                }
                else
                {
                    CurrentState = PlayerState.Idle;
                    _cooldownTimer = GameConstants.SwordCooldown;
                    _debugSwordBox = Rectangle.Empty; // Clear debug box when done
                }
            }
        }

        private Rectangle _debugSwordBox;

        private void PerformSwordHit(List<Enemy> enemies, float damageMult, float knockbackMult)
        {
            // 1. Calculate Chest Position (Dynamic! Updates every frame)
            Vector2 chestPosition = new Vector2(position.X, position.Y - 50);

            // 2. Attack Direction
            Vector2 attackDir = Vector2.Zero;
            if (_currentDirectionIndex == 0) attackDir = new Vector2(0, 1);  // Down
            if (_currentDirectionIndex == 1) attackDir = new Vector2(0, -1); // Up
            if (_currentDirectionIndex == 2) attackDir = new Vector2(-1, 0); // Left
            if (_currentDirectionIndex == 3) attackDir = new Vector2(1, 0);  // Right

            // 3. Create Hitbox
            float reach = 80f * knockbackMult;
            Vector2 hitCenter = chestPosition + (attackDir * reach);
            int boxSize = (int)(80 * knockbackMult);

            Rectangle swordHitbox = new Rectangle(
                (int)(hitCenter.X - boxSize / 2),
                (int)(hitCenter.Y - boxSize / 2),
                boxSize,
                boxSize
            );

            // Update Debug Box so it follows us visually
            _debugSwordBox = swordHitbox;

            // 4. Check Collisions
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                // FILTER: If we already hit this guy this swing, skip him!
                if (_enemiesHitThisAttack.Contains(enemy)) continue;

                Rectangle enemyRect = new Rectangle(
                    (int)enemy.Position.X - 30,
                    (int)enemy.Position.Y - 80,
                    60,
                    80
                );

                if (swordHitbox.Intersects(enemyRect))
                {
                    // HIT CONFIRMED
                    int finalDamage = (int)(GameConstants.SwordDamage * damageMult);
                    CombatSystem.DealDamage(this, enemy, finalDamage);

                    Vector2 kb = enemy.Position - this.position;
                    if (kb != Vector2.Zero) kb.Normalize();
                    enemy.ApplyKnockback(kb * GameConstants.SwordKnockback * knockbackMult);

                    // Add to list so we don't hit him again this animation
                    _enemiesHitThisAttack.Add(enemy);
                }
            }
        }


        // ===================
        // INTERFACE METHODS
        // ===================
        // IN FILE: Player.cs

        public void TakeDamage(int amount, ICombatant attacker)
        {
            // 1. Check I-Frames
            if (_isInvincible) return; // Can't touch this

            if (!IsAlive) return;

            Health -= amount;

            if (Health <= 0)
            {
                Die();
            }
            else
            {
                CurrentState = PlayerState.Hurt;
                _stateTimer = 300f; // Short hurt animation (Snappy!)
            }
        }

        public void PerformAttack() { }

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

            // FLIP LOGIC
            // ONLY flip based on velocity if we are NOT attacking or dead.
            // This keeps the attack facing the direction we clicked.
            if (CurrentState == PlayerState.Run || CurrentState == PlayerState.Idle || CurrentState == PlayerState.Dash)
            {
                if (_velocity.X < -0.1f) _flipEffect = SpriteEffects.FlipHorizontally;
                else if (_velocity.X > 0.1f) _flipEffect = SpriteEffects.None;
            }

            _animManager.Play(animKey);
            _animManager.Update(gameTime);
        }

        public void Die()
        {
            // Don't set Visible = false yet! We want to see the body fall.
            CurrentState = PlayerState.Dead;
            CombatSystem.ClearTarget(this);
        }

        // ===================
        // DRAW
        // ===================

        public override void Draw(SpriteBatch spriteBatch)
        {
            // 1. Draw Sprite
            _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect, _currentDirectionIndex);

            // 2. Draw Health Bar
            if (IsAlive)
            {
                int barY = (int)position.Y - 120;
                int barWidth = (int)(32 * Scale);
                int barX = (int)position.X - (barWidth / 2);

                spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, barWidth, 8), Color.DarkRed);
                float healthPercent = (float)Health / MaxHealth;
                spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, (int)(barWidth * healthPercent), 8), Color.Gold);
            }

            // 3. DEBUG: Draw the Sword Hitbox
            // Only draw it if we are currently attacking
            if (CurrentState == PlayerState.Attack1 || CurrentState == PlayerState.Attack2)
            {
                // Draw a semi-transparent Red Box
                spriteBatch.Draw(_healthBarTexture, _debugSwordBox, new Color(255, 0, 0, 100));
            }

            // 4. DEBUG: Draw the Player's Hurtbox (Where enemies hit YOU)
            // Feet up to Head
            Rectangle myHurtBox = new Rectangle((int)position.X - 20, (int)position.Y - 80, 40, 80);
            // Draw a semi-transparent Blue Box
            spriteBatch.Draw(_healthBarTexture, myHurtBox, new Color(0, 0, 255, 100));

            // 5. Cyan Dot (Feet)
            spriteBatch.Draw(_healthBarTexture, new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4), Color.Cyan);
        }
    }
}