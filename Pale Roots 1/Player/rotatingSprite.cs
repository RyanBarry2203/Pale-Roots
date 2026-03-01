using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // Extends Sprite with rotation and navigation helpers used by enemies, allies, and projectiles.
    public class RotatingSprite : Sprite
    {
        // Controls how fast the sprite turns each frame.
        public float rotationSpeed;

        public RotatingSprite(Game g, Microsoft.Xna.Framework.Graphics.Texture2D tx, Vector2 StartPosition, int NoOfFrames)
            : base(g, tx, StartPosition, NoOfFrames, 1)
        {
        }

        // Rotate smoothly to face another sprite.
        public void follow(Sprite sp)
        {
            this.angleOfRotation = TurnToFace(position, sp.position, angleOfRotation, rotationSpeed);
        }

        // Compute an angle that turns toward a target over multiple frames and clamp the change.
        protected static float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed)
        {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // Compute desired angle to the target.
            float desiredAngle = (float)Math.Atan2(y, x);

            // Find the smallest angular difference and limit how much we rotate this frame.
            float difference = WrapAngle(desiredAngle - currentAngle);
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // Return the new wrapped angle.
            return WrapAngle(currentAngle + difference);
        }

        // Keep angles within the -PI to PI range.
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi) radians += MathHelper.TwoPi;
            while (radians > MathHelper.Pi) radians -= MathHelper.TwoPi;
            return radians;
        }

        // Move toward a world target while avoiding solid WorldObjects and allow sliding along obstacles.
        public void MoveToward(Vector2 target, float speed, System.Collections.Generic.List<WorldObject> obstacles)
        {
            Vector2 direction = target - position;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Vector2 velocity = direction * speed;

                // Check X and Y independently so the sprite can slide along blocking objects.

                // Try moving along the X axis if not colliding.
                Vector2 futurePosX = new Vector2(position.X + velocity.X, position.Y);
                if (!IsColliding(futurePosX, obstacles))
                {
                    position.X = futurePosX.X;
                }

                // Try moving along the Y axis if not colliding.
                Vector2 futurePosY = new Vector2(position.X, position.Y + velocity.Y);
                if (!IsColliding(futurePosY, obstacles))
                {
                    position.Y = futurePosY.Y;
                }

                // Face the movement direction.
                angleOfRotation = (float)Math.Atan2(direction.Y, direction.X);

                // Prevent moving outside the world bounds defined in Sprite.
                ClampToMap();
            }
        }

        // Instantly set rotation to look at a target point.
        public void SnapToFace(Vector2 target)
        {
            float x = target.X - position.X;
            float y = target.Y - position.Y;
            angleOfRotation = (float)Math.Atan2(y, x);
        }

        // Smoothly rotate to face a specific world coordinate.
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