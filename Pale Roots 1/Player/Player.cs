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
        // ICombatant Properties (REQUIRED TO FIX ERROR)
        // ===================
        public string Name { get; set; } = "Hero";
        public CombatTeam Team => CombatTeam.Player;

        private int _health;
        public int Health
        {
            get => _health;
            set => _health = Math.Max(0, value);
        }

        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => _health > 0;
        public bool IsActive => Visible;
        public ICombatant CurrentTarget { get; set; }
        public int AttackerCount { get; set; }
        public Vector2 Position => position;

        // Helper property for center
        public Vector2 CentrePos => Center;

        // ===================
        // MOVEMENT & SWORD FIELDS
        // ===================
        private float _speed;
        private Vector2 _velocity;

        // SWORD FIELDS
        private Texture2D _swordTexture;
        private bool _isAttacking = false;
        private float _swingTimer = 0f;
        private float _cooldownTimer = 0f;
        private Vector2 _facingDirection = new Vector2(0, 1); // Default facing Down
        private float _swordRotation = 0f;

        // HEALTH BAR TEXTURE
        private static Texture2D _healthBarTexture;

        // ===================
        // CONSTRUCTOR
        // ===================
        // Note: We added swordTexture to the arguments here!
        public Player(Game game, Texture2D texture, Texture2D swordTexture, Vector2 startPosition, int frameCount)
            : base(game, texture, startPosition, frameCount, 1)
        {
            _swordTexture = swordTexture;
            _speed = GameConstants.DefaultPlayerSpeed;
            MaxHealth = GameConstants.DefaultHealth;
            _health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;

            Scale = 90.0f / spriteHeight;

            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        // ===================
        // UPDATE
        // ===================
        public void Update(GameTime gameTime, TileLayer currentLayer, List<Enemy> enemies)
        {
            if (!IsAlive) return;

            // 1. Handle Cooldowns
            if (_cooldownTimer > 0) _cooldownTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // 2. Handle Attack Input
            if (!_isAttacking && _cooldownTimer <= 0 &&
               (Keyboard.GetState().IsKeyDown(Keys.Space) || Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                StartAttack(enemies);
            }

            // 3. Update Attack Animation
            if (_isAttacking)
            {
                _swingTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                // Calculate sword rotation (-45 to +45 degrees)
                float progress = 1f - (_swingTimer / GameConstants.SwordSwingDuration);
                _swordRotation = MathHelper.Lerp(-1.5f, 1.5f, progress); // Wider swing

                if (_swingTimer <= 0)
                {
                    _isAttacking = false;
                    _cooldownTimer = GameConstants.SwordCooldown;
                }
            }

            // 4. Movement (Only move if NOT attacking)
            if (!_isAttacking)
            {
                HandleInput(currentLayer);
                // Only animate legs if moving
                if (_velocity != Vector2.Zero) base.Update(gameTime);
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
                _facingDirection = inputDirection; // Remember direction
                _velocity = inputDirection * _speed;

                // Simple collision check
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
        }

        private bool CanMoveTo(Vector2 newPos, TileLayer layer)
        {
            // Simple center point check for now to save time
            int tx = (int)((newPos.X + spriteWidth / 2) / GameConstants.TileSize);
            int ty = (int)((newPos.Y + spriteHeight / 2) / GameConstants.TileSize);

            if (tx < 0 || tx >= layer.Tiles.GetLength(1) || ty < 0 || ty >= layer.Tiles.GetLength(0))
                return false;

            return layer.Tiles[ty, tx].Passable;
        }

        // ===================
        // COMBAT LOGIC
        // ===================
        private void StartAttack(List<Enemy> enemies)
        {
            _isAttacking = true;
            _swingTimer = GameConstants.SwordSwingDuration;

            // Create Hitbox in front of player
            Vector2 hitBoxCenter = this.Center + (_facingDirection * GameConstants.SwordRange);
            Rectangle swordHitbox = new Rectangle(
                (int)(hitBoxCenter.X - GameConstants.SwordArcWidth / 2),
                (int)(hitBoxCenter.Y - GameConstants.SwordArcWidth / 2),
                (int)GameConstants.SwordArcWidth,
                (int)GameConstants.SwordArcWidth
            );

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                Rectangle enemyRect = new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, enemy.spriteWidth, enemy.spriteHeight);

                if (swordHitbox.Intersects(enemyRect))
                {
                    CombatSystem.DealDamage(this, enemy, GameConstants.SwordDamage);

                    // Knockback
                    Vector2 knockbackDir = enemy.Position - this.position;
                    if (knockbackDir != Vector2.Zero) knockbackDir.Normalize();
                    enemy.position += knockbackDir * GameConstants.SwordKnockback;
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

        public void PerformAttack() { /* Used for AI, not player input */ }

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
            base.Draw(spriteBatch);

            // Draw Sword
            if (_isAttacking)
            {
                // Pivot at the handle (Left Middle of image)
                Vector2 origin = new Vector2(0, _swordTexture.Height / 2);
                float baseAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                float finalAngle = baseAngle + _swordRotation;

                spriteBatch.Draw(_swordTexture, this.Center, null, Color.White, finalAngle, origin, 1.0f, SpriteEffects.None, 0f);
            }

            // Draw Health Bar
            if (IsAlive)
            {
                int barWidth = (int)(spriteWidth * Scale);
                int barY = (int)position.Y - 15;
                spriteBatch.Draw(_healthBarTexture, new Rectangle((int)position.X, barY, barWidth, 8), Color.DarkRed);
                float healthPercent = (float)Health / MaxHealth;
                spriteBatch.Draw(_healthBarTexture, new Rectangle((int)position.X, barY, (int)(barWidth * healthPercent), 8), Color.Gold);
            }
        }
    }
}