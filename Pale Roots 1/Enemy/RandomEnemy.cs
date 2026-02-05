using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    /// <summary>
    /// Enemy that wanders randomly across the screen.
    /// Uses CombatSystem.RandomInt instead of creating new Random() each call.
    /// </summary>
    public class RandomEnemy : Enemy
    {
        private Vector2 _randomTarget;

        public RandomEnemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            _randomTarget = CreateRandomTarget();
            
            // Start wandering instead of charging
            CurrentAIState = AISTATE.Wandering;
        }

        /// <summary>
        /// Create a random target position within screen bounds
        /// </summary>
        private Vector2 CreateRandomTarget()
        {
            // Use the shared random from CombatSystem
            int rx = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Width - spriteWidth);
            int ry = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Height - spriteHeight);
            return new Vector2(rx, ry);
        }

        /// <summary>
        /// Override wander to use screen-wide random targets
        /// </summary>
        protected override void PerformWander(List<WorldObject> obstacles)
        {
            // Move toward target
            MoveToward(_randomTarget, Velocity, obstacles);

            // Pick new target when we arrive
            if (Vector2.Distance(position, _randomTarget) < 5f)
            {
                int rx = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Width - spriteWidth);
                int ry = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Height - spriteHeight);
                _randomTarget = new Vector2(rx, ry);
            }
        }

        /// <summary>
        /// Override charge to also wander (this enemy doesn't charge)
        /// </summary>
        protected override void PerformCharge(List<WorldObject> obstacles)
        {
            PerformWander(obstacles);
        }
    }
}
