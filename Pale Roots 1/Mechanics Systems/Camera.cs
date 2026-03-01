using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Camera that centers on a world point, clamps inside the level, and produces a SpriteBatch transform.
    public class Camera
    {
        // World-space center position of the camera.
        public Vector2 Position { get; private set; }

        // Zoom level where 1.0 = 100%.
        public float Zoom { get; set; } = 1.0f;

        // Matrix to pass into SpriteBatch.Begin(transformMatrix: CurrentCameraTranslation).
        public Matrix CurrentCameraTranslation { get; private set; }

        // Full map size in world units used to clamp the camera.
        private Vector2 _mapSize;

        // Initialize camera center and map extents.
        public Camera(Vector2 startPos, Vector2 mapSize)
        {
            Position = startPos;
            _mapSize = mapSize;
            Zoom = 1.0f;
        }

        // Immediately move the camera to targetPos and update the transform for drawing.
        public void LookAt(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;

            // Clamp to map bounds based on current zoom and viewport.
            ClampPosition(viewport);

            // Recompute the transform matrix.
            UpdateMatrix(viewport);
        }

        // Follow API; call each frame with the target world position and viewport.
        public void follow(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;
            ClampPosition(viewport);
            UpdateMatrix(viewport);
        }

        // Keep the camera center inside the level using the visible world size derived from zoom.
        private void ClampPosition(Viewport viewport)
        {
            float visibleWidth = viewport.Width / Zoom;
            float visibleHeight = viewport.Height / Zoom;

            float halfWidth = visibleWidth / 2f;
            float halfHeight = visibleHeight / 2f;

            float newX = Position.X;
            float newY = Position.Y;

            // If the viewport shows more than the map, center on the map instead of clamping edges.
            if (visibleWidth > _mapSize.X)
            {
                newX = _mapSize.X / 2f;
            }
            else
            {
                newX = MathHelper.Clamp(Position.X, halfWidth, _mapSize.X - halfWidth);
            }

            if (visibleHeight > _mapSize.Y)
            {
                newY = _mapSize.Y / 2f;
            }
            else
            {
                newY = MathHelper.Clamp(Position.Y, halfHeight, _mapSize.Y - halfHeight);
            }

            Position = new Vector2(newX, newY);
        }

        // Recompute the transform matrix used for rendering:
        // translate world so camera center is origin, scale by Zoom, then translate to screen center.
        private void UpdateMatrix(Viewport viewport)
        {
            Vector2 screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);

            CurrentCameraTranslation =
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        }
    }
}