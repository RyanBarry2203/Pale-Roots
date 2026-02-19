using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // `CircularChasingEnemy` chases targets inside a circular radius and returns to its start when the target leaves.
    // It builds on `Enemy` and uses `CombatSystem`, `MoveToward`, and AI state fields from the base class.
    public class CircularChasingEnemy : Enemy
    {
        // How far this enemy will detect and begin chasing a target.
        public float ChaseRadius { get; set; }
        
        // Tracks whether the enemy is currently pursuing a target.
        private bool _isAggro = false;

        // Constructor matches the base `Enemy` signatures so factories/LevelManager can instantiate this like other enemies.
        public CircularChasingEnemy(Game g, Texture2D texture, Vector2 position1, int framecount)
            : base(g, texture, position1, framecount)
        {
            // Default detection radius read from a shared configuration (`GameConstants`).
            ChaseRadius = GameConstants.DefaultChaseRadius;
            // Default movement speed for this enemy type (inherited `Velocity` used by movement/animation).
            Velocity = 2.0f;
        }

        // AI tick called by `Enemy.Update` with obstacle data from the level.
        // - Verifies the current target is still valid and inside an extended chase boundary.
        // - If target escaped, clear target bookkeeping and switch to wandering.
        protected override void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {
            // If we have a target, check how far away it is using the shared `CombatSystem`.
            if (CurrentTarget != null)
            {
                float distanceToTarget = CombatSystem.GetDistance(this, CurrentTarget);
                
                // If the target moved well outside the chase zone (buffered by 1.5x), disengage.
                // `CombatSystem.ClearTarget` updates central target lists; `CurrentAIState` controls behavior next tick.
                if (distanceToTarget > ChaseRadius * 1.5f)
                {
                    _isAggro = false;
                    CombatSystem.ClearTarget(this);
                    CurrentAIState = AISTATE.Wandering;
                }
            }
            
            // Let the base class perform its shared AI work (cooldowns, state dispatch to PerformX methods).
            base.UpdateAI(gameTime, obstacles);
        }

        // Wander behavior overridden so the enemy returns to its `startPosition` when not aggressive.
        protected override void PerformWander(List<WorldObject> obstacles)
        {
            // If we're not close to the start, path back slowly using the inherited `MoveToward` helper.
            if (Vector2.Distance(position, startPosition) > 5f)
            {
                MoveToward(startPosition, Velocity * 0.5f, obstacles);
            }
            // Otherwise remain idle at `startPosition`.
        }

        // Utility to ask whether a potential `ICombatant` is within the configured chase radius.
        public bool IsInChaseZone(ICombatant target)
        {
            if (target == null) return false;
            return CombatSystem.GetDistance(this, target) <= ChaseRadius;
        }

        // External trigger to make this enemy start chasing a valid target.
        // - Validity is checked with `CombatSystem.IsValidTarget`.
        // - Assigns the target centrally and flips AI state to `Chasing`.
        public void Aggro(ICombatant target)
        {
            if (target == null || !CombatSystem.IsValidTarget(this, target)) return;
            
            _isAggro = true;
            CombatSystem.AssignTarget(this, target);
            CurrentAIState = AISTATE.Chasing;
        }
    }
}
