using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    /// <summary>
    /// Ally unit that fights alongside the player.
    /// Uses the same state machine as Enemy but on the Player team.
    /// 
    /// This class replaces the hacky "List of Sprite with AI logic in Engine" approach.
    /// Now allies manage their own behavior just like enemies do.
    /// </summary>
    public class Ally : RotatingSprite, ICombatant
    {
        // ===================
        // ENUMS
        // ===================
        
        public enum ALLYSTATE { ALIVE, DYING, DEAD }

        // ===================
        // STATE
        // ===================
        
        private ALLYSTATE _lifecycleState = ALLYSTATE.ALIVE;
        public ALLYSTATE LifecycleState 
        { 
            get => _lifecycleState; 
            set => _lifecycleState = value; 
        }

        public Enemy.AISTATE CurrentAIState { get; set; } = Enemy.AISTATE.Charging;

        // ===================
        // ICOMBATANT IMPLEMENTATION
        // ===================
        
        public string Name { get; set; } = "Ally";
        public CombatTeam Team => CombatTeam.Player;
        
        private int _health;
        public int Health 
        { 
            get => _health; 
            set => _health = Math.Max(0, value); 
        }
        
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => _health > 0 && _lifecycleState == ALLYSTATE.ALIVE;
        public bool IsActive => Visible && _lifecycleState != ALLYSTATE.DEAD;
        
        private ICombatant _currentTarget;
        public ICombatant CurrentTarget 
        { 
            get => _currentTarget;
            set => _currentTarget = value;
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

        // ===================
        // CONSTRUCTOR
        // ===================
        
        public Ally(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            startPosition = userPosition;
            Velocity = GameConstants.DefaultAllySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            _health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            CurrentAIState = Enemy.AISTATE.Charging;
            
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
        
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);

            switch (_lifecycleState)
            {
                case ALLYSTATE.ALIVE:
                    UpdateAI(gametime);
                    break;
                    
                case ALLYSTATE.DYING:
                    UpdateDying(gametime);
                    break;
                    
                case ALLYSTATE.DEAD:
                    // Waiting for cleanup
                    break;
            }
        }

        /// <summary>
        /// AI State Machine - mirrors Enemy but charges opposite direction
        /// </summary>
        protected virtual void UpdateAI(GameTime gameTime)
        {
            // Cooldown timer
            if (_attackCooldown > 0)
                _attackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Validate target
            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                CurrentAIState = Enemy.AISTATE.Wandering;
            }

            switch (CurrentAIState)
            {
                case Enemy.AISTATE.Charging:
                    PerformCharge();
                    break;
                    
                case Enemy.AISTATE.Chasing:
                    PerformChase();
                    break;
                    
                case Enemy.AISTATE.InCombat:
                    PerformCombat(gameTime);
                    break;
                    
                case Enemy.AISTATE.Wandering:
                    PerformWander();
                    break;
            }
        }

        // ===================
        // AI BEHAVIORS
        // ===================
        
        /// <summary>
        /// Allies charge RIGHT (toward enemy lines)
        /// </summary>
        protected virtual void PerformCharge()
        {
            position.X += Velocity;
        }

        protected virtual void PerformChase()
        {
            if (_currentTarget == null)
            {
                CurrentAIState = Enemy.AISTATE.Wandering;
                return;
            }

            MoveToward(_currentTarget.Center, Velocity);

            float distance = CombatSystem.GetDistance(this, _currentTarget);
            if (distance < GameConstants.CombatEngageRange)
            {
                CurrentAIState = Enemy.AISTATE.InCombat;
            }
        }

        protected virtual void PerformCombat(GameTime gameTime)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                CurrentAIState = Enemy.AISTATE.Wandering;
                return;
            }

            SnapToFace(_currentTarget.Center);

            float distance = CombatSystem.GetDistance(this, _currentTarget);

            if (distance < GameConstants.MeleeAttackRange && _attackCooldown <= 0)
            {
                PerformAttack();
            }

            if (distance > GameConstants.CombatBreakRange)
            {
                CurrentAIState = Enemy.AISTATE.Chasing;
            }
        }

        protected virtual void PerformWander()
        {
            if (wanderTarget == Vector2.Zero || 
                Vector2.Distance(position, wanderTarget) < 5f)
            {
                wanderTarget = startPosition + new Vector2(
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1),
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1)
                );
            }

            MoveToward(wanderTarget, Velocity * 0.5f);
        }

        protected virtual void UpdateDying(GameTime gameTime)
        {
            _deathCountdown--;
            
            if (_deathCountdown <= 0)
            {
                _lifecycleState = ALLYSTATE.DEAD;
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
            _lifecycleState = ALLYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            CombatSystem.ClearTarget(this);
        }

        // ===================
        // DRAW
        // ===================
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            if (IsAlive)
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

            // Background
            spriteBatch.Draw(_healthBarTexture, 
                new Rectangle(barX, barY, barWidth, barHeight), 
                Color.Red);

            // Health (blue for allies to distinguish from enemies)
            float healthPercent = (float)Health / MaxHealth;
            int currentBarWidth = (int)(barWidth * healthPercent);
            
            spriteBatch.Draw(_healthBarTexture, 
                new Rectangle(barX, barY, currentBarWidth, barHeight), 
                Color.CornflowerBlue);
        }
    }
}
