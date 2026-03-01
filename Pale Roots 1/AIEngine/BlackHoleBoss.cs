using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Major boss. It inherits from the base Enemy class but adds unique 
    // gravity manipulation mechanics to pull the player in or push them away.
    public class BlackHoleBoss : Enemy
    {
        // Timers and thresholds to control how often the boss can use its gravity abilities.
        // We don't want the boss spamming these moves every frame.
        private float _gravityCooldownTimer = 0f;
        private const float GravityCooldownDuration = 6000f;

        // Tracks how much damage the boss has taken recently. If it takes too much, 
        // it triggers a defensive burst to push the attacker away.
        private int _damageTakenSinceLastGravity = 0;
        private const int RepelDamageThreshold = 200;
        public float GravityMultiplier { get; set; } = 1.0f;

        public BlackHoleBoss(Game game, Dictionary<string, Texture2D> textures, Vector2 pos)
    : base(game, textures, pos, 4)
        {
            // Set up the massive stats for the boss encounter.
            Name = "Space Tear Golem";
            MaxHealth = 2000;
            Health = MaxHealth;
            Scale = 7.0f;
            AttackDamage = 40;
            AttackRange = 150f;

            // Load all the specific boss animations. Notice the frame counts and timings 
            // are unique to this massive sprite sheet.
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 11, 0, 100f, false, 1, 0, false));
            _animManager.AddAnimation("Death", new Animation(textures["Death"], 13, 0, 150f, false, 1, 0, false));
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 8, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 10, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Hurt", new Animation(textures["Hurt"], 4, 0, 150f, false, 1, 0, false));

            // The boss waits in the idle state until the player triggers the fight.
            ChangeState(new BossIdleState());

        }

        // We override the standard damage taken method to sneak in our custom defensive logic.
        public override void TakeDamage(int amount, ICombatant attacker)
        {
            // First, let the base Enemy class handle the actual health reduction and death checks.
            base.TakeDamage(amount, attacker);

            // Add the incoming damage to our tracker.
            _damageTakenSinceLastGravity += amount;

            // If the player is dealing too much damage and our ability is off cooldown, 
            // trigger the defensive repel to get them off our back.
            if (_damageTakenSinceLastGravity >= RepelDamageThreshold && _gravityCooldownTimer <= 0)
            {
                ExecuteGravityBurst(attacker, false);
            }
        }

        // This is a custom update loop specifically called during the boss fight, usually 
        // managed by the BossGameState or LevelManager so it has direct access to the player.
        public void UpdateBossLogic(GameTime gameTime, Player player)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Tick down the timer so the boss can use its gravity moves again.
            if (_gravityCooldownTimer > 0)
            {
                _gravityCooldownTimer -= dt;
            }
            else if (CurrentState is ChaseState)
            {
                // If the boss is actively trying to get to the player but the player is running away 
                // (outside of a 250 pixel range), use the offensive suck ability to pull them back in.
                if (Vector2.Distance(this.Center, player.Center) > 320f)
                {
                    ExecuteGravityBurst(player, true);
                }
            }

            // Figure out which side of the boss the player is on, and flip the sprite so 
            // the boss is always looking at them.
            float xDifference = player.Center.X - this.Center.X;
            if (xDifference < 0) _flipEffect = SpriteEffects.FlipHorizontally;
            else _flipEffect = SpriteEffects.None;

            // Run the standard Enemy update for animations and state machines.
            base.Update(gameTime);
        }

        // This handles the actual physics manipulation of the target (usually the player).
        private void ExecuteGravityBurst(ICombatant target, bool isSucking)
        {
            if (target is Player p)
            {
                // Find the straight line between the boss and the player.
                Vector2 direction = this.Center - p.Center;
                if (direction != Vector2.Zero) direction.Normalize();

                // Calculate how hard we are pushing or pulling. 
                // Positive force pulls them toward the center, negative force blasts them away.
                float forceAmount = (isSucking ? 30f : -45f) * GravityMultiplier;

                // Send the calculated force to the Player class so it can update its own physics.
                p.ApplyExternalForce(direction * forceAmount);

                // Reset our ability timers and damage trackers.
                _gravityCooldownTimer = GravityCooldownDuration;
                _damageTakenSinceLastGravity = 0;
            }
        }

        // We override this because it's a massive boss. We don't want standard attacks 
        // knocking it around the screen. We reduce all incoming knockback physics by 95%.
        public override void ApplyKnockback(Vector2 force)
        {
            base.ApplyKnockback(force * 0.05f);
        }

        public override void Die()
        {
            // Give the boss a longer death countdown so its full 13-frame death animation 
            // has time to play out before it disappears from the screen.
            base.Die();
            _deathCountdown = 150;
        }

        protected override void DrawHealthBar(SpriteBatch spriteBatch)
        {
            // Intentionally leave this blank to override the standard floating health bar.
            // For this boss, the UIManager handles drawing a massive health bar at the top of the screen.
        }
    }
}