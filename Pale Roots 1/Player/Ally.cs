using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Ally: a friendly combatant that mirrors Enemy behavior but fights for the player.
    // Responsibilities:
    // - Implements ICombatant so CombatSystem and ChaseAndFireEngine can treat it like any actor.
    // - Holds animations via AnimationManager and transitions states (wander, chase, combat).
    // - Movement and combat decision logic is intentionally similar to Enemy for reuse.
    // Interactions:
    // - Receives target assignments from ChaseAndFireEngine via CombatSystem.AssignTarget.
    // - Uses CombatSystem to deal and take damage; ClearTarget called when a target is invalid.
    // - Uses RotatingSprite movement/integration for obstacle-aware pathing.
    public class Ally : RotatingSprite, ICombatant
    {
        public enum ALLYSTATE { ALIVE, DYING, DEAD }

        private AnimationManager _animManager;
        private int _currentDirectionIndex = 1;
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

        // Ally constructor uses texture dictionary for animations, similar to Enemy
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

            Scale = 3.0f;
            _animManager = new AnimationManager();

            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 200f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 4, 0, 125f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 6, 0, 175f, false, 4, 0, true));

            _animManager.Play("Idle");

            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        // High-level update: animation selection and death handling
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            UpdateDirection();

            string animKey = "Idle";
            if (CurrentAIState == Enemy.AISTATE.InCombat && _attackCooldown > 800)
                animKey = "Attack";
            else if (Velocity > 0.1f || CurrentAIState == Enemy.AISTATE.Charging)
                animKey = "Walk";

            _animManager.Play(animKey);
            _animManager.Update(gametime);

            if (_lifecycleState == ALLYSTATE.DYING) UpdateDying(gametime);
        }

        // Update with obstacle list to enable pathing checks; uses shared AI loop pattern
        public void Update(GameTime gametime, List<WorldObject> obstacles)
        {
            this.Update(gametime);
            if (_lifecycleState == ALLYSTATE.ALIVE)
            {
                UpdateAI(gametime, obstacles);
            }
        }

        // AI tick shares structure with Enemy: cooldowns, validation, and state dispatch
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

        // Move forward while charging; different direction compared to Enemy
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

        // Simple damage handling; transitions to dead state when health depletes
        public virtual void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;
            Health -= amount;
            if (Health <= 0) Die();
        }

        // Trigger melee attack using CombatSystem to resolve damage and cooldown bookkeeping
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

        // Determine facing for animation; flipEffect is managed for visual mirroring
        private void UpdateDirection()
        {
            if (CurrentTarget != null)
            {
                Vector2 diff = CurrentTarget.Position - this.Position;

                _flipEffect = SpriteEffects.None;

                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                {
                    _currentDirectionIndex = (diff.X < 0) ? 0 : 1;
                }
                else
                {
                    _currentDirectionIndex = (diff.Y < 0) ? 2 : 3;
                }
            }
            else if (Velocity > 0.1f)
            {
                // keep current direction when moving
            }
        }

        // Draw uses the AnimationManager with the chosen direction and draws a small health bar above the ally
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