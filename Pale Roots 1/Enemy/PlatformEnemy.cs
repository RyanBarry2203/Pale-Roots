using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Patrol enemy that walks between two points.
    // Inherits AI, movement and rendering from Enemy -> RotatingSprite -> Sprite.
    public class PlatformEnemy : Enemy
    {
        // Patrol endpoints and the active target we're moving toward.
        private Vector2 _pointA;
        private Vector2 _pointB;
        private Vector2 _currentPatrolTarget;
        
        // How quickly position interpolates toward the patrol target (0-1).
        public float PatrolLerpSpeed { get; set; } = 0.05f;

        // Constructor receives two positions and uses the base Enemy constructor for setup.
        // Level/Factory code constructs this the same way as other enemies.
        public PlatformEnemy(Game g, Texture2D texture, Vector2 position1, Vector2 position2, int framecount)
            : base(g, texture, position1, framecount)
        {
            _pointA = position1;
            _pointB = position2;
            _currentPatrolTarget = _pointB;
            
            // Start in the wandering AI state so UpdateAI will call PerformWander.
            CurrentAIState = AISTATE.Wandering;
        }

        // Patrol logic runs while wandering.
        // - Uses Vector2.Lerp to smoothly move between points.
        // - Swaps target when close enough to an endpoint.
        // - Ignores obstacles here (no MoveToward); if you need obstacle avoidance, switch to MoveToward.
        protected override void PerformWander(List<WorldObject> obstacles)
        {
            // Smoothly move toward the current patrol target.
            position = Vector2.Lerp(position, _currentPatrolTarget, PatrolLerpSpeed);
            
            // When we reach one endpoint, set the other as the next target.
            if (Vector2.Distance(position, _pointB) < 1)
            {
                _currentPatrolTarget = _pointA;
            }
            else if (Vector2.Distance(position, _pointA) < 1)
            {
                _currentPatrolTarget = _pointB;
            }
        }

        // Use the same patrol behavior while in Charging state so the enemy doesn't lunge.
        protected override void PerformCharge(List<WorldObject> obstacles)
        {
            PerformWander(obstacles);
        }
        
        // After combat finishes, return to patrol.
        // Calls base combat behavior first (handles facing/attacks), then ensures we resume wandering if target is gone.
        protected override void PerformCombat(GameTime gameTime)
        {
            base.PerformCombat(gameTime);
            
            // If we lost our target, go back to patrolling.
            if (CurrentTarget == null || !CurrentTarget.IsAlive)
            {
                CurrentAIState = AISTATE.Wandering;
            }
        }
    }
}
