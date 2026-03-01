using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Boss enemy that manipulates gravity to pull or push the player.
    public class BlackHoleBoss : Enemy
    {
        // Timer that controls how often gravity abilities can be used.
        private float _gravityCooldownTimer = 0f;
        // Cooldown duration for gravity abilities in milliseconds.
        private const float GravityCooldownDuration = 6000f;

        // Damage accumulated since the last gravity ability.
        private int _damageTakenSinceLastGravity = 0;
        // Damage threshold that triggers a repel burst.
        private const int RepelDamageThreshold = 200;
        // Multiplier applied to gravity force calculations.
        public float GravityMultiplier { get; set; } = 1.0f;

        public BlackHoleBoss(Game game, Dictionary<string, Texture2D> textures, Vector2 pos)
    : base(game, textures, pos, 4)
        {
            // Initialize boss stats and size.
            Name = "Space Tear Golem";
            MaxHealth = 2000;
            Health = MaxHealth;
            Scale = 7.0f;
            AttackDamage = 40;
            AttackRange = 150f;

            // Register the boss animations with the animation manager.
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 11, 0, 100f, false, 1, 0, false));
            _animManager.AddAnimation("Death", new Animation(textures["Death"], 13, 0, 150f, false, 1, 0, false));
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 8, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 10, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Hurt", new Animation(textures["Hurt"], 4, 0, 150f, false, 1, 0, false));

            // Start the boss in the idle state.
            ChangeState(new BossIdleState());

        }

        // Reduce health using the base logic and track damage to trigger defensive gravity.
        public override void TakeDamage(int amount, ICombatant attacker)
        {
            // Let the base class apply damage and death handling.
            base.TakeDamage(amount, attacker);

            // Accumulate the damage for gravity triggers.
            _damageTakenSinceLastGravity += amount;

            // Trigger a repel burst if damage exceeds the threshold and the ability is ready.
            if (_damageTakenSinceLastGravity >= RepelDamageThreshold && _gravityCooldownTimer <= 0)
            {
                ExecuteGravityBurst(attacker, false);
            }
        }

        // Custom update called during the boss fight to manage gravity use and facing.
        public void UpdateBossLogic(GameTime gameTime, Player player)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Decrease the gravity cooldown timer each frame.
            if (_gravityCooldownTimer > 0)
            {
                _gravityCooldownTimer -= dt;
            }
            else if (CurrentState is ChaseState)
            {
                // If the player is far from the boss, use the sucking ability to pull them closer.
                if (Vector2.Distance(this.Center, player.Center) > 320f)
                {
                    ExecuteGravityBurst(player, true);
                }
            }

            // Flip the sprite to face the player's horizontal position.
            float xDifference = player.Center.X - this.Center.X;
            if (xDifference < 0) _flipEffect = SpriteEffects.FlipHorizontally;
            else _flipEffect = SpriteEffects.None;

            // Run the standard enemy update for animations and state transitions.
            base.Update(gameTime);
        }

        // Apply a gravity force to the target to either pull or push it.
        private void ExecuteGravityBurst(ICombatant target, bool isSucking)
        {
            if (target is Player p)
            {
                // Compute the normalized direction from the player to the boss.
                Vector2 direction = this.Center - p.Center;
                if (direction != Vector2.Zero) direction.Normalize();

                // Determine the force magnitude and apply the gravity multiplier.
                float forceAmount = (isSucking ? 30f : -45f) * GravityMultiplier;

                // Apply the calculated force to the player through its physics method.
                p.ApplyExternalForce(direction * forceAmount);

                // Reset the gravity ability cooldown and damage tracker.
                _gravityCooldownTimer = GravityCooldownDuration;
                _damageTakenSinceLastGravity = 0;
            }
        }

        // Reduce incoming knockback for large boss to make him feel strogner and more powerful than other enemies.
        public override void ApplyKnockback(Vector2 force)
        {
            base.ApplyKnockback(force * 0.05f);
        }

        public override void Die()
        {
            // Use a longer death countdown so the full death animation can finish.
            base.Die();
            _deathCountdown = 150;
        }

        protected override void DrawHealthBar(SpriteBatch spriteBatch)
        {
            // Do not draw the standard floating health bar for this boss.
            // The UIManager draws the boss health bar in the HUD instead, again reinforce the significance of this boss.
        }
    }
}