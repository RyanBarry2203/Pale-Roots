using Microsoft.Xna.Framework;
using System;

namespace Pale_Roots_1
{
    public static class PhysicsGlobals
    {
        // Adapted from VelcroPhysics GravityController logic
        public static Vector2 CalculateGravitationalForce(Vector2 targetPos, Vector2 objectPos, float strength, float maxRadius)
        {
            Vector2 d = targetPos - objectPos;
            float r2 = d.LengthSquared();

            // Ignore if outside radius or too close (prevent divide by zero)
            if (r2 > maxRadius * maxRadius || r2 < 0.001f)
                return Vector2.Zero;

            float r = (float)Math.Sqrt(r2);

            // F = Strength / r^2
            float forceMagnitude = strength / r2;

            // Normalize direction * magnitude
            return (d / r) * forceMagnitude;
        }
    }
}