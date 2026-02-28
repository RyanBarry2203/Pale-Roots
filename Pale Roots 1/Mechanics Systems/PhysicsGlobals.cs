using Microsoft.Xna.Framework;
using System;

namespace Pale_Roots_1
{
    // This static helper class holds universal physics calculations that any object in the game can use.
    // By centralizing this math, you ensure that spells, bosses, and environmental hazards all pull objects 
    // using the exact same consistent physics rules.
    public static class PhysicsGlobals
    {
        // Adapted from VelcroPhysics GravityController logic.
        // Calculates a pull (or push, if strength is negative) from the targetPos acting upon the objectPos.
        public static Vector2 CalculateGravitationalForce(Vector2 targetPos, Vector2 objectPos, float strength, float maxRadius)
        {
            // 1. Calculate the raw distance vector between the two points.
            Vector2 d = targetPos - objectPos;

            // 2. Calculate the squared distance (r^2). 
            // We do this first because checking bounds against a squared radius is mathematically faster 
            // than calculating the true distance using a square root right away.
            float r2 = d.LengthSquared();

            // 3. Safety and Boundary Checks.
            // If the object is outside the gravitational area of effect, don't bother calculating the math.
            // Alternatively, if the object is mathematically too close to the center (< 0.001f), 
            // we bail out to prevent a "divide by zero" error which would instantly crash the engine or create infinite velocity.
            if (r2 > maxRadius * maxRadius || r2 < 0.001f)
                return Vector2.Zero;

            // 4. Calculate the true distance (r) now that we know we actually need it.
            float r = (float)Math.Sqrt(r2);

            // 5. Calculate the magnitude of the force using the inverse-square law.
            // The closer the object gets to the target, the exponentially stronger the pull becomes.
            float forceMagnitude = strength / r2;

            // 6. Apply the magnitude to the normalized direction vector.
            // (d / r) gives us a vector with a length of exactly 1 pointing straight at the target.
            // Multiplying that by our force magnitude gives us the final physical force to apply to the object this frame.
            return (d / r) * forceMagnitude;
        }
    }
}