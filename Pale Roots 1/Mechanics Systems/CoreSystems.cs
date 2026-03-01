using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // Shared numeric constants used by gameplay systems like Player, Enemy, Camera, and AI.
    // Other classes read these values for speeds, ranges, timers, and map dimensions.
    // i know this is a lot of constants but it helps keep magic numbers out of the code and makes tuning easier.
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

    // Team labels used by CombatSystem and AI to determine friend or foe relationships.
    public enum CombatTeam
    {
        Player,
        Enemy,
        Neutral
    }

    // Minimal interface implemented by Player, Enemy, Ally and other combat-capable objects.
    // CombatSystem and AI use this to apply damage, assign targets, and query status.
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

    // Central combat helper used to deal damage, manage targets, and emit combat events.
    // Systems subscribe to events and use the RNG helpers to produce consistent randomness for every run.
    public static class CombatSystem
    {
        // Events for damage, kills, and target assignment used across the project.
        public static event Action<ICombatant, ICombatant, int> OnDamageDealt;
        public static event Action<ICombatant, ICombatant> OnCombatantKilled;
        public static event Action<ICombatant, ICombatant> OnTargetAcquired;

        // Single Random instance used everywhere to avoid duplicate seeding issues.
        private static readonly Random _random = new Random();

        public static int RandomInt(int min, int max) => _random.Next(min, max);
        public static float RandomFloat() => (float)_random.NextDouble();
        public static float RandomFloat(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        // Apply damage from attacker to target, invoke events, and handle death.
        public static int DealDamage(ICombatant attacker, ICombatant target, int baseDamage, float multiplier = 1.0f)
        {
            if (target == null || !target.IsAlive) return 0;
            if (baseDamage <= 0) return 0;

            int finalBase = (int)(baseDamage * multiplier);

            // Small variance so damage is not identical every hit.
            float variance = RandomFloat(0.9f, 1.1f);
            int finalDamage = Math.Max(1, (int)(baseDamage * variance));

            target.TakeDamage(finalDamage, attacker);

            OnDamageDealt?.Invoke(attacker, target, finalDamage);

            if (!target.IsAlive)
            {
                HandleKill(attacker, target);
            }

            return finalDamage;
        }
        private static void HandleKill(ICombatant killer, ICombatant victim)
        {
            if (victim.CurrentTarget != null)
            {
                victim.CurrentTarget.AttackerCount--;
                victim.CurrentTarget = null;
            }

            victim.Die();
            OnCombatantKilled?.Invoke(killer, victim);
        }

        // Set a new target for a combatant and maintain the AttackerCount on the target.
        // Other systems listen to OnTargetAcquired to react to targeting changes.
        public static void AssignTarget(ICombatant combatant, ICombatant newTarget)
        {
            if (combatant == null) return;

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

        // Clear the current target and update attacker counts.
        public static void ClearTarget(ICombatant combatant)
        {
            if (combatant?.CurrentTarget != null)
            {
                combatant.CurrentTarget.AttackerCount--;
                combatant.CurrentTarget = null;
            }
        }

        // Returns true when the two combatants are on opposing teams.
        public static bool AreEnemies(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return false;
            return a.Team != b.Team && a.Team != CombatTeam.Neutral && b.Team != CombatTeam.Neutral;
        }

        // Validate that a target is alive, active, and an enemy.
        public static bool IsValidTarget(ICombatant attacker, ICombatant target)
        {
            if (target == null) return false;
            if (!target.IsAlive) return false;
            if (!target.IsActive) return false;
            if (!AreEnemies(attacker, target)) return false;
            return true;
        }

        // Check if attacker is within range to attack the target using their centers.
        public static bool CanAttack(ICombatant attacker, ICombatant target, float range = -1)
        {
            if (!IsValidTarget(attacker, target)) return false;

            if (range < 0) range = GameConstants.MeleeAttackRange;

            float distance = Vector2.Distance(attacker.Center, target.Center);
            return distance <= range;
        }

        // Utility to compute distance between two combatants used by AI and targeting code.
        public static float GetDistance(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector2.Distance(a.Center, b.Center);
        }

        // Remove any subscribed event handlers, useful during teardown or tests.
        public static void ClearAllEvents()
        {
            OnDamageDealt = null;
            OnCombatantKilled = null;
            OnTargetAcquired = null;
        }
    }
}
