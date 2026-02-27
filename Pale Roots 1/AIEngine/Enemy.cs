using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // This is the core blueprint for all standard enemies in the game.
    // It inherits from RotatingSprite for rendering and implements INpcActor so our AI state machine can drive it.
    public class Enemy : RotatingSprite, INpcActor
    {
        // Tracks the broader lifecycle of the enemy so we know when to play death animations or remove them from the list.
        public enum ENEMYSTATE { ALIVE, DYING, DEAD }
        public bool IsStunned { get; set; }

        protected AnimationManager _animManager;
        private int _currentDirectionIndex = 2;
        protected SpriteEffects _flipEffect = SpriteEffects.None;

        // Used to smoothly push the enemy backward when they get hit.
        private Vector2 _knockBackVelocity;

        // We use a single shared 1x1 pixel texture to draw health bars for all enemies to save video memory.
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        // We track the previous position to figure out which way the enemy is moving when they don't have a target.
        private Vector2 _previousPosition;
        private ENEMYSTATE _lifecycleState = ENEMYSTATE.ALIVE;

        public ENEMYSTATE LifecycleState
        {
            get => _lifecycleState;
            set => _lifecycleState = value;
        }

        // Basic RPG stats and identification.
        public string Name { get; set; } = "Enemy";
        public CombatTeam Team => CombatTeam.Enemy;
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; set; }

        // Quick checks for the engine to know if this enemy should be updated, drawn, or targeted.
        public bool IsAlive => Health > 0 && _lifecycleState == ENEMYSTATE.ALIVE;
        public bool IsActive => Visible && _lifecycleState != ENEMYSTATE.DEAD;

        private ICombatant _currentTarget;

        // When the AI locks onto a target, we also update the underlying sprite's combat partner variable.
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

        // Variables required by INpcActor so the different AI states can move the enemy and manage its attacks.
        public float Velocity { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 WanderTarget { get; set; }
        public float AttackCooldown { get; set; }
        public float AttackRange { get; set; } = GameConstants.MeleeAttackRange;

        protected int _deathCountdown;

        // The active AI behavior (like WanderState, ChaseState, etc.).
        public IAIState CurrentState { get; private set; }

        // Cleanly swaps the AI state by triggering the exit logic of the old one before entering the new one.
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

        // Primary constructor for complex enemies that use a dictionary of different sprite sheets for their animations.
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

        // Legacy constructor for simpler enemies that only have a single sprite sheet.
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
            // Pull default starting values from our GameConstants file to make global balancing easier.
            StartPosition = pos;
            _previousPosition = pos;
            Velocity = GameConstants.DefaultEnemySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;

            // Kick off the AI state machine.
            ChangeState(new ChargeState());
        }

        private void SetupHealthBar(Game g)
        {
            // If it hasn't been created yet, generate the single white pixel we use to stretch out into health bars.
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }
        }

        public override void Update(GameTime gametime)
        {
            // Handle knockback physics. We apply the velocity to the position, then use friction to slow it down over time.
            if (_knockBackVelocity != Vector2.Zero)
            {
                position += _knockBackVelocity;
                _knockBackVelocity *= GameConstants.KnockbackFriction;
                if (_knockBackVelocity.Length() < 0.1f) _knockBackVelocity = Vector2.Zero;
            }

            base.Update(gametime);
            UpdateDirection();

            // A priority system to figure out which animation we should be playing based on what the state machine is doing.
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

        // An overloaded update so the level engine can pass in collision obstacles for the AI to path around.
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
            // If stunned by a spell or effect, completely skip the AI logic for this frame.
            if (IsStunned) return;

            // Tick down the attack cooldown so we can hit the player again.
            if (AttackCooldown > 0)
                AttackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check with the CombatSystem to see if our target is dead
            // If they are, drop the target and go back to wandering.
            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                if (!(CurrentState is WanderState)) ChangeState(new WanderState());
            }

            // Run the actual behavior logic for whatever state we are currently in.
            CurrentState?.Update(this, gameTime, obstacles);
        }

        public virtual void ApplyKnockback(Vector2 force)
        {
            _knockBackVelocity += force;
        }

        protected virtual void UpdateDying(GameTime gameTime)
        {
            // Give the death animation a brief moment to play before we flag the enemy as fully dead and hide it.
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

            // If we ran out of health, trigger the death sequence. Otherwise, flinch by entering the HurtState.
            if (Health <= 0) Die();
            else ChangeState(new HurtState());
        }

        public virtual void PerformAttack()
        {
            if (_currentTarget == null || AttackCooldown > 0) return;

            _animManager.Play("Attack");

            // Tell the CombatSystem to apply damage to our target, then reset our swing timer.
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
            // First try to look directly at our target.
            if (CurrentTarget != null)
            {
                Vector2 diff = CurrentTarget.Position - this.Position;
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    _currentDirectionIndex = (diff.X > 0) ? 3 : 2;
                else
                    _currentDirectionIndex = (diff.Y > 0) ? 0 : 1;
            }
            // If we don't have a target, look in the direction we are currently walking based on our last position.
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
            // Pass our position, scale, and facing direction over to the animation manager to draw the correct frame.
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
            // Draw a scalable health bar above the enemy. 
            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - spriteHeight / 2 - 10;

            // First draw the red background.
            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            // Calculate health percentage and pick a color (Green for high, Orange for medium, Red for low).
            float healthPercent = (float)Health / MaxHealth;
            int currentBarWidth = (int)(barWidth * healthPercent);
            Color healthColor = healthPercent > 0.6f ? Color.Green : healthPercent > 0.3f ? Color.Orange : Color.Red;

            // Draw the colored bar representing current health over the background.
            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), healthColor);
        }
    }
}