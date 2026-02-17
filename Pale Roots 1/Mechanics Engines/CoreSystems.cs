using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // CoreSystems contains global constants, interface and the CombatSystem which coordinates combat.
    // Purpose and interactions:
    // - GameConstants centralizes magic numbers used by many modules (Player, Enemy, Projectiles, Camera, etc.).
    // - ICombatant is the shared contract for any actor that can fight; systems rely on this to handle damage, targeting and queries.
    // - CombatSystem is the authoritative place for damage resolution, random utilities and target bookkeeping; it emits events that UI or audio/particle systems can subscribe to.
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
    }

    public enum CombatTeam
    {
        Player,
        Enemy,
        Neutral
    }

    // Shared contract used by Player, Enemy, Ally and any other combat-capable actor.
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

    // Central combat coordination: damage, target assignment, validation and events.
    // Systems that need to apply damage or query validity call into CombatSystem so bookkeeping (attacker counts, events) stays consistent.
    public static class CombatSystem
    {
        // Events for external systems (UI, sounds, scoring) to react to combat outcomes.
        public static event Action<ICombatant, ICombatant, int> OnDamageDealt;
        public static event Action<ICombatant, ICombatant> OnCombatantKilled;
        public static event Action<ICombatant, ICombatant> OnTargetAcquired;

        // Single shared RNG used across the game to avoid many Random instances with identical seeds.
        private static readonly Random _random = new Random();

        public static int RandomInt(int min, int max) => _random.Next(min, max);
        public static float RandomFloat() => (float)_random.NextDouble();
        public static float RandomFloat(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        // Apply damage with a small variance, notify listeners and perform kill handling.
        public static int DealDamage(ICombatant attacker, ICombatant target, int baseDamage, float multiplier = 1.0f)
        {
            if (target == null || !target.IsAlive) return 0;
            if (baseDamage <= 0) return 0;

            int finalBase = (int)(baseDamage * multiplier);

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

        // Clean up references and fire kill event. This avoids dangling attacker counts.
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

        // Assign a target and maintain AttackerCount on the target. Emits OnTargetAcquired for systems to react.
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

        // Clears a combatant's target and adjusts attacker bookkeeping.
        public static void ClearTarget(ICombatant combatant)
        {
            if (combatant?.CurrentTarget != null)
            {
                combatant.CurrentTarget.AttackerCount--;
                combatant.CurrentTarget = null;
            }
        }

        // Helper to determine if two combatants are on opposing teams (ignores Neutral).
        public static bool AreEnemies(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return false;
            return a.Team != b.Team && a.Team != CombatTeam.Neutral && b.Team != CombatTeam.Neutral;
        }

        // Validates whether a target can be attacked (alive, active, and an enemy).
        public static bool IsValidTarget(ICombatant attacker, ICombatant target)
        {
            if (target == null) return false;
            if (!target.IsAlive) return false;
            if (!target.IsActive) return false;
            if (!AreEnemies(attacker, target)) return false;
            return true;
        }

        // Range check using centers of combatants. Default uses MeleeAttackRange.
        public static bool CanAttack(ICombatant attacker, ICombatant target, float range = -1)
        {
            if (!IsValidTarget(attacker, target)) return false;

            if (range < 0) range = GameConstants.MeleeAttackRange;

            float distance = Vector2.Distance(attacker.Center, target.Center);
            return distance <= range;
        }

        // Utility to return distance between entities for targeting decisions.
        public static float GetDistance(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector2.Distance(a.Center, b.Center);
        }

        // Reset event subscriptions (useful during scene teardown or tests).
        public static void ClearAllEvents()
        {
            OnDamageDealt = null;
            OnCombatantKilled = null;
            OnTargetAcquired = null;
        }
    }
}
