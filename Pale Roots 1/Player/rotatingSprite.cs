using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // RotatingSprite extends Sprite with rotation helpers and movement that respects obstacles.
    // Responsibilities:
    // - Provide smooth rotation toward a point (TurnToFace, Follow).
    // - MoveToward attempts X and Y moves separately and checks collisions via Sprite.IsColliding.
    // - Reuse WrapAngle to normalize rotation values.
    // Interactions:
    // - Used by Enemy, Sentry and Projectile to aim/rotate toward targets.
    public class RotatingSprite : Sprite
    {
        public float rotationSpeed;

        public RotatingSprite(Game g, Microsoft.Xna.Framework.Graphics.Texture2D tx, Vector2 StartPosition, int NoOfFrames)
            : base(g, tx, StartPosition, NoOfFrames, 1)
        {
        }

        // Convenience: make this sprite rotate to face another sprite instance.
        public void follow(Sprite sp)
        {
            this.angleOfRotation = TurnToFace(position, sp.position, angleOfRotation, rotationSpeed);
        }

        // Compute a bounded rotation toward a target; clamps rotation change by turnSpeed per call.
        protected static float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed)
        {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;
            float desiredAngle = (float)Math.Atan2(y, x);

            float difference = WrapAngle(desiredAngle - currentAngle);
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            return WrapAngle(currentAngle + difference);
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        // Normalize angle into range [-Pi, Pi]
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        // Move toward a target while checking collisions against the provided obstacle list.
        // Movement resolves X and Y separately to allow sliding along obstacles.
        public void MoveToward(Vector2 target, float speed, System.Collections.Generic.List<WorldObject> obstacles)
        {
            Vector2 direction = target - position;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Vector2 velocity = direction * speed;

                // Attempt X movement
                Vector2 futurePosX = new Vector2(position.X + velocity.X, position.Y);
                if (!IsColliding(futurePosX, obstacles))
                {
                    position.X = futurePosX.X;
                }

                // Attempt Y movement
                Vector2 futurePosY = new Vector2(position.X, position.Y + velocity.Y);
                if (!IsColliding(futurePosY, obstacles))
                {
                    position.Y = futurePosY.Y;
                }

                // Update rotation to face movement direction
                angleOfRotation = (float)Math.Atan2(direction.Y, direction.X);

                ClampToMap();
            }
        }

        // Immediately set rotation to face a point without smoothing
        public void SnapToFace(Vector2 target)
        {
            float x = target.X - position.X;
            float y = target.Y - position.Y;
            angleOfRotation = (float)Math.Atan2(y, x);
        }

        // Smooth follow wrapper using TurnToFace
        public void Follow(Vector2 targetPosition)
        {
            this.angleOfRotation = TurnToFace(position, targetPosition, angleOfRotation, rotationSpeed);
        }
    }
}
