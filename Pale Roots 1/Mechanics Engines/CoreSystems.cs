using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // ============================================================================
    // GAME CONSTANTS - All magic numbers in one place
    // ============================================================================
    
    /// <summary>
    /// Centralized game constants. No more magic numbers scattered across files.
    /// </summary>
    public static class GameConstants
    {
        // Add these to your GameConstants class
        public const float SwordSwingDuration = 300f; // 300ms swing
        public const float SwordCooldown = 500f;      // Time before you can swing again
        public const int SwordDamage = 25;
        public const float SwordRange = 60f;          // How far the sword reaches
        public const float SwordKnockback = 20f;      // How far enemies get pushed
        public const float SwordArcWidth = 60f;       // Width of the swing


        // COMBAT DISTANCES
        public const float MeleeAttackRange = 85f;
        public const float CombatEngageRange = 70f;
        public const float CombatBreakRange = 100f;
        public const float DefaultDetectionRadius = 400f;
        public const float DefaultChaseRadius = 200f;

        // COMBAT STATS
        public const int DefaultHealth = 100;
        public const int DefaultMeleeDamage = 15;
        public const float DefaultAttackCooldown = 1000f;
        public const float TargetScanInterval = 500f;
        public const int MaxAttackersPerTarget = 2;

        // MOVEMENT
        public const float DefaultEnemySpeed = 3.0f;
        public const float DefaultAllySpeed = 3.0f;
        public const float DefaultPlayerSpeed = 4.0f;
        public const float ChargingSpeed = 3.0f;

        // PROJECTILES
        public const float DefaultProjectileSpeed = 4.0f;
        public const float DefaultReloadTime = 2000f;
        public const float ExplosionDuration = 1000f;

        // DEATH & CLEANUP
        public const int DeathCountdown = 100;
        public const int WanderRadius = 100;

        // MAP & TILES
        public const int TileSize = 64;
        public static readonly Vector2 DefaultMapSize = new Vector2(1920, 1920);
    }

    // ============================================================================
    // COMBAT TEAM - Which side is a combatant on
    // ============================================================================
    
    public enum CombatTeam
    {
        Player,
        Enemy,
        Neutral
    }

    // ============================================================================
    // ICOMBATANT - Interface for anything that can fight
    // ============================================================================
    
    /// <summary>
    /// Interface for any entity that can participate in combat.
    /// </summary>
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

    // ============================================================================
    // COMBAT SYSTEM - Centralized damage and targeting
    // ============================================================================
    
    /// <summary>
    /// Centralized combat system handling all damage calculations and combat events.
    /// </summary>
    public static class CombatSystem
    {
        // EVENTS
        public static event Action<ICombatant, ICombatant, int> OnDamageDealt;
        public static event Action<ICombatant, ICombatant> OnCombatantKilled;
        public static event Action<ICombatant, ICombatant> OnTargetAcquired;

        // SHARED RANDOM - Use this instead of new Random()
        private static readonly Random _random = new Random();
        
        public static int RandomInt(int min, int max) => _random.Next(min, max);
        public static float RandomFloat() => (float)_random.NextDouble();
        public static float RandomFloat(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        /// <summary>
        /// Deal damage from attacker to target.
        /// </summary>
        public static int DealDamage(ICombatant attacker, ICombatant target, int baseDamage)
        {
            if (target == null || !target.IsAlive) return 0;
            if (baseDamage <= 0) return 0;
            
            // Calculate final damage with variance
            float variance = RandomFloat(0.9f, 1.1f);
            int finalDamage = Math.Max(1, (int)(baseDamage * variance));
            
            // Apply damage
            target.TakeDamage(finalDamage, attacker);
            
            // Fire event
            OnDamageDealt?.Invoke(attacker, target, finalDamage);
            
            // Check for kill
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

        /// <summary>
        /// Assign a target to a combatant. Handles attacker count bookkeeping.
        /// </summary>
        public static void AssignTarget(ICombatant combatant, ICombatant newTarget)
        {
            if (combatant == null) return;
            
            // Release old target
            if (combatant.CurrentTarget != null && combatant.CurrentTarget != newTarget)
            {
                combatant.CurrentTarget.AttackerCount--;
            }
            
            // Assign new target
            combatant.CurrentTarget = newTarget;
            
            if (newTarget != null)
            {
                newTarget.AttackerCount++;
                OnTargetAcquired?.Invoke(combatant, newTarget);
            }
        }

        /// <summary>
        /// Clear a combatant's target safely
        /// </summary>
        public static void ClearTarget(ICombatant combatant)
        {
            if (combatant?.CurrentTarget != null)
            {
                combatant.CurrentTarget.AttackerCount--;
                combatant.CurrentTarget = null;
            }
        }

        /// <summary>
        /// Check if two combatants are enemies
        /// </summary>
        public static bool AreEnemies(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return false;
            return a.Team != b.Team && a.Team != CombatTeam.Neutral && b.Team != CombatTeam.Neutral;
        }

        /// <summary>
        /// Check if target is valid (alive, active, and an enemy)
        /// </summary>
        public static bool IsValidTarget(ICombatant attacker, ICombatant target)
        {
            if (target == null) return false;
            if (!target.IsAlive) return false;
            if (!target.IsActive) return false;
            if (!AreEnemies(attacker, target)) return false;
            return true;
        }

        /// <summary>
        /// Check if combatant can attack target (in range and valid)
        /// </summary>
        public static bool CanAttack(ICombatant attacker, ICombatant target, float range = -1)
        {
            if (!IsValidTarget(attacker, target)) return false;
            
            if (range < 0) range = GameConstants.MeleeAttackRange;
            
            float distance = Vector2.Distance(attacker.Center, target.Center);
            return distance <= range;
        }

        /// <summary>
        /// Get distance between two combatants
        /// </summary>
        public static float GetDistance(ICombatant a, ICombatant b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector2.Distance(a.Center, b.Center);
        }

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public static void ClearAllEvents()
        {
            OnDamageDealt = null;
            OnCombatantKilled = null;
            OnTargetAcquired = null;
        }
    }
}
