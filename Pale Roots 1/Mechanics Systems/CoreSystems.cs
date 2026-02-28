using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // This is our master configuration file. 
    // By keeping all our speeds, cooldowns, and damage values here, we avoid "magic numbers" in our code.
    // If we want to balance the game later, we only have to change the numbers in this one file.
    public static class GameConstants
    {
        public const float SwordSwingDuration = 250f;
        public const float SwordCooldown = 500f;
        public const int SwordDamage = 25;
        public const float SwordRange = 60f;
        public const float SwordKnockback = 15f;
        public const float SwordArcWidth = 60f;

        public const float MeleeAttackRange = 85f;
        public const float CombatEngageRange = 70f;
        public const float CombatBreakRange = 100f;
        public const float DefaultDetectionRadius = 400f;
        public const float DefaultChaseRadius = 200f;

        public const int DefaultHealth = 100;
        public const int DefaultMeleeDamage = 15;
        public const float DefaultAttackCooldown = 1000f;
        public const float TargetScanInterval = 500f;

        // This prevents 50 enemies from swarming a single target and causing massive visual clipping.
        public const int MaxAttackersPerTarget = 2;

        public const float DefaultEnemySpeed = 3.0f;
        public const float DefaultAllySpeed = 3.0f;
        public const float DefaultPlayerSpeed = 4.0f;
        public const float ChargingSpeed = 3.0f;

        public const float DefaultProjectileSpeed = 4.0f;
        public const float DefaultReloadTime = 2000f;
        public const float ExplosionDuration = 1000f;

        public const int DeathCountdown = 30;
        public const int WanderRadius = 300;
        public const float KnockbackFriction = 0.9f;

        public const int TileSize = 64;
        public static readonly Vector2 DefaultMapSize = new Vector2(3840, 2160);

        public const int WinConditionKills = 120;
    }

    // A simple tag used by the AI to quickly figure out who to attack and who to ignore.
    public enum CombatTeam
    {
        Player,
        Enemy,
        Neutral
    }

    // The universal contract for anything that can fight and die in our game.
    // By forcing the Player, Allies, and Enemies to all implement this, the CombatSystem can run 
    // damage math on them without needing to know exactly what specific class they are.
    public interface ICombatant
    {
        string Name { get; }
        CombatTeam Team { get; }
        int Health { get; set; }
        int MaxHealth { get; }
        int AttackDamage { get; }
        bool IsAlive { get; }
        ICombatant CurrentTarget { get; set; }
        int AttackerCount { get; set; }
        bool IsActive { get; }
        Vector2 Position { get; }
        Vector2 Center { get; }
        void TakeDamage(int amount, ICombatant attacker);
        void PerformAttack();
        void Die();
    }

    // This acts as the global referee for the game. 
    // It manages the random number generator, calculates damage variances, and tracks who is targeting whom.
    public static class CombatSystem
    {
        // Global events that other systems (like the ChaseAndFireEngine or UIManager) can listen to.
        // This allows us to trigger UI updates, sound effects, or enemy spawns without hardcoding those systems directly into the combat math.
        public static event Action<ICombatant, ICombatant, int> OnDamageDealt;
        public static event Action<ICombatant, ICombatant> OnCombatantKilled;
        public static event Action<ICombatant, ICombatant> OnTargetAcquired;

        // We instantiate exactly one Random object for the entire project. 
        // Creating new Random() instances rapidly in an Update loop can cause them to generate the exact same "random" number.
        private static readonly Random _random = new Random();

        public static int RandomInt(int min, int max) => _random.Next(min, max);
        public static float RandomFloat() => (float)_random.NextDouble();
        public static float RandomFloat(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        // The core method that safely deducts health.
        public static int DealDamage(ICombatant attacker, ICombatant target, int baseDamage, float multiplier = 1.0f)
        {
            if (target == null || !target.IsAlive) return 0;
            if (baseDamage <= 0) return 0;

            int finalBase = (int)(baseDamage * multiplier);

            // Add a tiny bit of math to slightly randomize the damage. 
            // It makes combat feel more dynamic if you hit for 14, 16, 15 instead of exactly 15 every time.
            float variance = RandomFloat(0.9f, 1.1f);
            int finalDamage = Math.Max(1, (int)(baseDamage * variance));

            // Tell the target to actually process the damage.
            target.TakeDamage(finalDamage, attacker);

            // Shout out to the rest of the engine that a hit just landed.
            OnDamageDealt?.Invoke(attacker, target, finalDamage);

            // If that specific hit pushed their health to 0 or below, trigger the death sequence.
            if (!target.IsAlive)
            {
                HandleKill(attacker, target);
            }

            return finalDamage;
        }

        private static void HandleKill(ICombatant killer, ICombatant victim)
        {
            // If the dying character was locking up a slot on someone else (preventing them from being attacked), 
            // free that slot up so the remaining enemies can swarm them.
            if (victim.CurrentTarget != null)
            {
                victim.CurrentTarget.AttackerCount--;
                victim.CurrentTarget = null;
            }

            victim.Die();
            OnCombatantKilled?.Invoke(killer, victim);
        }

        // Safely hooks a seeker onto a new target, updating the target's internal counter 
        // so we don't accidentally exceed our MaxAttackersPerTarget limit.
        public static void AssignTarget(ICombatant combatant, ICombatant newTarget)
        {
            if (combatant == null) return;

            // If we were already attacking someone else, make sure to let them go first.
            if (combatant.CurrentTarget != null && combatant.CurrentTarget != newTarget)
            {
                combatant.CurrentTarget.AttackerCount--;
            }

            combatant.CurrentTarget = newTarget;

            if (newTarget != null)
            {
                newTarget.AttackerCount++;
                OnTargetAcquired?.Invoke(combatant, newTarget);
            }
        }

        // Unhooks a combatant from whatever they are currently fighting.
        public static void ClearTarget(ICombatant combatant)
        {
            if (combatant?.CurrentTarget != null)
            {
                combatant.CurrentTarget.AttackerCount--;
                combatant.CurrentTarget = null;
            }
        }

        // Quick logic check to make sure Allies don't attack the Player, and nobody attacks Neutral objects.
        public static bool AreEnemies(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return false;
            return a.Team != b.Team && a.Team != CombatTeam.Neutral && b.Team != CombatTeam.Neutral;
        }

        // Used heavily by the AI state machines to verify if the thing they want to punch is actually a valid, living target.
        public static bool IsValidTarget(ICombatant attacker, ICombatant target)
        {
            if (target == null) return false;
            if (!target.IsAlive) return false;
            if (!target.IsActive) return false;
            if (!AreEnemies(attacker, target)) return false;
            return true;
        }

        // Checks if the distance between two fighters is small enough to land a hit.
        public static bool CanAttack(ICombatant attacker, ICombatant target, float range = -1)
        {
            if (!IsValidTarget(attacker, target)) return false;

            // If no custom range was provided (like a projectile), fall back to our global melee distance.
            if (range < 0) range = GameConstants.MeleeAttackRange;

            float distance = Vector2.Distance(attacker.Center, target.Center);
            return distance <= range;
        }

        // A quick helper that returns the exact pixel distance between two entities.
        public static float GetDistance(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector2.Distance(a.Center, b.Center);
        }

        // This is crucial for cleanly resetting the game. It wipes out all the event listeners 
        // so old, dead states don't accidentally try to process new damage calculations.
        public static void ClearAllEvents()
        {
            OnDamageDealt = null;
            OnCombatantKilled = null;
            OnTargetAcquired = null;
        }
    }
}