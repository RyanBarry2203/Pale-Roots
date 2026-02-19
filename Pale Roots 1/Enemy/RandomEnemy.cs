using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // RandomEnemy: picks screen-wide random targets and wanders between them.
    // - Uses the shared RNG via CombatSystem.RandomInt (avoids creating new Random instances).
    // - Uses MoveToward from RotatingSprite/Enemy so it respects obstacles passed from level code.
    public class RandomEnemy : Enemy
    {
        // Current random target position on screen
        private Vector2 _randomTarget;

        // Constructor uses the base Enemy constructor for setup and starts in Wandering state.
        public RandomEnemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            _randomTarget = CreateRandomTarget();
            
            // This enemy wanders rather than charging at spawn.
            CurrentAIState = AISTATE.Wandering;
        }

        // Pick a random point inside the viewport (uses CombatSystem's helper RNG).
        private Vector2 CreateRandomTarget()
        {
            int rx = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Width - spriteWidth);
            int ry = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Height - spriteHeight);
            return new Vector2(rx, ry);
        }

        // Wander behavior: walk toward the random target and choose a new one on arrival.
        protected override void PerformWander(List<WorldObject> obstacles)
        {
            // MoveToward handles collision checking with obstacles.
            MoveToward(_randomTarget, Velocity, obstacles);

            // When close enough, pick a new random target inside the viewport.
            if (Vector2.Distance(position, _randomTarget) < 5f)
            {
                int rx = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Width - spriteWidth);
                int ry = CombatSystem.RandomInt(0, game.GraphicsDevice.Viewport.Height - spriteHeight);
                _randomTarget = new Vector2(rx, ry);
            }
        }

        // This enemy doesn't charge; use the wander behavior instead.
        protected override void PerformCharge(List<WorldObject> obstacles)
        {
            PerformWander(obstacles);
        }
    }
}
