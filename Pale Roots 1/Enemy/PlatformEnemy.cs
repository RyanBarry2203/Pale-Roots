using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    /// <summary>
    /// Enemy that patrols between two points.
    /// Good for guarding areas or creating predictable patterns.
    /// </summary>
    public class PlatformEnemy : Enemy
    {
        private Vector2 _pointA;
        private Vector2 _pointB;
        private Vector2 _currentPatrolTarget;
        
        /// <summary>Speed of interpolation (0-1, higher = faster)</summary>
        public float PatrolLerpSpeed { get; set; } = 0.05f;

        public PlatformEnemy(Game g, Texture2D texture, Vector2 position1, Vector2 position2, int framecount)
            : base(g, texture, position1, framecount)
        {
            _pointA = position1;
            _pointB = position2;
            _currentPatrolTarget = _pointB;
            
            // Start in wandering state (patrol mode)
            CurrentAIState = AISTATE.Wandering;
        }

        /// <summary>
        /// Override wander to patrol between points
        /// </summary>
        protected override void PerformWander(List<WorldObject> obstacles)
        {
            // Lerp toward current target
            position = Vector2.Lerp(position, _currentPatrolTarget, PatrolLerpSpeed);
            
            // Swap targets when we reach one
            if (Vector2.Distance(position, _pointB) < 1)
            {
                _currentPatrolTarget = _pointA;
            }
            else if (Vector2.Distance(position, _pointA) < 1)
            {
                _currentPatrolTarget = _pointB;
            }
        }

        /// <summary>
        /// Override charge to patrol instead
        /// </summary>
        protected override void PerformCharge(List<WorldObject> obstacles)
        {
            PerformWander(obstacles);
        }
        
        /// <summary>
        /// After combat, return to patrol
        /// </summary>
        protected override void PerformCombat(GameTime gameTime)
        {
            base.PerformCombat(gameTime);
            
            // If target is lost, go back to patrolling
            if (CurrentTarget == null || !CurrentTarget.IsAlive)
            {
                CurrentAIState = AISTATE.Wandering;
            }
        }
    }
}
