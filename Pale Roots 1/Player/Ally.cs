using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Ally : RotatingSprite, ICombatant
    {
        public enum ALLYSTATE { ALIVE, DYING, DEAD }

        private AnimationManager _animManager;
        private int _currentDirectionIndex = 0;
        private SpriteEffects _flipEffect = SpriteEffects.None;
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        private ALLYSTATE _lifecycleState = ALLYSTATE.ALIVE;
        public ALLYSTATE LifecycleState
        {
            get => _lifecycleState;
            set => _lifecycleState = value;
        }

        public string Name { get; set; } = "Ally";
        public CombatTeam Team => CombatTeam.Player;

        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => Health > 0 && _lifecycleState == ALLYSTATE.ALIVE;
        public bool IsActive => Visible && _lifecycleState != ALLYSTATE.DEAD;

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

        public Ally(Game g, Dictionary<string, Texture2D> textures, Vector2 userPosition, int framecount)
            : base(g, textures["Walk"], userPosition, framecount)
        {
            startPosition = userPosition;
            Velocity = GameConstants.DefaultAllySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            CurrentAIState = Enemy.AISTATE.Charging;

            // FIX: Reduced Scale so they don't get stuck on each other
            Scale = 2.5f;

            _animManager = new AnimationManager();

            // FIX: Ensure totalRows = 1 for Strips
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 4, 0, 120f, true, 1, 0, false));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 4, 0, 100f, false, 1, 0, false));

            _animManager.Play("Walk");

            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            UpdateDirection();

            string animKey = "Idle";
            // Simple state check for animation
            if (CurrentAIState == Enemy.AISTATE.InCombat && _attackCooldown > 800)
                animKey = "Attack";
            else if (Velocity > 0.1f || CurrentAIState == Enemy.AISTATE.Charging)
                animKey = "Walk";

            _animManager.Play(animKey);
            _animManager.Update(gametime);

            if (_lifecycleState == ALLYSTATE.DYING) UpdateDying(gametime);
        }

        public void Update(GameTime gametime, List<WorldObject> obstacles)
        {
            this.Update(gametime);
            if (_lifecycleState == ALLYSTATE.ALIVE) UpdateAI(gametime, obstacles);
        }

        protected virtual void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {
            if (_attackCooldown > 0) _attackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                CurrentAIState = Enemy.AISTATE.Wandering;
            }

            switch (CurrentAIState)
            {
                case Enemy.AISTATE.Charging: PerformCharge(obstacles); break;
                case Enemy.AISTATE.Chasing: PerformChase(obstacles); break;
                case Enemy.AISTATE.InCombat: PerformCombat(gameTime); break;
                case Enemy.AISTATE.Wandering: PerformWander(obstacles); break;
            }
        }

        protected virtual void PerformCharge(List<WorldObject> obstacles)
        {
            position.X += Velocity;
            Vector2 target = new Vector2(position.X - 1000, position.Y);
            MoveToward(target, Velocity, obstacles);
        }

        protected virtual void PerformChase(List<WorldObject> obstacles)
        {
            if (_currentTarget == null) { CurrentAIState = Enemy.AISTATE.Wandering; return; }
            MoveToward(_currentTarget.Center, Velocity, obstacles);
            if (CombatSystem.GetDistance(this, _currentTarget) < GameConstants.CombatEngageRange)
                CurrentAIState = Enemy.AISTATE.InCombat;
        }

        protected virtual void PerformCombat(GameTime gameTime)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive) { CurrentAIState = Enemy.AISTATE.Wandering; return; }
            SnapToFace(_currentTarget.Center);
            if (CombatSystem.GetDistance(this, _currentTarget) < GameConstants.MeleeAttackRange && _attackCooldown <= 0)
                PerformAttack();
            if (CombatSystem.GetDistance(this, _currentTarget) > GameConstants.CombatBreakRange)
                CurrentAIState = Enemy.AISTATE.Chasing;
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
            if (_deathCountdown <= 0) { _lifecycleState = ALLYSTATE.DEAD; Visible = false; }
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
            _lifecycleState = ALLYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            CombatSystem.ClearTarget(this);
        }

        private void UpdateDirection()
        {
            // FIX: ALWAYS USE ROW 0. 
            // This prevents the "facing wrong direction" bug because strips don't have rows 1, 2, or 3.
            _currentDirectionIndex = 0;
            _flipEffect = SpriteEffects.None;

            // Only flip if moving Left
            if (CurrentTarget != null)
            {
                if (CurrentTarget.Position.X < this.Position.X)
                    _flipEffect = SpriteEffects.FlipHorizontally;
            }
            else
            {
                // Default charging Right (No flip)
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
                _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect, _currentDirectionIndex);
            if (_drawHealthBar && IsAlive)
                DrawHealthBar(spriteBatch);
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
            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.CornflowerBlue);
        }
    }
}