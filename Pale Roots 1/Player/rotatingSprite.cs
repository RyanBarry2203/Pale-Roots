using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // This class extends the basic Sprite by adding "intelligence" for rotation and navigation.
    // It is the base for anything that needs to aim, turn, or move around the world (Enemies, Allies, Projectiles).
    public class RotatingSprite : Sprite
    {
        // Controls the 'steering' weight. A high value means instant turns, 
        // while a low value creates a heavy, sweeping rotation.
        public float rotationSpeed;

        public RotatingSprite(Game g, Microsoft.Xna.Framework.Graphics.Texture2D tx, Vector2 StartPosition, int NoOfFrames)
            : base(g, tx, StartPosition, NoOfFrames, 1)
        {
        }

        // --- SMOOTH ROTATION ---

        // Commands the sprite to slowly turn its "face" toward another sprite over several frames.
        public void follow(Sprite sp)
        {
            this.angleOfRotation = TurnToFace(position, sp.position, angleOfRotation, rotationSpeed);
        }

        // The core math for smooth turning. It calculates the desired angle using Atan2, 
        // finds the shortest path to get there, and clamps the change by the rotationSpeed.
        protected static float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed)
        {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // Atan2 converts the X/Y distance into a radian angle.
            float desiredAngle = (float)Math.Atan2(y, x);

            // We calculate the difference and "Wrap" it to ensure the sprite doesn't spin 
            // 350 degrees just to turn 10 degrees to the left.
            float difference = WrapAngle(desiredAngle - currentAngle);
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            return WrapAngle(currentAngle + difference);
        }

        // Helper to keep radians within the -PI to PI range.
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi) radians += MathHelper.TwoPi;
            while (radians > MathHelper.Pi) radians -= MathHelper.TwoPi;
            return radians;
        }

        // --- SLIDING COLLISION NAVIGATION ---

        // Moves the sprite toward a world coordinate while checking for solid WorldObjects.
        public void MoveToward(Vector2 target, float speed, System.Collections.Generic.List<WorldObject> obstacles)
        {
            Vector2 direction = target - position;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Vector2 velocity = direction * speed;

                // --- AXIS SEPARATION ---
                // We check X and Y independently. This allows the sprite to "slide" along walls.
                // If the path forward is blocked diagonally, the sprite can still move horizontally.

                // 1. Try moving along the X axis.
                Vector2 futurePosX = new Vector2(position.X + velocity.X, position.Y);
                if (!IsColliding(futurePosX, obstacles))
                {
                    position.X = futurePosX.X;
                }

                // 2. Try moving along the Y axis.
                Vector2 futurePosY = new Vector2(position.X, position.Y + velocity.Y);
                if (!IsColliding(futurePosY, obstacles))
                {
                    position.Y = futurePosY.Y;
                }

                // Update the rotation so the sprite always looks where it is walking.
                angleOfRotation = (float)Math.Atan2(direction.Y, direction.X);

                // Inherited from Sprite: prevent the sprite from walking off the world map.
                ClampToMap();
            }
        }

        // Instantly snaps the rotation to a point with no transition time.
        public void SnapToFace(Vector2 target)
        {
            float x = target.X - position.X;
            float y = target.Y - position.Y;
            angleOfRotation = (float)Math.Atan2(y, x);
        }

        // A wrapper that lets the AI target a raw Vector2 coordinate instead of a full Sprite object.
        public void Follow(Vector2 targetPosition)
        {
            this.angleOfRotation = TurnToFace(position, targetPosition, angleOfRotation, rotationSpeed);
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}