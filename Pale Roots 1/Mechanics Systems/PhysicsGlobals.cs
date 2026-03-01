using Microsoft.Xna.Framework;
using System;

namespace Pale_Roots_1
{
    // Provides physics helpers used by gameplay systems that apply forces.
    public static class PhysicsGlobals
    {
        // Calculate an inverse-square gravitational force from targetPos toward objectPos.
        // Used by abilities that pull or push actors, such as the boss gravity burst.
        public static Vector2 CalculateGravitationalForce(Vector2 targetPos, Vector2 objectPos, float strength, float maxRadius)
        {
            Vector2 d = targetPos - objectPos;
            float r2 = d.LengthSquared();

            // Return zero when outside the effect radius or when distance is too small to avoid division by zero.
            if (r2 > maxRadius * maxRadius || r2 < 0.001f)
                return Vector2.Zero;

            float r = (float)Math.Sqrt(r2);

            // Compute force magnitude using an inverse square relationship.
            float forceMagnitude = strength / r2;

            // Return the direction from object to target normalized and scaled by the magnitude.
            return (d / r) * forceMagnitude;
        }
    }
}