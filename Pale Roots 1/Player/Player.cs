using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    /// <summary>
    /// Player character with WASD movement, tile collision, and combat.
    /// Implements ICombatant so enemies can target and attack the player.
    /// 
    /// Merged functionality from old Player and PlayerWithWeapon classes.
    /// </summary>
    public class Player : Sprite, ICombatant
    {
        // ===================
        // MOVEMENT
        // ===================
        
        private float _speed;
        private Vector2 _velocity;

        // ===================
        // ICOMBATANT IMPLEMENTATION
        // ===================
        
        public string Name { get; set; } = "Player";
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
        // Center inherited from Sprite

        // ===================
        // WEAPON
        // ===================
        
        public Projectile MyProjectile { get; set; }
        private float _attackCooldown = 0f;
        private KeyboardState _previousKeyState;

        // ===================
        // HEALTH BAR
        // ===================
        
        private static Texture2D _healthBarTexture;

        // ===================
        // COMPUTED PROPERTIES
        // ===================
        
        public Vector2 CentrePos => Center;

        // ===================
        // CONSTRUCTOR
        // ===================
        
        public Player(Game game, Texture2D texture, Vector2 startPosition, int frameCount)
            : base(game, texture, startPosition, frameCount, 1)
        {
            _speed = GameConstants.DefaultPlayerSpeed;
            MaxHealth = GameConstants.DefaultHealth;
            _health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            
            // Scale to reasonable size
            Scale = 90.0f / spriteHeight;
            
            // Create shared health bar texture
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        // ===================
        // UPDATE
        // ===================
        
        /// <summary>
        /// Update with tile collision
        /// </summary>
        public void Update(GameTime gameTime, TileLayer currentLayer)
        {
            if (!IsAlive)
            {
                // Could play death animation here
                return;
            }
            
            // Attack cooldown
            if (_attackCooldown > 0)
                _attackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            HandleInput(currentLayer);
            HandleCombat(gameTime);
            UpdateProjectile(gameTime);
            
            // Store keyboard state for next frame
            _previousKeyState = Keyboard.GetState();
            
            // Animate only when moving
            if (_velocity != Vector2.Zero)
                base.Update(gameTime);
        }

        /// <summary>
        /// Handle WASD movement with collision
        /// </summary>
        private void HandleInput(TileLayer currentLayer)
        {
            Vector2 inputDirection = Vector2.Zero;
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W)) inputDirection.Y -= 1;
            if (state.IsKeyDown(Keys.S)) inputDirection.Y += 1;
            if (state.IsKeyDown(Keys.A)) inputDirection.X -= 1;
            if (state.IsKeyDown(Keys.D)) inputDirection.X += 1;

            if (inputDirection != Vector2.Zero)
                inputDirection.Normalize();

            _velocity = inputDirection * _speed;
            Vector2 proposedPosition = position + _velocity;

            // Collision detection
            if (currentLayer != null)
            {
                if (CanMoveTo(proposedPosition, currentLayer))
                {
                    position = proposedPosition;
                }
            }
            else
            {
                position = proposedPosition;
            }
        }

        /// <summary>
        /// Check if player can move to position (4-corner collision)
        /// </summary>
        private bool CanMoveTo(Vector2 newPos, TileLayer layer)
        {
            float visualW = spriteWidth * (float)Scale;
            float visualH = spriteHeight * (float)Scale;

            // Hitbox is smaller than visual (40% width, 70% height)
            float hitboxW = visualW * 0.4f;
            float hitboxH = visualH * 0.7f;

            // Check all 4 corners
            Vector2 topLeft = new Vector2(newPos.X - hitboxW / 2, newPos.Y - hitboxH / 2);
            Vector2 topRight = new Vector2(newPos.X + hitboxW / 2, newPos.Y - hitboxH / 2);
            Vector2 bottomLeft = new Vector2(newPos.X - hitboxW / 2, newPos.Y + hitboxH / 2);
            Vector2 bottomRight = new Vector2(newPos.X + hitboxW / 2, newPos.Y + hitboxH / 2);

            return IsWalkable(topLeft, layer) &&
                   IsWalkable(topRight, layer) &&
                   IsWalkable(bottomLeft, layer) &&
                   IsWalkable(bottomRight, layer);
        }

        private bool IsWalkable(Vector2 pixelPos, TileLayer layer)
        {
            int tx = (int)(pixelPos.X / GameConstants.TileSize);
            int ty = (int)(pixelPos.Y / GameConstants.TileSize);

            if (tx < 0 || tx >= layer.Tiles.GetLength(1) || 
                ty < 0 || ty >= layer.Tiles.GetLength(0))
                return false;

            return layer.Tiles[ty, tx].Passable;
        }

        // ===================
        // COMBAT
        // ===================
        
        private void HandleCombat(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            
            // Melee attack on Space
            if (state.IsKeyDown(Keys.Space) && _previousKeyState.IsKeyUp(Keys.Space))
            {
                if (_attackCooldown <= 0)
                {
                    // Find nearest enemy in melee range
                    // (This would need access to enemies list - 
                    //  could use events or pass in from engine)
                    PerformAttack();
                }
            }
        }

        private void UpdateProjectile(GameTime gameTime)
        {
            if (MyProjectile == null) return;
            
            if (MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL)
            {
                MyProjectile.position = this.Center;
            }
            
            MyProjectile.Update(gameTime);
        }

        public void LoadProjectile(Projectile p)
        {
            MyProjectile = p;
        }

        // ===================
        // ICOMBATANT METHODS
        // ===================
        
        public void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;
            
            Health -= amount;
            
            // Visual feedback - screen shake, flash red, etc.
            
            if (Health <= 0)
            {
                Die();
            }
        }

        public void PerformAttack()
        {
            if (_attackCooldown > 0) return;
            
            // If we have a target, attack it
            if (CurrentTarget != null && CombatSystem.CanAttack(this, CurrentTarget))
            {
                CombatSystem.DealDamage(this, CurrentTarget, AttackDamage);
            }
            
            _attackCooldown = GameConstants.DefaultAttackCooldown;
        }

        public void Die()
        {
            // Game over logic would go here
            Visible = false;
            CombatSystem.ClearTarget(this);
        }

        // ===================
        // DRAW
        // ===================
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            if (MyProjectile != null && 
                MyProjectile.ProjectileState != Projectile.PROJECTILE_STATE.STILL)
            {
                MyProjectile.Draw(spriteBatch);
            }
            
            if (IsAlive)
            {
                DrawHealthBar(spriteBatch);
            }
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            int barWidth = (int)(spriteWidth * Scale);
            int barHeight = 8;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - (int)(spriteHeight * Scale / 2) - 15;

            // Background
            spriteBatch.Draw(_healthBarTexture, 
                new Rectangle(barX, barY, barWidth, barHeight), 
                Color.DarkRed);

            // Health (gold for player)
            float healthPercent = (float)Health / MaxHealth;
            int currentBarWidth = (int)(barWidth * healthPercent);
            
            spriteBatch.Draw(_healthBarTexture, 
                new Rectangle(barX, barY, currentBarWidth, barHeight), 
                Color.Gold);
        }
    }
}
