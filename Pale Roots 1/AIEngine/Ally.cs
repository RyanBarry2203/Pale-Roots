using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Represents a friendly NPC that fights alongside the player.
    // Extends RotatingSprite and implements INpcActor for AI and rendering.
    public class Ally : RotatingSprite, INpcActor
    {
        // Tracks the ally's lifecycle for death handling.
        public enum ALLYSTATE { ALIVE, DYING, DEAD }

        private AnimationManager _animManager;
        private int _currentDirectionIndex = 1;
        private SpriteEffects _flipEffect = SpriteEffects.None;

        // Shared 1x1 texture used to draw health bars.
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        public ALLYSTATE LifecycleState { get; set; } = ALLYSTATE.ALIVE;

        // Basic RPG stats and identification.
        public string Name { get; set; } = "Ally";
        public CombatTeam Team => CombatTeam.Player;
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }

        // Quick checks for alive and active status.
        public bool IsAlive => Health > 0 && LifecycleState == ALLYSTATE.ALIVE;
        public bool IsActive => Visible && LifecycleState != ALLYSTATE.DEAD;

        private ICombatant _currentTarget;

        // Update the sprite combat partner when current target changes.
        public ICombatant CurrentTarget
        {
            get => _currentTarget;
            set
            {
                _currentTarget = value;
                CurrentCombatPartner = value as Sprite;
            }
        }

        // Movement and AI fields required by INpcActor.
        public Vector2 Position => position;
        public float Velocity { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 WanderTarget { get; set; }
        public float AttackCooldown { get; set; }
        public float AttackRange { get; set; } = GameConstants.MeleeAttackRange;
        private int _deathCountdown;

        // Current AI state such as chase or combat.
        public IAIState CurrentState { get; private set; }

        // Change states by exiting the old state and entering the new one.
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

        public Ally(Game g, Dictionary<string, Texture2D> textures, Vector2 userPosition, int framecount)
            : base(g, textures["Walk"], userPosition, framecount)
        {
            // Initialize default stats from GameConstants.
            StartPosition = userPosition;
            Velocity = GameConstants.DefaultAllySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            Scale = 3.0f;

            // Configure animations and play the idle animation.
            _animManager = new AnimationManager();
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 200f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 4, 0, 125f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 6, 0, 175f, false, 4, 0, true));
            _animManager.AddAnimation("Hurt", new Animation(textures["Idle"], 4, 0, 150f, false, 4, 0, true));
            _animManager.Play("Idle");

            // Create shared health bar texture if it does not exist.
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            // Start in the wander state.
            ChangeState(new WanderState());
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            UpdateDirection();

            // Choose animation based on state and movement.
            string animKey = "Idle";
            if (LifecycleState == ALLYSTATE.DYING) animKey = "Idle";
            else if (CurrentState is HurtState) animKey = "Hurt";
            else if (CurrentState is CombatState && AttackCooldown > 800) animKey = "Attack";
            else if (Velocity > 0.1f || CurrentState is ChaseState || CurrentState is ChargeState) animKey = "Walk";

            _animManager.Play(animKey);
            _animManager.Update(gametime);

            if (LifecycleState == ALLYSTATE.DYING) UpdateDying(gametime);
        }

        // Update with obstacle list for pathfinding.
        public void Update(GameTime gametime, List<WorldObject> obstacles)
        {
            this.Update(gametime);
            if (LifecycleState == ALLYSTATE.ALIVE)
            {
                UpdateAI(gametime, obstacles);
            }
        }

        protected virtual void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {
            // Decrease attack cooldown over time.
            if (AttackCooldown > 0) AttackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Let CombatSystem validate the current target and clear it if invalid.
            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                if (!(CurrentState is WanderState)) ChangeState(new WanderState());
            }

            // Run the current AI state's update.
            CurrentState?.Update(this, gameTime, obstacles);
        }

        protected virtual void UpdateDying(GameTime gameTime)
        {
            // Countdown to fully dead and hide the sprite.
            _deathCountdown--;
            if (_deathCountdown <= 0) { LifecycleState = ALLYSTATE.DEAD; Visible = false; }
        }

        public virtual void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;

            Health -= amount;

            // Reduce health and change to hurt or die.
            if (Health <= 0) Die();
            else ChangeState(new HurtState());
        }

        public virtual void PerformAttack()
        {
            if (_currentTarget == null || AttackCooldown > 0) return;

            _animManager.Play("Attack");

            // Apply damage to the current target via CombatSystem.
            CombatSystem.DealDamage(this, _currentTarget, AttackDamage);

            // Reset attack cooldown.
            AttackCooldown = GameConstants.DefaultAttackCooldown;
        }

        public virtual void Die()
        {
            // Enter dying state, reset death timer, and clear targets.
            LifecycleState = ALLYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            CombatSystem.ClearTarget(this);
        }

        private void UpdateDirection()
        {
            // Set facing direction based on target position.
            if (CurrentTarget != null)
            {
                Vector2 diff = CurrentTarget.Position - this.Position;
                _flipEffect = SpriteEffects.None;

                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    _currentDirectionIndex = (diff.X < 0) ? 0 : 1;
                else
                    _currentDirectionIndex = (diff.Y < 0) ? 2 : 3;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the current animation frame and optional health bar.
            if (Visible) _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect, _currentDirectionIndex);

            if (_drawHealthBar && IsAlive) DrawHealthBar(spriteBatch);
        }

        protected virtual void DrawHealthBar(SpriteBatch spriteBatch)
        {
            // Draw health bar above the sprite using the shared texture.
            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = ((int)position.Y - spriteHeight / 2 - 10) - 20;
            // Draw the red background representing missing health.
            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            // Draw the blue foreground showing current health percent.
            float healthPercent = (float)Health / MaxHealth;
            int currentBarWidth = (int)(barWidth * healthPercent);
            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.CornflowerBlue);
        }
    }
}