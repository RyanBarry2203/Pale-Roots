using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    /// <summary>
    /// Base enemy class with full state machine for AI behavior.
    /// Implements ICombatant for standardized combat interactions.
    /// 
    /// STATE MACHINE:
    /// - Charging: Initial rush toward enemy lines
    /// - Chasing: Pursuing a specific target
    /// - InCombat: Actively fighting a target
    /// - Wandering: No target, moving randomly
    /// </summary>
    public class Enemy : RotatingSprite, ICombatant
    {
        // ===================
        // ENUMS
        // ===================
        
        public enum ENEMYSTATE { ALIVE, DYING, DEAD }
        public enum AISTATE { Charging, Chasing, InCombat, Wandering }

        private Vector2 _knockBackVelocity;

        // ===================
        // STATE
        // ===================
        
        private ENEMYSTATE _lifecycleState = ENEMYSTATE.ALIVE;
        public ENEMYSTATE LifecycleState 
        { 
            get => _lifecycleState; 
            set => _lifecycleState = value; 
        }
        
        // Keep old name for compatibility
        public ENEMYSTATE EnemyStateza 
        { 
            get => _lifecycleState; 
            set => _lifecycleState = value; 
        }

        public AISTATE CurrentAIState { get; set; } = AISTATE.Charging;

        // ===================
        // ICOMBATANT IMPLEMENTATION
        // ===================
        
        public string Name { get; set; } = "Enemy";
        public CombatTeam Team => CombatTeam.Enemy;
        
        private int _health;
        public int Health 
        { 
            get => _health; 
            set => _health = Math.Max(0, value); 
        }
        
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => _health > 0 && _lifecycleState == ENEMYSTATE.ALIVE;
        public bool IsActive => Visible && _lifecycleState != ENEMYSTATE.DEAD;
        
        private ICombatant _currentTarget;
        public ICombatant CurrentTarget 
        { 
            get => _currentTarget;
            set => _currentTarget = value;
        }
        
        // Legacy property for compatibility
        public Sprite CurrentCombatPartner
        {
            get => _currentTarget as Sprite;
            set => _currentTarget = value as ICombatant;
        }
        
        public int AttackerCount { get; set; }
        
        public Vector2 Position => position;
        // Center is inherited from Sprite

        // ===================
        // MOVEMENT & COMBAT
        // ===================
        
        protected float Velocity;
        protected Vector2 startPosition;
        protected Vector2 wanderTarget;
        
        private float _attackCooldown = 0f;
        private int _deathCountdown;

        // ===================
        // HEALTH BAR
        // ===================
        
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        // ===================
        // CONSTRUCTOR
        // ===================
        
        public Enemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            startPosition = userPosition;
            Velocity = GameConstants.DefaultEnemySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            _health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            CurrentAIState = AISTATE.Charging;
            
            // Create shared health bar texture
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        // ===================
        // UPDATE - Main State Machine
        // ===================
        
        public void Update(GameTime gametime, List<WorldObject> obstacles)
        {
            if (_knockBackVelocity != Vector2.Zero)
            {
                position += _knockBackVelocity;
                _knockBackVelocity *= GameConstants.KnockbackFriction;
                
                // Stop if velocity is very low
                if (_knockBackVelocity.Length() < 0.1f)
                    _knockBackVelocity = Vector2.Zero;
            }
            base.Update(gametime);

            switch (_lifecycleState)
            {
                case ENEMYSTATE.ALIVE:
                    UpdateAI(gametime, obstacles);
                    break;
                    
                case ENEMYSTATE.DYING:
                    UpdateDying(gametime);
                    break;
                    
                case ENEMYSTATE.DEAD:
                    // Do nothing, waiting for cleanup
                    break;
            }
        }

        /// <summary>
        /// AI State Machine - determines behavior based on current state
        /// </summary>
        protected virtual void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {
            // Cooldown timer
            if (_attackCooldown > 0)
                _attackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Validate target is still valid
            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                CurrentAIState = AISTATE.Wandering;
            }

            switch (CurrentAIState)
            {
                case AISTATE.Charging:
                    PerformCharge(obstacles);
                    break;
                    
                case AISTATE.Chasing:
                    PerformChase(obstacles);
                    break;
                    
                case AISTATE.InCombat:
                    PerformCombat(gameTime);
                    break;
                    
                case AISTATE.Wandering:
                    PerformWander(obstacles);
                    break;
            }
        }

        // ===================
        // AI BEHAVIORS
        // ===================
        
        /// <summary>
        /// Charging: Move in initial direction (usually toward enemy lines)
        /// Override in subclasses for different charge behavior
        /// </summary>
        protected virtual void PerformCharge(List<WorldObject> obstacles)
        {
            // Default: charge left (toward player side)
            position.X -= Velocity;
            Vector2 target = new Vector2(position.X - 1000, position.Y);
            MoveToward(target, Velocity, obstacles);
        }

        /// <summary>
        /// Chasing: Pursue the current target
        /// </summary>
        /// 

        public void ApplyKnockback(Vector2 force) 
        {
            _knockBackVelocity += force;
        }
        protected virtual void PerformChase(List<WorldObject> obstacle)
        {
            if (_currentTarget == null)
            {
                CurrentAIState = AISTATE.Wandering;
                return;
            }

            MoveToward(_currentTarget.Center, Velocity, obstacle);

            // Check if close enough to engage
            float distance = CombatSystem.GetDistance(this, _currentTarget);
            if (distance < GameConstants.CombatEngageRange)
            {
                CurrentAIState = AISTATE.InCombat;
            }
        }

        /// <summary>
        /// In Combat: Attack the target, maintain position
        /// </summary>
        protected virtual void PerformCombat(GameTime gameTime)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                CurrentAIState = AISTATE.Wandering;
                return;
            }

            // Face the target
            SnapToFace(_currentTarget.Center);

            float distance = CombatSystem.GetDistance(this, _currentTarget);

            // Attack if in range and cooldown ready
            if (distance < GameConstants.MeleeAttackRange && _attackCooldown <= 0)
            {
                PerformAttack();
            }

            // Break combat if target moves too far
            if (distance > GameConstants.CombatBreakRange)
            {
                CurrentAIState = AISTATE.Chasing;
            }
        }

        /// <summary>
        /// Wandering: Move randomly near start position
        /// </summary>
        protected virtual void PerformWander(List<WorldObject> obstacles)
        {
            // Pick new target if needed
            if (wanderTarget == Vector2.Zero || 
                Vector2.Distance(position, wanderTarget) < 5f)
            {
                wanderTarget = startPosition + new Vector2(
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1),
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1)
                );
            }

            MoveToward(wanderTarget, Velocity * 0.5f, obstacles); // Wander slower
        }

        /// <summary>
        /// Dying: Play death animation, then become dead
        /// </summary>
        protected virtual void UpdateDying(GameTime gameTime)
        {
            _deathCountdown--;
            
            // Could add death animation here
            // Fade out, play particles, etc.
            
            if (_deathCountdown <= 0)
            {
                _lifecycleState = ENEMYSTATE.DEAD;
                Visible = false;
            }
        }

        // ===================
        // ICOMBATANT METHODS
        // ===================
        
        public virtual void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;
            
            Health -= amount;
            
            // Visual feedback - could flash red, play sound, etc.
            
            if (Health <= 0)
            {
                Die();
            }
        }

        public virtual void PerformAttack()
        {
            if (_currentTarget == null || _attackCooldown > 0) return;
            
            CombatSystem.DealDamage(this, _currentTarget, AttackDamage);
            _attackCooldown = GameConstants.DefaultAttackCooldown;
        }

        public virtual void Die()
        {
            _lifecycleState = ENEMYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            
            // Clear our target
            CombatSystem.ClearTarget(this);
        }

        // ===================
        // DRAW
        // ===================
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            if (_drawHealthBar && IsAlive)
            {
                DrawHealthBar(spriteBatch);
            }
        }

        protected virtual void DrawHealthBar(SpriteBatch spriteBatch)
        {
            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - spriteHeight / 2 - 10;

            // Background (red)
            spriteBatch.Draw(_healthBarTexture, 
                new Rectangle(barX, barY, barWidth, barHeight), 
                Color.Red);

            // Foreground (green) - proportional to health
            float healthPercent = (float)Health / MaxHealth;
            int currentBarWidth = (int)(barWidth * healthPercent);
            
            Color healthColor = healthPercent > 0.6f ? Color.Green :
                               healthPercent > 0.3f ? Color.Orange : Color.Red;
                               
            spriteBatch.Draw(_healthBarTexture, 
                new Rectangle(barX, barY, currentBarWidth, barHeight), 
                healthColor);
        }
    }
}
