using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Enemy : RotatingSprite, ICombatant
    {
        public enum ENEMYSTATE { ALIVE, DYING, DEAD }
        public enum AISTATE { Charging, Chasing, InCombat, Wandering }

        private AnimationManager _animManager;
        private int _currentDirectionIndex = 0;
        private SpriteEffects _flipEffect = SpriteEffects.None;
        private Vector2 _knockBackVelocity;
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        private ENEMYSTATE _lifecycleState = ENEMYSTATE.ALIVE;
        public ENEMYSTATE LifecycleState
        {
            get => _lifecycleState;
            set => _lifecycleState = value;
        }

        public string Name { get; set; } = "Enemy";
        public CombatTeam Team => CombatTeam.Enemy;

        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => Health > 0 && _lifecycleState == ENEMYSTATE.ALIVE;
        public bool IsActive => Visible && _lifecycleState != ENEMYSTATE.DEAD;

        private ICombatant _currentTarget;
        public ICombatant CurrentTarget
        {
            get => _currentTarget;
            set
            {
                _currentTarget = value;
                CurrentCombatPartner = value as Sprite;
            }
        }

        public Vector2 Position => position;
        protected float Velocity;
        protected Vector2 startPosition;
        protected Vector2 wanderTarget;
        private float _attackCooldown = 0f;
        private int _deathCountdown;

        // ==================================================================================
        // CONSTRUCTOR 1: NEW (For Animated Orcs)
        // ==================================================================================
        public Enemy(Game g, Dictionary<string, Texture2D> textures, Vector2 userPosition, int framecount)
            : base(g, textures["Idle"], userPosition, framecount)
        {
            SetupCommonStats(userPosition);

            // SCALE: 3.0f is a balance. 
            // 4.0f makes the hitbox too big for 64px tiles. 
            // 2.0f might be too small visually.
            Scale = 3.0f;

            _animManager = new AnimationManager();

            // ORC SHEET CONFIGURATION
            // isGrid = true (It has 4 rows)
            // totalRows = 4
            // Speed = 200f (Slower, so you can see the animation)
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 200f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 4, 0, 150f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 4, 0, 200f, false, 4, 0, true));
            _animManager.AddAnimation("Hurt", new Animation(textures["Hurt"], 4, 0, 150f, false, 4, 0, true));
            _animManager.AddAnimation("Death", new Animation(textures["Death"], 4, 0, 200f, false, 4, 0, true));

            _animManager.Play("Idle");
            SetupHealthBar(g);
        }

        // ==================================================================================
        // CONSTRUCTOR 2: LEGACY (For Sentry, etc.)
        // ==================================================================================
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

        private void SetupCommonStats(Vector2 pos)
        {
            startPosition = pos;
            Velocity = GameConstants.DefaultEnemySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            CurrentAIState = AISTATE.Charging;
        }

        private void SetupHealthBar(Game g)
        {
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        public virtual void Update(GameTime gametime)
        {
            if (_knockBackVelocity != Vector2.Zero)
            {
                position += _knockBackVelocity;
                _knockBackVelocity *= GameConstants.KnockbackFriction;
                if (_knockBackVelocity.Length() < 0.1f) _knockBackVelocity = Vector2.Zero;
            }

            base.Update(gametime);
            UpdateDirection();

            string animKey = "Idle";

            if (_lifecycleState == ENEMYSTATE.DYING)
            {
                animKey = "Death";
            }
            else if (CurrentAIState == AISTATE.InCombat && _attackCooldown > 800)
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
        }

        public void Update(GameTime gametime, List<WorldObject> obstacles)
        {
            this.Update(gametime);
            if (_lifecycleState == ENEMYSTATE.ALIVE)
            {
                UpdateAI(gametime, obstacles);
            }
        }

        protected virtual void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {
            if (_attackCooldown > 0)
                _attackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                CurrentAIState = AISTATE.Wandering;
            }

            switch (CurrentAIState)
            {
                case AISTATE.Charging: PerformCharge(obstacles); break;
                case AISTATE.Chasing: PerformChase(obstacles); break;
                case AISTATE.InCombat: PerformCombat(gameTime); break;
                case AISTATE.Wandering: PerformWander(obstacles); break;
            }
        }

        protected virtual void PerformCharge(List<WorldObject> obstacles)
        {
            position.X -= Velocity;
            Vector2 target = new Vector2(position.X - 1000, position.Y);
            MoveToward(target, Velocity, obstacles);
        }

        public void ApplyKnockback(Vector2 force)
        {
            _knockBackVelocity += force;
        }

        protected virtual void PerformChase(List<WorldObject> obstacle)
        {
            if (_currentTarget == null) { CurrentAIState = AISTATE.Wandering; return; }
            MoveToward(_currentTarget.Center, Velocity, obstacle);
            if (CombatSystem.GetDistance(this, _currentTarget) < GameConstants.CombatEngageRange)
            {
                CurrentAIState = AISTATE.InCombat;
            }
        }

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

        protected virtual void UpdateDying(GameTime gameTime)
        {
            _deathCountdown--;
            if (_deathCountdown <= 0)
            {
                _lifecycleState = ENEMYSTATE.DEAD;
                Visible = false;
            }
        }

        public virtual void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;
            Health -= amount;
            if (Health <= 0) Die();
        }

        public virtual void PerformAttack()
        {
            if (_currentTarget == null || _attackCooldown > 0) return;
            _animManager.Play("Attack");
            CombatSystem.DealDamage(this, _currentTarget, AttackDamage);
            _attackCooldown = GameConstants.DefaultAttackCooldown;
        }

        public virtual void Die()
        {
            _lifecycleState = ENEMYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            CombatSystem.ClearTarget(this);
        }

        private void UpdateDirection()
        {
            // ORC SHEET MAPPING (Based on visual inspection of orc1_run_full.png)
            // Row 0: Down
            // Row 1: Up
            // Row 2: Left
            // Row 3: Right

            if (CurrentTarget != null)
            {
                Vector2 diff = CurrentTarget.Position - this.Position;
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                {
                    // Horizontal
                    _currentDirectionIndex = (diff.X > 0) ? 3 : 2; // 3=Right, 2=Left
                }
                else
                {
                    // Vertical
                    _currentDirectionIndex = (diff.Y > 0) ? 0 : 1; // 0=Down, 1=Up
                }
            }
            else if (Velocity > 0)
            {
                // Charging Left (towards player)
                _currentDirectionIndex = 2;
            }
        }

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