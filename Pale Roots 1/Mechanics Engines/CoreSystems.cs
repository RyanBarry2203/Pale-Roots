using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // Shared numeric constants used across the project (speeds, ranges, timeouts, map size).
    // Consumers: Player, Enemy, Projectile, Camera, AI and other systems read these values.
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

        public const int WinConditionKills = 1;
    }

    // Team tags used by CombatSystem and AI to decide friend/foe behavior.
    public enum CombatTeam
    {
        Player,
        Enemy,
        Neutral
    }

    // Minimal interface any combat-capable actor must implement so systems can interact with it.
    // Implementers: Player, Enemy, Ally, Projectile-shaping actors, etc.
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

    // Centralized combat helper and bookkeeping.
    // - Resolves damage (with small variance), fires events, and keeps attacker counts consistent.
    // - Provides RNG helpers so the whole game uses one Random instance.
    public static class CombatSystem
    {
        // Events: UI, audio, spawning logic, or analytics subscribe to these.
        public static event Action<ICombatant, ICombatant, int> OnDamageDealt;
        public static event Action<ICombatant, ICombatant> OnCombatantKilled;
        public static event Action<ICombatant, ICombatant> OnTargetAcquired;

        // Single RNG used project-wide to avoid seed duplication issues.
        private static readonly Random _random = new Random();

        public static int RandomInt(int min, int max) => _random.Next(min, max);
        public static float RandomFloat() => (float)_random.NextDouble();
        public static float RandomFloat(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        // Apply damage to target, notify listeners and handle death bookkeeping.
        public static int DealDamage(ICombatant attacker, ICombatant target, int baseDamage, float multiplier = 1.0f)
        {
            if (target == null || !target.IsAlive) return 0;
            if (baseDamage <= 0) return 0;

            int finalBase = (int)(baseDamage * multiplier);

            // Small randomness so hits vary slightly.
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

        // Clear attacker bookkeeping and emit kill event.
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

        // Assign a new target for a combatant and maintain AttackerCount on the target.
        // Emits OnTargetAcquired so UI/AI can react.
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

        // Remove whatever target a combatant currently has and update counts.
        public static void ClearTarget(ICombatant combatant)
        {
            if (combatant?.CurrentTarget != null)
            {
                combatant.CurrentTarget.AttackerCount--;
                combatant.CurrentTarget = null;
            }
        }

        // True if teams differ and neither is Neutral.
        public static bool AreEnemies(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return false;
            return a.Team != b.Team && a.Team != CombatTeam.Neutral && b.Team != CombatTeam.Neutral;
        }

        // Checks that the target is alive, active and an enemy.
        public static bool IsValidTarget(ICombatant attacker, ICombatant target)
        {
            if (target == null) return false;
            if (!target.IsAlive) return false;
            if (!target.IsActive) return false;
            if (!AreEnemies(attacker, target)) return false;
            return true;
        }

        // Distance-based attack range check using entity centers.
        public static bool CanAttack(ICombatant attacker, ICombatant target, float range = -1)
        {
            if (!IsValidTarget(attacker, target)) return false;

            if (range < 0) range = GameConstants.MeleeAttackRange;

            float distance = Vector2.Distance(attacker.Center, target.Center);
            return distance <= range;
        }

        // Utility used by targeting logic to choose nearest candidates.
        public static float GetDistance(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector2.Distance(a.Center, b.Center);
        }

        // Helper to remove external event handlers (useful during teardown/tests).
        public static void ClearAllEvents()
        {
            OnDamageDealt = null;
            OnCombatantKilled = null;
            OnTargetAcquired = null;
        }
    }
}
