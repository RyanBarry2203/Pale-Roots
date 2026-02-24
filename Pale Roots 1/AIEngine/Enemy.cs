using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Enemy : RotatingSprite, INpcActor
    {

        public enum ENEMYSTATE { ALIVE, DYING, DEAD }
        public bool IsStunned { get; set; }

        protected AnimationManager _animManager;
        private int _currentDirectionIndex = 2;
        protected SpriteEffects _flipEffect = SpriteEffects.None;
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
                CurrentCombatPartner = value as Sprite;
            }
        }

        public Vector2 Position => position;

        // Exposed variables so the State Objects can manipulate them
        public float Velocity { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 WanderTarget { get; set; }
        public float AttackCooldown { get; set; }
        private int _deathCountdown;

        // --- THE ENGINE FLEX: POLYMORPHIC STATE MACHINE ---
        public IAIState CurrentState { get; private set; }

        public void ChangeState(IAIState newState)
        {
            CurrentState?.Exit(this);
            CurrentState = newState;
            CurrentState?.Enter(this);
        }

        public void PlayAnimation(string key)
        {
            _animManager.Play(key);
        }

        public Enemy(Game g, Dictionary<string, Texture2D> textures, Vector2 userPosition, int framecount)
            : base(g, textures["Idle"], userPosition, framecount)
        {
            SetupCommonStats(userPosition);
            Scale = 3.0;
            _animManager = new AnimationManager();
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 150f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 8, 0, 150f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 8, 0, 125f, false, 4, 0, true));
            _animManager.AddAnimation("Hurt", new Animation(textures["Hurt"], 6, 0, 150f, false, 4, 0, true));
            _animManager.AddAnimation("Death", new Animation(textures["Death"], 8, 0, 150f, false, 4, 0, true));
            _animManager.Play("Idle");
            SetupHealthBar(g);
        }

        public Enemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            SetupCommonStats(userPosition);
            Scale = 3.0;
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
            StartPosition = pos;
            _previousPosition = pos;
            Velocity = GameConstants.DefaultEnemySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;

            // Start the brain in the Charge State!
            ChangeState(new ChargeState());
        }

        private void SetupHealthBar(Game g)
        {
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        public override void Update(GameTime gametime)
        {
            if (_knockBackVelocity != Vector2.Zero)
            {
                position += _knockBackVelocity;
                _knockBackVelocity *= GameConstants.KnockbackFriction;
                if (_knockBackVelocity.Length() < 0.1f) _knockBackVelocity = Vector2.Zero;
            }

            base.Update(gametime);
            UpdateDirection();

            // Check what class the current state is to drive the animation
            string animKey = "Idle";
            if (_lifecycleState == ENEMYSTATE.DYING) animKey = "Death";
            else if (CurrentState is HurtState) animKey = "Hurt";
            else if (CurrentState is CombatState && AttackCooldown > 200) animKey = "Attack";
            else if (Velocity > 0.1f || CurrentState is ChaseState || CurrentState is ChargeState) animKey = "Walk";

            _animManager.Play(animKey);
            _animManager.Update(gametime);

            if (_lifecycleState == ENEMYSTATE.DYING) UpdateDying(gametime);

            _previousPosition = position;
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
            if (IsStunned) return;

            if (AttackCooldown > 0)
                AttackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                if (!(CurrentState is WanderState)) ChangeState(new WanderState());
            }

            CurrentState?.Update(this, gameTime, obstacles);
        }

        public virtual void ApplyKnockback(Vector2 force)
        {
            _knockBackVelocity += force;
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
            else ChangeState(new HurtState());
        }

        public virtual void PerformAttack()
        {
            if (_currentTarget == null || AttackCooldown > 0) return;
            _animManager.Play("Attack");
            CombatSystem.DealDamage(this, _currentTarget, AttackDamage);
            AttackCooldown = GameConstants.DefaultAttackCooldown;
        }

        public virtual void Die()
        {
            _lifecycleState = ENEMYSTATE.DYING;
            _deathCountdown = 80;
            CombatSystem.ClearTarget(this);
        }

        private void UpdateDirection()
        {
            if (CurrentTarget != null)
            {
                Vector2 diff = CurrentTarget.Position - this.Position;
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    _currentDirectionIndex = (diff.X > 0) ? 3 : 2;
                else
                    _currentDirectionIndex = (diff.Y > 0) ? 0 : 1;
            }
            else
            {
                Vector2 movement = position - _previousPosition;
                if (movement.Length() > 0.5f)
                {
                    if (Math.Abs(movement.X) > Math.Abs(movement.Y))
                        _currentDirectionIndex = (movement.X > 0) ? 3 : 2;
                    else
                        _currentDirectionIndex = (movement.Y > 0) ? 0 : 1;
                }
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