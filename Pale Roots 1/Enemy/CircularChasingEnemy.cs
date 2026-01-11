using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    /// <summary>
    /// Enemy that only chases targets within a detection radius.
    /// Returns to start position if target leaves the zone.
    /// 
    /// Good for: Territorial enemies, guards, ambush predators
    /// </summary>
    public class CircularChasingEnemy : Enemy
    {
        /// <summary>Radius within which this enemy will detect and chase targets</summary>
        public float ChaseRadius { get; set; }
        
        /// <summary>If true, enemy has detected a target and is in full pursuit</summary>
        private bool _isAggro = false;

        public CircularChasingEnemy(Game g, Texture2D texture, Vector2 position1, int framecount)
            : base(g, texture, position1, framecount)
        {
            ChaseRadius = GameConstants.DefaultChaseRadius;
            Velocity = 2.0f; // Slightly slower than default
        }

        protected override void UpdateAI(GameTime gameTime)
        {
            // Check if current target is still in range
            if (CurrentTarget != null)
            {
                float distanceToTarget = CombatSystem.GetDistance(this, CurrentTarget);
                
                // If target leaves chase radius, disengage
                if (distanceToTarget > ChaseRadius * 1.5f) // Give some buffer before disengaging
                {
                    _isAggro = false;
                    CombatSystem.ClearTarget(this);
                    CurrentAIState = AISTATE.Wandering;
                }
            }
            
            base.UpdateAI(gameTime);
        }

        /// <summary>
        /// Override wander to return to start position when not aggro
        /// </summary>
        protected override void PerformWander()
        {
            // Return to start position
            if (Vector2.Distance(position, startPosition) > 5f)
            {
                MoveToward(startPosition, Velocity * 0.5f);
            }
            // else just idle at start position
        }

        /// <summary>
        /// Check if a target is within chase zone
        /// </summary>
        public bool IsInChaseZone(ICombatant target)
        {
            if (target == null) return false;
            return CombatSystem.GetDistance(this, target) <= ChaseRadius;
        }

        /// <summary>
        /// Trigger aggro on this enemy
        /// </summary>
        public void Aggro(ICombatant target)
        {
            if (target == null || !CombatSystem.IsValidTarget(this, target)) return;
            
            _isAggro = true;
            CombatSystem.AssignTarget(this, target);
            CurrentAIState = AISTATE.Chasing;
        }
    }
}
