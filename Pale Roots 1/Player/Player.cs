using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Player controlled character.
    // Implements combat interface so it participates in the CombatSystem.
    // Uses Sprite for position and drawing.
    public class Player : Sprite, ICombatant
    {
        // ICombatant contract requiremnets
        public string Name { get; set; } = "Hero";
        public float DamageMultiplier { get; set; } = 1.0f;
        public CombatTeam Team => CombatTeam.Player;
        public int MaxHealth { get; set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => Health > 0;
        public bool IsActive => Visible;
        public ICombatant CurrentTarget { get; set; }

        // internal sprite properties
        public new Vector2 Position { get => position; set => position = value; }
        public Game Game => game;
        public Vector2 CentrePos => Center;

        // movement and physics
        private float _speed;
        private Vector2 _velocity;

        // External momentum from forces or knockback.
        private Vector2 _externalVelocity = Vector2.Zero;

        // Controls what the player can do each frame.
        public enum PlayerState { Idle, Run, Attack1, Attack2, Dash, Hurt, Dead }
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        private Vector2 _mouseWorldPosition;
        private Vector2 _facingDirection = new Vector2(0, 1);
        public bool IsHeavyAttackUnlocked { get; set; } = false;
        public bool IsDashUnlocked { get; set; } = false;

        // Dash mechanics
        private float _dashSpeed = 12f;
        private Vector2 _dashDirection;
        private float _dashCooldownTimer = 0f;
        private float _dashCooldownDuration = 800f;
        private bool _isInvincible = false;

        public float DashTimer => _dashCooldownTimer;
        public float DashDuration => _dashCooldownDuration;

        // Timing & logic helpers
        private float _stateTimer = 0f;
        private float _cooldownTimer = 0f;
        private List<ICombatant> _enemiesHitThisAttack = new List<ICombatant>();

        // Animation state
        private int _currentDirectionIndex = 2; // Maps to sprite sheet rows
        private AnimationManager _animManager;
        private SpriteEffects _flipEffect = SpriteEffects.None;

        // Textures & visuals
        private Texture2D _txIdle, _txRun, _txAttack1, _txAttack2, _txHurt, _txDeath, _txDash;
        private static Texture2D _healthBarTexture;

        public Player(Game game, Texture2D texture, Vector2 startPosition, int frameCount)
            : base(game, texture, startPosition, frameCount, 1)
        {
            // Initialize stats from GameConstants.
            _speed = GameConstants.DefaultPlayerSpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            Scale = 3f;

            // Create a shared 1x1 texture for health bars if needed.
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            _animManager = new AnimationManager();

            // Load animation sheets.
            _txIdle = game.Content.Load<Texture2D>("Player/Idle");
            _txRun = game.Content.Load<Texture2D>("Player/Run");
            _txAttack1 = game.Content.Load<Texture2D>("Player/Attack 1");
            _txAttack2 = game.Content.Load<Texture2D>("Player/Attack 2");
            _txHurt = game.Content.Load<Texture2D>("Player/Hurt");
            _txDeath = game.Content.Load<Texture2D>("Player/Death");
            _txDash = game.Content.Load<Texture2D>("Player/Dash");

            // Register animations with the manager.
            int standardWidth = _txIdle.Width / 7;
            _animManager.AddAnimation("Idle", new Animation(_txIdle, 7, 0, 150f, true));
            _animManager.AddAnimation("Run", new Animation(_txRun, 8, 0, 120f, true));
            _animManager.AddAnimation("Attack1", new Animation(_txAttack1, 10, 0, 100f, false));
            _animManager.AddAnimation("Attack2", new Animation(_txAttack2, 15, 0, 100f, false, 1, standardWidth));
            _animManager.AddAnimation("Dash", new Animation(_txDash, 4, 0, 125f, false, 1, standardWidth));
            _animManager.AddAnimation("Hurt", new Animation(_txHurt, 3, 0, 150f, false, 1, standardWidth));
            _animManager.AddAnimation("Death", new Animation(_txDeath, 15, 0, 150f, false, 1, standardWidth));

            _animManager.Play("Idle");
        }

        public void Update(GameTime gameTime, TileLayer currentLayer, List<Enemy> enemies, List<WorldObject> obstacles)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Apply external forces before player input movement.
            if (_externalVelocity != Vector2.Zero)
            {
                Vector2 proposedPos = position + _externalVelocity;
                if (CanMoveTo(proposedPos, currentLayer, obstacles))
                {
                    position = proposedPos;
                }
                else
                {
                    _externalVelocity = Vector2.Zero; // stop on collision
                }

                _externalVelocity *= GameConstants.KnockbackFriction;
                if (_externalVelocity.Length() < 0.1f) _externalVelocity = Vector2.Zero;
            }

            // Update timers
            if (_cooldownTimer > 0) _cooldownTimer -= dt;
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= dt;

            // Convert mouse screen position to a world position relative to the player center.
            MouseState mouseState = Mouse.GetState();
            Vector2 screenCenter = new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
            Vector2 mouseOffset = new Vector2(mouseState.X, mouseState.Y) - screenCenter;
            _mouseWorldPosition = this.Center + mouseOffset;

            // Run logic based on the current finite state machine state.
            switch (CurrentState)
            {
                case PlayerState.Idle:
                case PlayerState.Run:
                    HandleInput(currentLayer, obstacles, true);
                    CheckForCombatInput(enemies);
                    break;

                case PlayerState.Attack1:
                case PlayerState.Attack2:
                    HandleInput(currentLayer, obstacles, false); // allow movement but do not switch to Run
                    UpdateAttack(gameTime, enemies, (CurrentState == PlayerState.Attack1 ? 1 : 2));
                    break;

                case PlayerState.Dash:
                    UpdateDash(gameTime, currentLayer, obstacles);
                    break;

                case PlayerState.Hurt:
                    HandleInput(currentLayer, obstacles, false);
                    UpdateHurt(gameTime);
                    break;
            }

            UpdateAnimation(gameTime);
        }

        private void CheckForCombatInput(List<Enemy> enemies)
        {
            if (IsDashUnlocked && InputEngine.IsActionPressed("Dash"))
            {
                StartDash();
                return;
            }

            if (_cooldownTimer > 0) return;

            if (InputEngine.IsActionPressed("LightAttack"))
            {
                StartAttack(enemies, 1);
            }
            else if (IsHeavyAttackUnlocked && InputEngine.IsActionPressed("HeavyAttack"))
            {
                StartAttack(enemies, 2);
            }
        }

        private void StartDash()
        {
            if (_dashCooldownTimer > 0) return;

            CurrentState = PlayerState.Dash;
            _dashSpeed = 20f;
            _stateTimer = 150f;
            _dashCooldownTimer = _dashCooldownDuration;
            _isInvincible = true;

            // Dash in movement direction or default horizontal direction.
            if (_velocity != Vector2.Zero)
                _dashDirection = Vector2.Normalize(_velocity);
            else
                _dashDirection = (_currentDirectionIndex == 2) ? new Vector2(-1, 0) : new Vector2(1, 0);
        }

        private void UpdateDash(GameTime gameTime, TileLayer layer, List<WorldObject> obstacles)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Vector2 proposedPosition = position + (_dashDirection * _dashSpeed);

            if (layer == null || CanMoveTo(proposedPosition, layer, obstacles))
            {
                position = proposedPosition;
            }

            ClampToMap();

            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
                _velocity = Vector2.Zero;
                _isInvincible = false;
            }
        }

        private void UpdateHurt(GameTime gameTime)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
                _isInvincible = false;
            }
        }

        private void HandleInput(TileLayer currentLayer, List<WorldObject> obstacles, bool updateState)
        {
            Vector2 inputDirection = Vector2.Zero;

            if (InputEngine.IsActionHeld("MoveUp")) inputDirection.Y -= 1;
            if (InputEngine.IsActionHeld("MoveDown")) inputDirection.Y += 1;
            if (InputEngine.IsActionHeld("MoveLeft")) inputDirection.X -= 1;
            if (InputEngine.IsActionHeld("MoveRight")) inputDirection.X += 1;

            if (inputDirection != Vector2.Zero)
            {
                inputDirection.Normalize();
                _facingDirection = inputDirection;
                _velocity = inputDirection * _speed;

                // Do not change facing while attacking.
                if (CurrentState != PlayerState.Attack1 && CurrentState != PlayerState.Attack2)
                {
                    _currentDirectionIndex = GetDirectionFromVector(_facingDirection);
                }

                Vector2 proposedPosition = position + _velocity;
                if (CanMoveTo(proposedPosition, currentLayer, obstacles))
                {
                    position = proposedPosition;
                }
            }
            else
            {
                _velocity = Vector2.Zero;
            }

            ClampToMap();
            if (updateState)
            {
                CurrentState = (_velocity != Vector2.Zero) ? PlayerState.Run : PlayerState.Idle;
            }
        }

        private int GetDirectionFromVector(Vector2 dir)
        {
            // Map vector to sprite sheet row index for facing.
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                return (dir.X > 0) ? 3 : 2;
            else
                return (dir.Y > 0) ? 0 : 1;
        }

        private bool CanMoveTo(Vector2 newPos, TileLayer layer, List<WorldObject> obstacles)
        {
            // Build a small hitbox near the player's feet for collision testing.
            float scale = (float)Scale;
            int playerW = (int)(spriteWidth * scale * 0.4f);
            int playerH = (int)(spriteHeight * scale * 0.2f);
            int playerX = (int)(newPos.X - (playerW / 2));
            int playerY = (int)(newPos.Y + (spriteHeight * scale / 2) - playerH);

            Rectangle futurePlayerBox = new Rectangle(playerX, playerY, playerW, playerH);

            // Check bounds against the map size.
            float mapWidth = layer.Tiles.GetLength(1) * 64;
            float mapHeight = layer.Tiles.GetLength(0) * 64;
            if (newPos.X < 0 || newPos.Y < 0 || newPos.X > mapWidth || newPos.Y > mapHeight) return false;

            // Check collisions with world objects and return the negated result.
            return !IsColliding(newPos, obstacles);
        }


        private void StartAttack(List<Enemy> enemies, int attackNum)
        {
            // Face the mouse and lock facing for the attack.
            Vector2 dirToMouse = _mouseWorldPosition - this.Center;
            dirToMouse.Normalize();
            _currentDirectionIndex = GetDirectionFromVector(dirToMouse);

            // Flip sprite for left attacks.
            _flipEffect = (_currentDirectionIndex == 2) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            string animName = (attackNum == 1) ? "Attack1" : "Attack2";
            float duration = (attackNum == 1) ? 1000f : 1500f;

            CurrentState = (attackNum == 1) ? PlayerState.Attack1 : PlayerState.Attack2;
            _stateTimer = duration;
            _enemiesHitThisAttack.Clear();
            _animManager.Play(animName);
        }

        private void UpdateAttack(GameTime gameTime, List<Enemy> enemies, int attackNum)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (attackNum == 1) PerformSwordHit(enemies, 1.0f, 1.0f);
            else PerformSwordHit(enemies, 2.0f, 1.5f);

            if (_stateTimer <= 0)
            {
                CurrentState = PlayerState.Idle;
                _cooldownTimer = GameConstants.SwordCooldown;
            }
        }

        private void PerformSwordHit(List<Enemy> enemies, float damageMult, float knockbackMult)
        {
            // Create a rectangular hitbox in front of the player based on facing.
            Vector2 chestPosition = new Vector2(position.X, position.Y - 50);
            Vector2[] directions = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(1, 0) };
            Vector2 attackDir = directions[_currentDirectionIndex];

            float reach = 80f * knockbackMult;
            Vector2 hitCenter = chestPosition + (attackDir * reach);
            int boxSize = (int)(80 * knockbackMult);

            Rectangle swordHitbox = new Rectangle((int)(hitCenter.X - boxSize / 2), (int)(hitCenter.Y - boxSize / 2), boxSize, boxSize);

            foreach (var enemy in enemies.ToArray())
            {
                if (!enemy.IsAlive || _enemiesHitThisAttack.Contains(enemy)) continue;

                // enemy hurtbox.
                Rectangle enemyRect = new Rectangle((int)enemy.Position.X - 30, (int)enemy.Position.Y - 80, 60, 80);

                if (swordHitbox.Intersects(enemyRect))
                {
                    int finalDamage = (int)(GameConstants.SwordDamage * damageMult * DamageMultiplier);
                    CombatSystem.DealDamage(this, enemy, finalDamage);

                    // Apply knockback away from the player.
                    Vector2 kb = enemy.Position - this.position;
                    if (kb != Vector2.Zero) kb.Normalize();
                    enemy.ApplyKnockback(kb * GameConstants.SwordKnockback * knockbackMult);

                    _enemiesHitThisAttack.Add(enemy);
                }
            }
        }

        public void TakeDamage(int amount, ICombatant attacker)
        {
            if (_isInvincible || !IsAlive) return;

            Health -= amount;
            if (Health <= 0) Die();
            else
            {
                CurrentState = PlayerState.Hurt;
                _stateTimer = 300f;
                _isInvincible = true;
            }
        }

        public void PerformAttack() { } // Attack handled by StartAttack and UpdateAttack

        private void UpdateAnimation(GameTime gameTime)
        {
            string animKey = CurrentState.ToString();
            if (CurrentState == PlayerState.Dead) animKey = "Death";

            // Flip for movement states only.
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
            CurrentState = PlayerState.Dead;
            CombatSystem.ClearTarget(this);
        }

        public void ApplyExternalForce(Vector2 force) => _externalVelocity += force;
        public void ClearExternalForces() => _externalVelocity = Vector2.Zero;

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the sprite offset so the feet align with the world position.
            Vector2 drawPos = position + new Vector2(0, 175);
            _animManager.Draw(spriteBatch, drawPos, (float)Scale, _flipEffect, _currentDirectionIndex);
        }
    }
}