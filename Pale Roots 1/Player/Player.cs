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
        private Texture2D _swordTexture;
        public enum PlayerState
        {
            Idle,
            Run,
            Attack1,
            Attack2,
            Jump,
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
        private float _swordRotation = 0f;

        // animation stuff

        public enum Direction {Down = 0, Left = 0, Right = 2, Up = 3 }
        private Direction _currentDirection = Direction.Down;

        private Texture2D _txIdle;
        private Texture2D _txRun;
        private Texture2D _txAttack1;
        private Texture2D _txAttack2; // Optional, if you want combo attacks
        private Texture2D _txHurt;
        private Texture2D _txDeath;
        private Texture2D _txJump;
        private Texture2D _txDash;

        private AnimationManager _animManager;
        private SpriteEffects _flipEffect = SpriteEffects.None;

        // --- FIX: CENTERED PIVOT ---
        // (0, -32) moves the pivot from the feet (Center) up to the Chest/Neck.
        // It is centered on the X axis (0) so it swings evenly left and right.
        private Vector2 _pivotOffset = new Vector2(0, -32);

        // HEALTH BAR TEXTURE
        private static Texture2D _healthBarTexture;

        // ===================
        // CONSTRUCTOR
        // ===================
        public Player(Game game, Texture2D texture, Texture2D swordTexture, Vector2 startPosition, int frameCount)
            : base(game, texture, startPosition, frameCount, 1)
        {
            _swordTexture = swordTexture;
            _speed = GameConstants.DefaultPlayerSpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;

            Scale = 3f; // Adjust as needed based sprite Size

            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            _animManager = new AnimationManager();

            // 1. Load the specific strips
            _txIdle = game.Content.Load<Texture2D>("Player/Idle");
            _txRun = game.Content.Load<Texture2D>("Player/Run");
            _txAttack1 = game.Content.Load<Texture2D>("Player/Attack 1");
            _txAttack2 = game.Content.Load<Texture2D>("Player/Attack 2");
            _txHurt = game.Content.Load<Texture2D>("Player/Hurt"); 
            _txDeath = game.Content.Load<Texture2D>("Player/Death");
            _txAttack1 = game.Content.Load<Texture2D>("Player/Attack 1");
            _txJump = game.Content.Load<Texture2D>("Player/Jump");
            _txDash = game.Content.Load<Texture2D>("Player/Dash");

            // 2. Register Animations
            // Format: Texture, FrameCount, Row(0), Speed(ms per frame), IsLooping

            //_animManager.AddAnimation("Idle", new Animation(_txIdle, 7, 0, 100f, true, 1, 125));
            //_animManager.AddAnimation("Run", new Animation(_txRun, 8, 0, 80f, true, 1));
            //_animManager.AddAnimation("Attack1", new Animation(_txAttack1, 10, 0, 80f, false, 1, 0));
            //_animManager.AddAnimation("Hurt", new Animation(_txHurt, 3, 0, 150f, false, 1));
            //_animManager.AddAnimation("Death", new Animation(_txDeath, 15, 0, 150f, false, 1));


            //_animManager.AddAnimation("Idle", new Animation(_txIdle, 7, 0, 100f, true, 1));
            //_animManager.AddAnimation("Run", new Animation(_txRun, 8, 0, 80f, true, 1));

            //_animManager.AddAnimation("Attack1", new Animation(_txAttack1, 10, 0, 80f, false, 1));
            //_animManager.AddAnimation("Attack2", new Animation(_txAttack2, 10, 0, 60f, false));

            //_animManager.AddAnimation("Hurt", new Animation(_txHurt, 3, 0, 150f, false, 1));
            //_animManager.AddAnimation("Death", new Animation(_txDeath, 15, 0, 150f, false, 1));

            //_animManager.AddAnimation("Jump", new Animation(_txJump, 2, 0, 150f, false));
            //_animManager.AddAnimation("Dash", new Animation(_txDash, 4, 0, 100f, false));

            int standardWidth = _txIdle.Width / 7;

            _animManager.AddAnimation("Idle", new Animation(_txIdle, 7, 0, 100f, true));
            _animManager.AddAnimation("Run", new Animation(_txRun, 8, 0, 80f, true));

            // Combat
            // We pass 'standardWidth' as the last argument to force the center point to stay consistent
            // Arguments: Texture, Frames, Row, Speed, Loop, TotalRows, CustomWidth
            _animManager.AddAnimation("Attack1", new Animation(_txAttack1, 10, 0, 80f, false, 1, standardWidth));
            _animManager.AddAnimation("Attack2", new Animation(_txAttack2, 10, 0, 60f, false, 1, standardWidth));

            // Actions
            _animManager.AddAnimation("Jump", new Animation(_txJump, 2, 0, 150f, false, 1, standardWidth));
            _animManager.AddAnimation("Dash", new Animation(_txDash, 4, 0, 100f, false, 1, standardWidth));

            // Damage
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

            //STATE MACHINE
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

                case PlayerState.Jump:
                    UpdateJump(gameTime);
                    break;

                case PlayerState.Hurt:
                    UpdateHurt(gameTime);
                    break;

                case PlayerState.Dead:
                    // Do nothing, just wait
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
            else if ( kState.IsKeyDown(Keys.F))
            {
                StartJump();
            }
        }
        private void StartDash()
        {
            CurrentState = PlayerState.Dash;
            _dashTimer = 400f;
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
        private void StartJump()
        {
            CurrentState = PlayerState.Jump;
            _stateTimer = 300f;
        }
        private void UpdateHurt(GameTime gameTime)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
            }
        }
        private void UpdateJump(GameTime gameTime)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            HandleInput(null);
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
            if (attackNum == 1)
            {
                CurrentState = PlayerState.Attack1;
                _stateTimer = 800f; // Duration of Attack 1
                _comboBuffered = false;
            }
            else if (attackNum == 2)
            {
                CurrentState = PlayerState.Attack2;
                _stateTimer = 600f; // Duration of Attack 2
            }

            // Calculate Sword Rotation logic here (same as your old code)
            // ...

            // Deal Damage immediately (or you can time this to the frame)
            PerformSwordHit(enemies);
        }
        private void UpdateAttack(GameTime gameTime, List<Enemy> enemies, int attackNum)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _stateTimer -= dt;

            // SWORD ANIMATION LERP
            float progress = 1f - (_stateTimer / 600f);
            _swordRotation = MathHelper.Lerp(-1.5f, 1.5f, progress);

            // COMBO CHECK
            // If we are in Attack 1, and player clicks, buffer the combo
            if (attackNum == 1 && (Keyboard.GetState().IsKeyDown(Keys.Space) || Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                // Only buffer if we are halfway through the swing
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
            // 1. Calculate Pivot and Direction
            Vector2 truePivot = this.Center + _pivotOffset;

            // 2. Define Hitbox parameters
            float reach = 60f;
            Vector2 hitBoxCenter = truePivot + (_facingDirection * (reach / 2));

            Rectangle swordHitbox = new Rectangle(
                (int)(hitBoxCenter.X - reach / 2),
                (int)(hitBoxCenter.Y - reach / 2),
                (int)reach,
                (int)reach
            );

            // 3. Check Collisions
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                // Simple bounding box check
                Rectangle enemyRect = new Rectangle(
                    (int)enemy.Position.X,
                    (int)enemy.Position.Y,
                    enemy.spriteWidth,
                    enemy.spriteHeight
                );

                if (swordHitbox.Intersects(enemyRect))
                {
                    // Deal Damage
                    CombatSystem.DealDamage(this, enemy, GameConstants.SwordDamage);

                    // Apply Knockback
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
            if (Health <= 0) Die();
        }

        public void PerformAttack() { }

        private void UpdateAnimation(GameTime gameTime)
        {
            string animKey = "Idle";

            switch (CurrentState)
            {
                case PlayerState.Idle:
                    animKey = "Idle";
                    break;
                case PlayerState.Run:
                    animKey = "Run";
                    break;
                case PlayerState.Attack1:
                    animKey = "Attack1";
                    break;
                case PlayerState.Attack2:
                    animKey = "Attack2";
                    break;
                case PlayerState.Jump:
                    animKey = "Jump";
                    break;
                case PlayerState.Dash:
                    animKey = "Dash";
                    break;
                case PlayerState.Hurt:
                    animKey = "Hurt";
                    break;
                case PlayerState.Dead:
                    animKey = "Death";
                    break;
            }

            // Flip logic
            if (_velocity.X < -0.1f) _flipEffect = SpriteEffects.FlipHorizontally;
            else if (_velocity.X > 0.1f) _flipEffect = SpriteEffects.None;

            _animManager.Play(animKey);
            _animManager.Update(gameTime);
        }

        public void Die()
        {
            Visible = false;
            CombatSystem.ClearTarget(this);
        }

        // ===================
        // DRAW
        // ===================
        public override void Draw(SpriteBatch spriteBatch)
        {
            // 1. Draw the Player Animation
            _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect);

            // 2. Draw the Sword ONLY if we are in an Attack State
            if (CurrentState == PlayerState.Attack1 || CurrentState == PlayerState.Attack2)
            {
                Vector2 truePivot = this.Center + _pivotOffset;
                float armLength = 0f;
                Vector2 pivotPoint = truePivot + (_facingDirection * armLength);
                Vector2 swordHandleOrigin = new Vector2(0, _swordTexture.Height / 2);

                // Calculate Angle
                float baseAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                float finalAngle = baseAngle + _swordRotation;

                // Draw Sword
                spriteBatch.Draw(_swordTexture,
                    pivotPoint,
                    null,
                    Color.White,
                    finalAngle,
                    swordHandleOrigin,
                    1.0f,
                    SpriteEffects.None,
                    0f);
            }

            // 3. Draw Health Bar
            if (IsAlive)
            {
                int barWidth = (int)(32 * Scale);
                int barY = (int)position.Y - 15;
                spriteBatch.Draw(_healthBarTexture, new Rectangle((int)position.X, barY, barWidth, 8), Color.DarkRed);
                float healthPercent = (float)Health / MaxHealth;
                spriteBatch.Draw(_healthBarTexture, new Rectangle((int)position.X, barY, (int)(barWidth * healthPercent), 8), Color.Gold);
            }
        }
    }
}