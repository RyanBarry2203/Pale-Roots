using System;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // RotatingSprite: adds rotation helpers and obstacle-aware movement on top of Sprite.
    // - Used by Enemy, Sentry, Projectile and Ally to aim and move while respecting collisions.
    public class RotatingSprite : Sprite
    {
        // How quickly the sprite can turn toward a target (radians per update).
        public float rotationSpeed;

        public RotatingSprite(Game g, Microsoft.Xna.Framework.Graphics.Texture2D tx, Vector2 StartPosition, int NoOfFrames)
            : base(g, tx, StartPosition, NoOfFrames, 1)
        {
        }

        // Rotate smoothly to face another sprite instance (uses TurnToFace under the hood).
        public void follow(Sprite sp)
        {
            this.angleOfRotation = TurnToFace(position, sp.position, angleOfRotation, rotationSpeed);
        }

        // Compute a bounded rotation toward a point.
        // - Returns a new angle clamped by turnSpeed so rotation is smooth over multiple frames.
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

        // Normalize an angle into [-PI, PI] so shortest rotation direction is used.
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

        // Move toward a target while checking collisions against obstacles.
        // - Movement resolves X and Y separately so the sprite can slide along obstacles.
        // - Updates angleOfRotation to face travel direction.
        public void MoveToward(Vector2 target, float speed, System.Collections.Generic.List<WorldObject> obstacles)
        {
            Vector2 direction = target - position;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Vector2 velocity = direction * speed;

                // Try X axis move; apply only if not colliding at the new X.
                Vector2 futurePosX = new Vector2(position.X + velocity.X, position.Y);
                if (!IsColliding(futurePosX, obstacles))
                {
                    position.X = futurePosX.X;
                }

                // Try Y axis move; apply only if not colliding at the new Y.
                Vector2 futurePosY = new Vector2(position.X, position.Y + velocity.Y);
                if (!IsColliding(futurePosY, obstacles))
                {
                    position.Y = futurePosY.Y;
                }

                // Face movement direction immediately.
                angleOfRotation = (float)Math.Atan2(direction.Y, direction.X);

                // Keep sprite inside map bounds (implemented on Sprite).
                ClampToMap();
            }
        }

        // Instantly set rotation to face a world point (no smoothing).
        public void SnapToFace(Vector2 target)
        {
            float x = target.X - position.X;
            float y = target.Y - position.Y;
            angleOfRotation = (float)Math.Atan2(y, x);
        }

        // Smooth follow wrapper that accepts a position rather than a Sprite.
        public void Follow(Vector2 targetPosition)
        {
            this.angleOfRotation = TurnToFace(position, targetPosition, angleOfRotation, rotationSpeed);
        }
    }
}
