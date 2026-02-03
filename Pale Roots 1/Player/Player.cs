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

        private int _health;
        private Vector2 _mouseWorldPosition;

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
        private Vector2 _facingDirection = new Vector2(0, 1);
        private float _swordRotation = 0f;

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

            if (_cooldownTimer > 0) _cooldownTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            MouseState mouseState = Mouse.GetState();
            Vector2 screenCenter = new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
            Vector2 mouseOffset = new Vector2(mouseState.X, mouseState.Y) - screenCenter;
            _mouseWorldPosition = this.Center + mouseOffset;

            if (!_isAttacking && _cooldownTimer <= 0 &&
               (Keyboard.GetState().IsKeyDown(Keys.Space) || Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                StartAttack(enemies);
            }

            if (_isAttacking)
            {
                _swingTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                float progress = 1f - (_swingTimer / GameConstants.SwordSwingDuration);
                _swordRotation = MathHelper.Lerp(-1.5f, 1.5f, progress);

                if (_swingTimer <= 0)
                {
                    _isAttacking = false;
                    _cooldownTimer = GameConstants.SwordCooldown;
                }
            }

            if (!_isAttacking)
            {
                HandleInput(currentLayer);
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
        }

        private bool CanMoveTo(Vector2 newPos, TileLayer layer)
        {
            //int tx = (int)((newPos.X + spriteWidth / 2) / GameConstants.TileSize);
            //int ty = (int)((newPos.Y + spriteHeight / 2) / GameConstants.TileSize);

            //float scaledCenterX = newPos.X + (spriteWidth * (float)Scale) / 2.0f;
            //float scaledCenterY = newPos.Y + (spriteHeight * (float)Scale) / 2.0f;

            //int tx = (int)(scaledCenterX / GameConstants.TileSize);
            //int ty = (int)(scaledCenterY / GameConstants.TileSize);

            //if (tx < 0 || tx >= layer.Tiles.GetLength(1) || ty < 0 || ty >= layer.Tiles.GetLength(0))
            //    return false;

            //return layer.Tiles[ty, tx].Passable;

            float feetY = newPos.Y + (spriteHeight * (float)Scale);
            float headY = newPos.Y + (spriteHeight * (float)Scale) * 0.5f;

            float centerX = newPos.X + (spriteWidth * (float)Scale) / 2.0f;

            float widthBuffer = 15f;

            int leftTileX = (int)((centerX - widthBuffer) / GameConstants.TileSize);
            int rightTileX = (int)((centerX + widthBuffer) / GameConstants.TileSize);

            int feetTileY = (int)((feetY - 5) / GameConstants.TileSize);

            if (leftTileX < 0 || rightTileX >= layer.Tiles.GetLength(1) || feetTileY < 0 || feetTileY >= layer.Tiles.GetLength(0))
            {
                return false;
            }
            Tile tileLeft = layer.Tiles[feetTileY, leftTileX];
            Tile tileRight = layer.Tiles[feetTileY, rightTileX];

            if (!tileLeft.Passable || !tileRight.Passable)
            {
                return false;
            }
            return true;
        }

        // ===================
        // COMBAT LOGIC
        // ===================
        private void StartAttack(List<Enemy> enemies)
        {
            _isAttacking = true;
            _swingTimer = GameConstants.SwordSwingDuration;

            // 1. PIVOT: Center of the Chest
            Vector2 truePivot = this.Center + _pivotOffset;

            // 2. DIRECTION: Calculated from Chest to Mouse
            Vector2 directionToMouse = _mouseWorldPosition - truePivot;

            if (directionToMouse != Vector2.Zero)
            {
                directionToMouse.Normalize();
                _facingDirection = directionToMouse;
            }

            // 3. ARM LENGTH: Push sword 40px away from body (Orbit Radius)
            float armLength = 0f;
            Vector2 pivotPoint = truePivot + (_facingDirection * armLength);

            // 4. HITBOX: Centered on the blade
            float reach = 60f;
            Vector2 hitBoxCenter = pivotPoint + (_facingDirection * (reach / 2));

            Rectangle swordHitbox = new Rectangle(
                (int)(hitBoxCenter.X - reach / 2),
                (int)(hitBoxCenter.Y - reach / 2),
                (int)reach,
                (int)reach
            );

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                Rectangle enemyRect = new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, enemy.spriteWidth, enemy.spriteHeight);

                if (swordHitbox.Intersects(enemyRect))
                {
                    CombatSystem.DealDamage(this, enemy, GameConstants.SwordDamage);

                    Vector2 knockbackDir = enemy.Position - this.position;
                    if (knockbackDir != Vector2.Zero) knockbackDir.Normalize();

                    if (enemy is Enemy enemyObj)
                    {
                        enemyObj.ApplyKnockback(knockbackDir * GameConstants.SwordKnockback);
                    }
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

            if (_isAttacking)
            {
                // 1. PIVOT
                Vector2 truePivot = this.Center + _pivotOffset;

                // 2. ARM LENGTH (Orbit Radius)
                float armLength = 0f;
                Vector2 pivotPoint = truePivot + (_facingDirection * armLength);

                // 3. HANDLE ORIGIN (Hilt is at 0)
                Vector2 swordHandleOrigin = new Vector2(0, _swordTexture.Height / 2);

                // 4. ROTATION
                float baseAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                float finalAngle = baseAngle + _swordRotation;

                // 5. DEBUG DOT (Should be on Chest)
                spriteBatch.Draw(_healthBarTexture,
                    new Rectangle((int)truePivot.X - 2, (int)truePivot.Y - 2, 4, 4),
                    Color.Red);

                // 6. DRAW SWORD
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