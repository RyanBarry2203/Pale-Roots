using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Ally : RotatingSprite, INpcActor
    {
        public enum ALLYSTATE { ALIVE, DYING, DEAD }

        private AnimationManager _animManager;
        private int _currentDirectionIndex = 1;
        private SpriteEffects _flipEffect = SpriteEffects.None;
        private static Texture2D _healthBarTexture;
        private bool _drawHealthBar = true;

        public ALLYSTATE LifecycleState { get; set; } = ALLYSTATE.ALIVE;

        public string Name { get; set; } = "Ally";
        public CombatTeam Team => CombatTeam.Player;
        public int MaxHealth { get; protected set; }
        public int AttackDamage { get; protected set; }
        public bool IsAlive => Health > 0 && LifecycleState == ALLYSTATE.ALIVE;
        public bool IsActive => Visible && LifecycleState != ALLYSTATE.DEAD;

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
        public float Velocity { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 WanderTarget { get; set; }
        public float AttackCooldown { get; set; }
        private int _deathCountdown;

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

        public Ally(Game g, Dictionary<string, Texture2D> textures, Vector2 userPosition, int framecount)
            : base(g, textures["Walk"], userPosition, framecount)
        {
            StartPosition = userPosition;
            Velocity = GameConstants.DefaultAllySpeed;
            MaxHealth = GameConstants.DefaultHealth;
            Health = MaxHealth;
            AttackDamage = GameConstants.DefaultMeleeDamage;
            _deathCountdown = GameConstants.DeathCountdown;
            Scale = 3.0f;

            _animManager = new AnimationManager();
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 4, 0, 200f, true, 4, 0, true));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 4, 0, 125f, true, 4, 0, true));
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 6, 0, 175f, false, 4, 0, true));
            _animManager.AddAnimation("Hurt", new Animation(textures["Idle"], 4, 0, 150f, false, 4, 0, true)); // Fallback so HurtState doesn't crash
            _animManager.Play("Idle");

            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            // Start the brain!
            ChangeState(new ChargeState());
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            UpdateDirection();

            string animKey = "Idle";
            if (LifecycleState == ALLYSTATE.DYING) animKey = "Idle";
            else if (CurrentState is HurtState) animKey = "Hurt";
            else if (CurrentState is CombatState && AttackCooldown > 800) animKey = "Attack";
            else if (Velocity > 0.1f || CurrentState is ChaseState || CurrentState is ChargeState) animKey = "Walk";

            _animManager.Play(animKey);
            _animManager.Update(gametime);

            if (LifecycleState == ALLYSTATE.DYING) UpdateDying(gametime);
        }

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
            if (AttackCooldown > 0) AttackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_currentTarget != null && !CombatSystem.IsValidTarget(this, _currentTarget))
            {
                CombatSystem.ClearTarget(this);
                if (!(CurrentState is WanderState)) ChangeState(new WanderState());
            }

            CurrentState?.Update(this, gameTime, obstacles);
        }

        protected virtual void UpdateDying(GameTime gameTime)
        {
            _deathCountdown--;
            if (_deathCountdown <= 0) { LifecycleState = ALLYSTATE.DEAD; Visible = false; }
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
            LifecycleState = ALLYSTATE.DYING;
            _deathCountdown = GameConstants.DeathCountdown;
            CombatSystem.ClearTarget(this);
        }

        private void UpdateDirection()
        {
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
            if (Visible) _animManager.Draw(spriteBatch, position, (float)Scale, _flipEffect, _currentDirectionIndex);
            if (_drawHealthBar && IsAlive) DrawHealthBar(spriteBatch);
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