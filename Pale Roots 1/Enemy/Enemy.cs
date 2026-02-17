using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Enemy: a game actor that can navigate, target, fight and die.
    // Responsibilities:
    // - Holds combat stats and lifecycle state.
    // - Manages animation via AnimationManager.
    // - Contains AI states and transitions for Charging, Chasing, InCombat, Wandering and Hurt.
    // Interactions:
    // - Uses CombatSystem for damage, target validation and bookkeeping.
    // - Uses RotatingSprite.MoveToward and SnapToFace for movement/rotation.
    // - Consults WorldObject obstacles when pathing.
    // - Exposes ICombatant-compatible members so ChaseAndFireEngine and CombatSystem can operate on it.
    public class Enemy : RotatingSprite, ICombatant
    {
        public enum ENEMYSTATE { ALIVE, DYING, DEAD }
        public enum AISTATE { Charging, Chasing, InCombat, Wandering, Hurt }

        public bool IsStunned { get; set; }

        private AnimationManager _animManager;
        private int _currentDirectionIndex = 2;
        private SpriteEffects _flipEffect = SpriteEffects.None;
        private Vector2 _knockBackVelocity;
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;
        private Vector2 _previousPosition;

        private ENEMYSTATE _lifecycleState = ENEMYSTATE.ALIVE;
        public ENEMYSTATE LifecycleState
        {
            get => _lifecycleState;
            set => _lifecycleState = value;
        }

        // ICombatant surface
        public string Name { get; set; } = "Enemy";
        public CombatTeam Team => CombatTeam.Enemy;
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; set; }
        public bool IsAlive => Health > 0 && _lifecycleState == ENEMYSTATE.ALIVE;
        public bool IsActive => Visible && _lifecycleState != ENEMYSTATE.DEAD;

        private ICombatant _currentTarget;
        public ICombatant CurrentTarget
        {
            get => _currentTarget;
            set
            {
                _currentTarget = value;
                CurrentCombatPartner = value as Sprite; // keep Sprite reference for visual syncing if needed
            }
        }

        // Position and movement
        public Vector2 Position => position;
        protected float Velocity;
        protected Vector2 startPosition;
        protected Vector2 wanderTarget;
        private float _attackCooldown = 0f;
        private int _deathCountdown;

        // Constructor for modern animated enemies that supply a map of textures
        public Enemy(Game g, Dictionary<string, Texture2D> textures, Vector2 userPosition, int framecount)
            : base(g, textures["Idle"], userPosition, framecount)
        {
            SetupCommonStats(userPosition);

            Scale = 3.0f; // visual scale chosen to match tile sizing

            _animManager = new AnimationManager();

            // Register directional animations (grid sheets assumed)
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 150f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 8, 0, 150f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 8, 0, 125f, false, 4, 0, true));
            _animManager.AddAnimation("Hurt", new Animation(textures["Hurt"], 6, 0, 150f, false, 4, 0, true));
            _animManager.AddAnimation("Death", new Animation(textures["Death"], 8, 0, 150f, false, 4, 0, true));

            _animManager.Play("Idle");
            SetupHealthBar(g);
        }

        // Backwards-compatible constructor for legacy single-sheet enemies
        public Enemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            SetupCommonStats(userPosition);
            Scale = 3.0f;

            _animManager = new AnimationManager();
            var legacyAnim = new Animation(texture, framecount, 0, 200f, true, 1, 0, false);

            _animManager.AddAnimation("Idle", legacyAnim);
            _animManager.AddAnimation("Walk", legacyAnim);
            _animManager.AddAnimation("Attack", legacyAnim);
            _animManager.AddAnimation("Hurt", legacyAnim);
            _animManager.AddAnimation("Death", legacyAnim);

            _animManager.Play("Idle");
            SetupHealthBar(g);
        }

        // Shared initialization for both constructors
        private void SetupCommonStats(Vector2 pos)
        {
            startPosition = pos;
            _previousPosition = pos;
            Velocity = GameConstants.DefaultEnemySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            CurrentAIState = AISTATE.Charging;
        }

        // Create a 1x1 white texture once for health bars
        private void SetupHealthBar(Game g)
        {
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        // High-level update: physics, animations, and dying transitions
        public virtual void Update(GameTime gametime)
        {
            // Apply any residual knockback and damp it
            if (_knockBackVelocity != Vector2.Zero)
            {
                position += _knockBackVelocity;
                _knockBackVelocity *= GameConstants.KnockbackFriction;
                if (_knockBackVelocity.Length() < 0.1f) _knockBackVelocity = Vector2.Zero;
            }

            base.Update(gametime);
            UpdateDirection();

            // Choose animation based on lifecycle and AI state
            string animKey = "Idle";

            if (_lifecycleState == ENEMYSTATE.DYING)
            {
                animKey = "Death";
            }
            else if (CurrentAIState == AISTATE.Hurt)
            {
                animKey = "Hurt";
            }
            else if (CurrentAIState == AISTATE.InCombat && _attackCooldown > 200)
            {
                animKey = "Attack";
            }
            else if (Velocity > 0.1f || CurrentAIState == AISTATE.Chasing || CurrentAIState == AISTATE.Charging)
            {
                animKey = "Walk";
            }

            _animManager.Play(animKey);
            _animManager.Update(gametime);

            if (_lifecycleState == ENEMYSTATE.DYING) UpdateDying(gametime);

            _previousPosition = position;
        }

        // Update that includes obstacle-aware AI behavior
        public void Update(GameTime gametime, List<WorldObject> obstacles)
        {
            this.Update(gametime);
            if (_lifecycleState == ENEMYSTATE.ALIVE)
            {
                UpdateAI(gametime, obstacles);
            }
        }

        // Core AI tick: cooldowns, target validation, and state dispatch
        protected virtual void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {

            if (IsStunned)
            {
                return;
            }
            if (_attackCooldown > 0)
                _attackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                CurrentAIState = AISTATE.Wandering;
            }
            if (CurrentAIState == AISTATE.Hurt)
            {
                if (_attackCooldown <= 0)
                {
                    CurrentAIState = AISTATE.Chasing;
                }
            }

            switch (CurrentAIState)
            {
                case AISTATE.Charging: PerformCharge(obstacles); break;
                case AISTATE.Chasing: PerformChase(obstacles); break;
                case AISTATE.InCombat: PerformCombat(gameTime); break;
                case AISTATE.Wandering: PerformWander(obstacles); break;
            }
        }

        // Default charge behavior: move left and path toward a distant point
        protected virtual void PerformCharge(List<WorldObject> obstacles)
        {
            position.X -= Velocity;
            Vector2 target = new Vector2(position.X - 1000, position.Y);
            MoveToward(target, Velocity, obstacles);
        }

        // External call to apply knockback impulse from player or effects
        public void ApplyKnockback(Vector2 force)
        {
            _knockBackVelocity += force;
        }

        // Move to the current target; switch to InCombat when close
        protected virtual void PerformChase(List<WorldObject> obstacle)
        {
            if (_currentTarget == null) { CurrentAIState = AISTATE.Wandering; return; }
            MoveToward(_currentTarget.Center, Velocity, obstacle);
            if (CombatSystem.GetDistance(this, _currentTarget) < GameConstants.CombatEngageRange)
            {
                CurrentAIState = AISTATE.InCombat;
            }
        }

        // Combat loop: face target and perform melee if in range and off cooldown
        protected virtual void PerformCombat(GameTime gameTime)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive) { CurrentAIState = AISTATE.Wandering; return; }
            SnapToFace(_currentTarget.Center);
            if (CombatSystem.GetDistance(this, _currentTarget) < GameConstants.MeleeAttackRange && _attackCooldown <= 0)
            {
                PerformAttack();
            }
            if (CombatSystem.GetDistance(this, _currentTarget) > GameConstants.CombatBreakRange)
            {
                CurrentAIState = AISTATE.Chasing;
            }
        }

        // Wandering behavior: pick a random point around start and move toward it
        protected virtual void PerformWander(List<WorldObject> obstacles)
        {
            if (wanderTarget == Vector2.Zero || Vector2.Distance(position, wanderTarget) < 5f)
            {
                wanderTarget = startPosition + new Vector2(
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1),
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1)
                );
            }
            MoveToward(wanderTarget, Velocity * 0.5f, obstacles);
        }

        // When dying, countdown and then mark as dead and hide
        protected virtual void UpdateDying(GameTime gameTime)
        {
            _deathCountdown--;
            if (_deathCountdown <= 0)
            {
                _lifecycleState = ENEMYSTATE.DEAD;
                Visible = false;
            }
        }

        // Apply damage and trigger Hurt/Death transitions. Uses CombatSystem to propagate effects.
        public virtual void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;

            Health -= amount;

            if (Health <= 0)
            {
                Die();
            }
            else
            {
                CurrentAIState = AISTATE.Hurt;
                _attackCooldown = 500f; // brief stun
                _animManager.Play("Hurt");
            }
        }

        // Trigger attack animation and use central CombatSystem to resolve damage
        public virtual void PerformAttack()
        {
            if (_currentTarget == null || _attackCooldown > 0) return;
            _animManager.Play("Attack");
            CombatSystem.DealDamage(this, _currentTarget, AttackDamage);
            _attackCooldown = GameConstants.DefaultAttackCooldown;
        }

        // Mark entity as dying and clear its target bookkeeping
                    public virtual void Die()
        {
            _lifecycleState = ENEMYSTATE.DYING;
            _deathCountdown = 80;
            CombatSystem.ClearTarget(this);
        }

        // Determine animation row/direction based on target or velocity
        private void UpdateDirection()
        {
            // 1. If we have a target, face them (Combat/Chase)
            if (CurrentTarget != null)
            {
                Vector2 diff = CurrentTarget.Position - this.Position;
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    _currentDirectionIndex = (diff.X > 0) ? 3 : 2; // 3=Right, 2=Left
                else
                    _currentDirectionIndex = (diff.Y > 0) ? 0 : 1; // 0=Down, 1=Up
            }
            else
            {
                Vector2 movement = position - _previousPosition;

                // Only update if we moved enough to matter
                if (movement.Length() > 0.5f)
                {
                    if (Math.Abs(movement.X) > Math.Abs(movement.Y))
                        _currentDirectionIndex = (movement.X > 0) ? 3 : 2; // Right vs Left
                    else
                        _currentDirectionIndex = (movement.Y > 0) ? 0 : 1; // Down vs Up
                }
            }
        }

        // Draw animation then optional health bar; AnimationManager receives direction index to select row
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect, _currentDirectionIndex);
            }

            if (_drawHealthBar && IsAlive)
            {
                DrawHealthBar(spriteBatch);
            }
        }

        // Render a compact health bar above the enemy. Uses a shared 1x1 texture.
        protected virtual void DrawHealthBar(SpriteBatch spriteBatch)
        {
            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - spriteHeight / 2 - 10;

            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            float healthPercent = (float)Health / MaxHealth;
            int currentBarWidth = (int)(barWidth * healthPercent);

            Color healthColor = healthPercent > 0.6f ? Color.Green : healthPercent > 0.3f ? Color.Orange : Color.Red;

            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), healthColor);
        }
    }
}