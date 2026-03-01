using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // This is the blueprint for all friendly NPCs that fight alongside the player.
    // It inherits from RotatingSprite for drawing and implements INpcActor so the AI state machine can control it.
    public class Ally : RotatingSprite, INpcActor
    {
        // Tracks the basic life cycle of the character to handle death animations and cleanup.
        public enum ALLYSTATE { ALIVE, DYING, DEAD }

        private AnimationManager _animManager;
        private int _currentDirectionIndex = 1;
        private SpriteEffects _flipEffect = SpriteEffects.None;

        // We use a single shared 1x1 pixel texture to draw health bars for all allies, saving memory.
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        public ALLYSTATE LifecycleState { get; set; } = ALLYSTATE.ALIVE;

        // Basic RPG stats and identification.
        public string Name { get; set; } = "Ally";
        public CombatTeam Team => CombatTeam.Player;
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }

        // Quick checks to see if this ally should still be updated, drawn, or targeted by enemies.
        public bool IsAlive => Health > 0 && LifecycleState == ALLYSTATE.ALIVE;
        public bool IsActive => Visible && LifecycleState != ALLYSTATE.DEAD;

        private ICombatant _currentTarget;

        // When we lock onto a new target, we also update the CurrentCombatPartner so the underlying sprite logic knows about it.
        public ICombatant CurrentTarget
        {
            get => _currentTarget;
            set
            {
                _currentTarget = value;
                CurrentCombatPartner = value as Sprite;
            }
        }

        // Movement and AI variables needed by the INpcActor interface.
        public Vector2 Position => position;
        public float Velocity { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 WanderTarget { get; set; }
        public float AttackCooldown { get; set; }
        public float AttackRange { get; set; } = GameConstants.MeleeAttackRange;
        private int _deathCountdown;

        // Holds whatever behavior state the ally is currently executing (like ChaseState or CombatState).
        public IAIState CurrentState { get; private set; }

        // The core of our State Machine. We cleanly exit the old state before entering the new one.
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
            // Pull all our default starting stats from the GameConstants file so they are easy to balance later.
            StartPosition = userPosition;
            Velocity = GameConstants.DefaultAllySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            Scale = 3.0f;

            // Set up all the sprite sheets and timings for our different animations.
            _animManager = new AnimationManager();
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 200f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 4, 0, 125f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 6, 0, 175f, false, 4, 0, true));
            _animManager.AddAnimation("Hurt", new Animation(textures["Idle"], 4, 0, 150f, false, 4, 0, true)); // Fallback so HurtState doesn't crash
            _animManager.Play("Idle");

            // If this is the first ally created, generate the 1x1 white pixel texture for the health bars.
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            // Start the brain
            ChangeState(new WanderState());
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            UpdateDirection();

            // A simple priority system to figure out which animation should be playing based on what the AI is currently doing.
            string animKey = "Idle";
            if (LifecycleState == ALLYSTATE.DYING) animKey = "Idle";
            else if (CurrentState is HurtState) animKey = "Hurt";
            else if (CurrentState is CombatState && AttackCooldown > 800) animKey = "Attack";
            else if (Velocity > 0.1f || CurrentState is ChaseState || CurrentState is ChargeState) animKey = "Walk";

            _animManager.Play(animKey);
            _animManager.Update(gametime);

            if (LifecycleState == ALLYSTATE.DYING) UpdateDying(gametime);
        }

        // An overloaded update specifically for when the game needs to pass in obstacles for pathfinding.
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
            // Constantly tick down the attack cooldown so we can swing again.
            if (AttackCooldown > 0) AttackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check with the CombatSystem to see if the enemy we are chasing is dead or out of bounds.
            // If they are gone, drop the target and go back to wandering.
            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                if (!(CurrentState is WanderState)) ChangeState(new WanderState());
            }

            // Tell the current AI state (Chase, Combat, etc.) to run its logic for this frame.
            CurrentState?.Update(this, gameTime, obstacles);
        }

        protected virtual void UpdateDying(GameTime gameTime)
        {
            // Keep the character on screen for a brief moment after health hits 0, then flag them as completely dead.
            _deathCountdown--;
            if (_deathCountdown <= 0) { LifecycleState = ALLYSTATE.DEAD; Visible = false; }
        }

        public virtual void TakeDamage(int amount, ICombatant attacker)
        {
            if (!IsAlive) return;

            Health -= amount;

            // If the hit killed us, trigger the death sequence. Otherwise, flinch by going into the HurtState.
            if (Health <= 0) Die();
            else ChangeState(new HurtState());
        }

        public virtual void PerformAttack()
        {
            if (_currentTarget == null || AttackCooldown > 0) return;

            _animManager.Play("Attack");

            // Tell the CombatSystem to actually apply the math and hurt the target.
            CombatSystem.DealDamage(this, _currentTarget, AttackDamage);

            // Reset the swing timer based on our constants file.
            AttackCooldown = GameConstants.DefaultAttackCooldown;
        }

        public virtual void Die()
        {
            // Transition out of the alive state, start the cleanup timer, and make sure we aren't locking onto enemies anymore.
            LifecycleState = ALLYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            CombatSystem.ClearTarget(this);
        }

        private void UpdateDirection()
        {
            // Look at our target's position relative to ours and update our sprite's direction index (Up, Down, Left, Right) to face them.
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
            // Ask the animation manager to draw the correct frame based on our position, scale, and facing direction.
            if (Visible) _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect, _currentDirectionIndex);

            if (_drawHealthBar && IsAlive) DrawHealthBar(spriteBatch);
        }

        protected virtual void DrawHealthBar(SpriteBatch spriteBatch)
        {
            // Draw a simple, scalable health bar above the sprite's head using our 1x1 pixel texture.
            // First we draw the red background to represent missing health...
            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - spriteHeight / 2 - 10;
            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            // then we calculate the percentage of health remaining and draw a blue bar over the top of it.
            float healthPercent = (float)Health / MaxHealth;
            int currentBarWidth = (int)(barWidth * healthPercent);
            spriteBatch.Draw(_healthBarTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.CornflowerBlue);
        }
    }
}