using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class BlackHoleBoss : Enemy
    {
        // --- THE NEW BRAIN VARIABLES ---
        private float _gravityCooldownTimer = 0f;
        private const float GravityCooldownDuration = 6000f;

        private int _damageTakenSinceLastGravity = 0;
        private const int RepelDamageThreshold = 200;
        public float GravityMultiplier { get; set; } = 1.0f;

        public BlackHoleBoss(Game game, Dictionary<string, Texture2D> textures, Vector2 pos)
    : base(game, textures, pos, 4)
        {

            Name = "Event Horizon Golem";
            MaxHealth = 2000;
            Health = MaxHealth;
            Scale = 7.0f;
            AttackDamage = 40;

            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 11, 0, 100f, false, 1, 0, false));
            _animManager.AddAnimation("Death", new Animation(textures["Death"], 13, 0, 150f, false, 1, 0, false));
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 8, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 10, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Hurt", new Animation(textures["Hurt"], 4, 0, 150f, false, 1, 0, false));

            ChangeState(new BossIdleState());

        }

        // 1. DEFENSIVE BRAIN: Override TakeDamage to track damage and trigger Repel
        public override void TakeDamage(int amount, ICombatant attacker)
        {
            // Always let the base class handle actually reducing health and checking for death [1]
            base.TakeDamage(amount, attacker);

            _damageTakenSinceLastGravity += amount;

            // If we took a lot of damage and the cooldown is ready, BLAST THEM AWAY
            if (_damageTakenSinceLastGravity >= RepelDamageThreshold && _gravityCooldownTimer <= 0)
            {
                ExecuteGravityBurst(attacker, false); // false = repel
            }
        }

        // 2. OFFENSIVE BRAIN: Update loop to handle chasing and Sucking
        public void UpdateBossLogic(GameTime gameTime, Player player)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Handle Cooldown
            if (_gravityCooldownTimer > 0)
            {
                _gravityCooldownTimer -= dt;
            }
            else if (CurrentState is ChaseState)
            {
                // The boss wants to attack (is in ChaseState) [2].
                // If the player is outside of melee range and cooldown is ready, SUCK THEM IN.
                if (Vector2.Distance(this.Center, player.Center) > 250f)
                {
                    ExecuteGravityBurst(player, true); // true = suck
                }
            }

            // Standard Geometry Flipping [3]
            float xDifference = player.Center.X - this.Center.X;
            if (xDifference < 0) _flipEffect = SpriteEffects.FlipHorizontally;
            else _flipEffect = SpriteEffects.None;

            base.Update(gameTime);
        }

        // 3. THE ACTION: Calculate the discrete spatial shift
        private void ExecuteGravityBurst(ICombatant target, bool isSucking)
        {
            if (target is Player p)
            {
                Vector2 direction = this.Center - p.Center;
                if (direction != Vector2.Zero) direction.Normalize();

                // MATH WIN: 30f velocity * 0.9f friction = ~300 pixels of total smooth sliding
                // -45f velocity * 0.9f friction = ~450 pixels of total smooth repel sliding
                float forceAmount = (isSucking ? 30f : -45f) * GravityMultiplier;

                p.ApplyExternalForce(direction * forceAmount);

                _gravityCooldownTimer = GravityCooldownDuration;
                _damageTakenSinceLastGravity = 0;
            }
        }

        // Keep your mass/knockback resistance!
        public override void ApplyKnockback(Vector2 force)
        {
            base.ApplyKnockback(force * 0.05f);
        }
        public override void Die()
        {
            base.Die();
            _deathCountdown = 150;
        }
    }
}