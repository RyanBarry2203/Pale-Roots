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
        public int AttackerCount { get; set; }
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

        // Dash Variables
        private float _dashSpeed = 12f;
        private float _dashTimer = 0f;
        private Vector2 _dashDirection;

        private float _stateTimer = 0f;   
        private bool _comboBuffered = false;

        //private float _swingTimer = 0f;
        private float _cooldownTimer = 0f;
        private Vector2 _facingDirection = new Vector2(0, 1);


        // animation stuff

        public enum Direction {Down = 0, Left = 0, Right = 2, Up = 3 }
        private Direction _currentDirection = Direction.Down;

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
            if (_cooldownTimer > 0) _cooldownTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Update Mouse Position
            MouseState mouseState = Mouse.GetState();
            Vector2 screenCenter = new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
            Vector2 mouseOffset = new Vector2(mouseState.X, mouseState.Y) - screenCenter;
            _mouseWorldPosition = this.Center + mouseOffset;



            // STATE MACHINE
            switch (CurrentState)
            {
                case PlayerState.Idle:
                case PlayerState.Run:
                    HandleInput(currentLayer);
                    CheckForCombatInput(enemies);
                    break;

                case PlayerState.Attack1:
                    UpdateAttack(gameTime, enemies, 1);
                    break;

                case PlayerState.Attack2:
                    UpdateAttack(gameTime, enemies, 2);
                    break;

                case PlayerState.Dash:
                    UpdateDash(gameTime, currentLayer);
                    break;

                case PlayerState.Hurt:
                    UpdateHurt(gameTime, currentLayer);
                    break;

                case PlayerState.Dead:
                    // Play death animation until end, then stop
                    // The AnimationManager handles the "Stop at last frame" if looping is false
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
        private void StartDash()
        {
            CurrentState = PlayerState.Dash;
            _stateTimer = 500f; 
            _dashDirection = _facingDirection;
            if (_velocity != Vector2.Zero) _dashDirection = Vector2.Normalize(_velocity);
        }
        private void UpdateDash(GameTime gameTime, TileLayer layer)
        {
            float dt  = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _dashTimer -= dt;

            Vector2 dashStep = _dashDirection * _dashSpeed;

            position += dashStep;

            if (_dashTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
                _velocity = Vector2.Zero;
            }
        }
        private void UpdateHurt(GameTime gameTime, TileLayer currentLayer)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            HandleInput(currentLayer);

            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
            }
        }

        private void HandleInput(TileLayer currentLayer)
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

            if (_velocity != Vector2.Zero)
                CurrentState = PlayerState.Run;
            else
                CurrentState = PlayerState.Idle;
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
            // 1. LOCK DIRECTION
            // We calculate this ONCE when the attack starts.
            Vector2 dirToMouse = _mouseWorldPosition - this.Center;

            // Update the facing direction logic
            if (dirToMouse.X < 0)
            {
                _flipEffect = SpriteEffects.FlipHorizontally;
                _facingDirection = new Vector2(-1, 0); // Force left
            }
            else
            {
                _flipEffect = SpriteEffects.None;
                _facingDirection = new Vector2(1, 0); // Force right
            }

            // 2. SET STATE & TIMER
            if (attackNum == 1)
            {
                CurrentState = PlayerState.Attack1;
                // 10 frames * 100ms = 1000ms
                _stateTimer = 1000f;
                _comboBuffered = false;
            }
            else if (attackNum == 2)
            {
                CurrentState = PlayerState.Attack2;
                _stateTimer = 1000f;
            }

            // 3. DEAL DAMAGE
            PerformSwordHit(enemies);
        }
        private void UpdateAttack(GameTime gameTime, List<Enemy> enemies, int attackNum)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _stateTimer -= dt;

            // COMBO CHECK (Buffer input)
            float progress = 1f - (_stateTimer / (attackNum == 1 ? 800f : 600f));

            if (attackNum == 1 && (Keyboard.GetState().IsKeyDown(Keys.Space) || Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                // Allow combo input after 50% of animation
                if (progress > 0.5f) _comboBuffered = true;
            }

            if (_stateTimer <= 0)
            {
                if (attackNum == 1 && _comboBuffered)
                {
                    StartAttack(enemies, 2); // Chain into Attack 2
                }
                else
                {
                    CurrentState = PlayerState.Idle;
                    _cooldownTimer = GameConstants.SwordCooldown;
                }
            }
        }
        private void PerformSwordHit(List<Enemy> enemies)
        {
            // 1. CALCULATE CHEST POSITION
            // 'position' is the Feet.
            // We want to swing from the Chest, which is roughly 60-80 pixels UP.
            Vector2 chestPosition = new Vector2(position.X, position.Y - 80);

            // 2. DEFINE HITBOX
            // We extend the reach OUT from the Chest.
            float reach = 120f;
            Vector2 hitBoxCenter = chestPosition + (_facingDirection * (reach / 2));

            Rectangle swordHitbox = new Rectangle(
                (int)(hitBoxCenter.X - reach / 2),
                (int)(hitBoxCenter.Y - reach / 2),
                (int)reach,
                (int)reach
            );

            // 3. CHECK COLLISIONS
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                // ENEMY HITBOX (Assuming Enemy Position is also Feet)
                // We construct a box that covers the Enemy's body (Feet up to Head)
                Rectangle enemyRect = new Rectangle(
                    (int)enemy.Position.X - 40, // Center X
                    (int)enemy.Position.Y - 100, // Top of head (Feet - Height)
                    80, // Width
                    100 // Height
                );

                if (swordHitbox.Intersects(enemyRect))
                {
                    CombatSystem.DealDamage(this, enemy, GameConstants.SwordDamage);

                    Vector2 knockbackDir = enemy.Position - this.position;
                    if (knockbackDir != Vector2.Zero) knockbackDir.Normalize();
                    enemy.ApplyKnockback(knockbackDir * GameConstants.SwordKnockback);
                }
            }
        }


        // ===================
        // INTERFACE METHODS
        // ===================
        public void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;
            Health -= amount;

            if (Health <= 0)
            {
                Die();
            }
            else
            {
                // Trigger Hurt Animation
                CurrentState = PlayerState.Hurt;
                _stateTimer = 450f; // 3 frames * 150ms
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
            // 1. Draw Player (Anchored at Feet)
            _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect);

            // 2. Draw Health Bar
            if (IsAlive)
            {
                // Draw 120 pixels ABOVE the feet (roughly over the head)
                int barY = (int)position.Y - 120;
                int barWidth = (int)(32 * Scale);
                int barX = (int)position.X - (barWidth / 2); // Centered

                spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, barWidth, 8), Color.DarkRed);
                float healthPercent = (float)Health / MaxHealth;
                spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, (int)(barWidth * healthPercent), 8), Color.Gold);
            }

        
             spriteBatch.Draw(_healthBarTexture, new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4), Color.Cyan);
        }
    }
}