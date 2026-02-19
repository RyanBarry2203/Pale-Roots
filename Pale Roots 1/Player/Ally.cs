using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Ally: friendly combat unit that behaves like Enemy but fights for the player.
    // - Implements ICombatant so CombatSystem and ChaseAndFireEngine treat it like any actor.
    // - Uses RotatingSprite for movement/rotation helpers and AnimationManager for visuals.
    public class Ally : RotatingSprite, ICombatant
    {
        // Simple lifecycle enum for ally state
        public enum ALLYSTATE { ALIVE, DYING, DEAD }

        // Animation + facing state
        private AnimationManager _animManager;
        private int _currentDirectionIndex = 1;
        private SpriteEffects _flipEffect = SpriteEffects.None;

        // Shared 1x1 texture used to draw the ally's health bar
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        // Lifecycle backing field and property
        private ALLYSTATE _lifecycleState = ALLYSTATE.ALIVE;
        public ALLYSTATE LifecycleState
        {
            get => _lifecycleState;
            set => _lifecycleState = value;
        }

        // ICombatant surface (used by CombatSystem and engine)
        public string Name { get; set; } = "Ally";
        public CombatTeam Team => CombatTeam.Player;
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => Health > 0 && _lifecycleState == ALLYSTATE.ALIVE;
        public bool IsActive => Visible && _lifecycleState != ALLYSTATE.DEAD;

        // Target bookkeeping (keeps a Sprite reference for visual sync)
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

        // Position/movement fields used by RotatingSprite and AI
        public Vector2 Position => position;
        protected float Velocity;
        protected Vector2 startPosition;
        protected Vector2 wanderTarget;
        private float _attackCooldown = 0f;
        private int _deathCountdown;

        // Constructor: sets stats, animations and initial AI state (mirrors Enemy pattern)
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

            // Register animations using the supplied atlas dictionary
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 200f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 4, 0, 125f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 6, 0, 175f, false, 4, 0, true));

            _animManager.Play("Idle");

            // Create the health-bar texture once for all allies
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        // High-level update: run base update, choose animation and handle dying
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

        // Frame update that includes obstacle-aware AI behavior (called by engine)
        public void Update(GameTime gametime, List<WorldObject> obstacles)
        {
            this.Update(gametime);
            if (_lifecycleState == ALLYSTATE.ALIVE)
            {
                UpdateAI(gametime, obstacles);
            }
        }

        // AI tick: handle cooldowns, validate targets, and dispatch to state handlers
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

        // Charge movement: ally moves right (game convention differs from Enemy)
        protected virtual void PerformCharge(List<WorldObject> obstacles)
        {
            position.X += Velocity;
            Vector2 target = new Vector2(position.X - 1000, position.Y);
            MoveToward(target, Velocity, obstacles);
        }

        // Chase current target using MoveToward (handles obstacle sliding). Switch to InCombat when close.
        protected virtual void PerformChase(List<WorldObject> obstacles)
        {
            if (_currentTarget == null) { CurrentAIState = Enemy.AISTATE.Wandering; return; }
            MoveToward(_currentTarget.Center, Velocity, obstacles);
            if (CombatSystem.GetDistance(this, _currentTarget) < GameConstants.CombatEngageRange)
                CurrentAIState = Enemy.AISTATE.InCombat;
        }

        // Combat loop: face target, attack if in range and off-cooldown, break if target flees.
        protected virtual void PerformCombat(GameTime gameTime)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive) { CurrentAIState = Enemy.AISTATE.Wandering; return; }
            SnapToFace(_currentTarget.Center);
            if (CombatSystem.GetDistance(this, _currentTarget) < GameConstants.MeleeAttackRange && _attackCooldown <= 0)
                PerformAttack();
            if (CombatSystem.GetDistance(this, _currentTarget) > GameConstants.CombatBreakRange)
                CurrentAIState = Enemy.AISTATE.Chasing;
        }

        // Wander: pick a random point around the start and MoveToward it slowly
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

        // Dying countdown then mark dead and hide
        protected virtual void UpdateDying(GameTime gameTime)
        {
            _deathCountdown--;
            if (_deathCountdown <= 0) { _lifecycleState = ALLYSTATE.DEAD; Visible = false; }
        }

        // Apply damage; transition to dying/dead when health depletes
        public virtual void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;
            Health -= amount;
            if (Health <= 0) Die();
        }

        // Trigger melee attack animation and call CombatSystem to resolve damage
        public virtual void PerformAttack()
        {
            if (_currentTarget == null || _attackCooldown > 0) return;
            _animManager.Play("Attack");
            CombatSystem.DealDamage(this, _currentTarget, AttackDamage);
            _attackCooldown = GameConstants.DefaultAttackCooldown;
        }

        // Enter dying state and clear target bookkeeping
        public virtual void Die()
        {
            _lifecycleState = ALLYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            CombatSystem.ClearTarget(this);
        }

        // Choose facing direction for animation based on current target or recent movement
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

        // Draw animation and a small health bar above the ally
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
                _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect, _currentDirectionIndex);
            if (_drawHealthBar && IsAlive)
                DrawHealthBar(spriteBatch);
        }

        // Health bar rendering: red background, colored foreground based on health percent
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