using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Simple camera that centers on a point, clamps to map bounds and produces a transform matrix
    // Consumers (LevelManager / Game1 / drawing code) should use CurrentCameraTranslation as the SpriteBatch transform.
    public class Camera
    {
        // World-space position the camera is centered on (in world coordinates).
        public Vector2 Position { get; private set; }

        // Zoom factor (1.0 = 100%). Affects how much of the map is visible.
        public float Zoom { get; set; } = 1.0f;

        // Matrix to pass into SpriteBatch.Begin(transformMatrix: CurrentCameraTranslation)
        // Updated whenever Position or Zoom changes via LookAt/follow.
        public Matrix CurrentCameraTranslation { get; private set; }

        // Map size in world units; used to clamp camera so we don't show outside the level.
        private Vector2 _mapSize;

        // startPos: initial camera center. mapSize: full world extents (width, height).
        public Camera(Vector2 startPos, Vector2 mapSize)
        {
            Position = startPos;
            _mapSize = mapSize;
            Zoom = 1.0f;
        }

        // Move camera immediately to targetPos and update the transform.
        // viewport is required so we can compute how much world the screen shows at current Zoom.
        public void LookAt(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;

            // Keep camera inside the map edges based on current viewport and Zoom.
            ClampPosition(viewport);

            // Build the matrix used for rendering transforms right away.
            UpdateMatrix(viewport);
        }

        // Smooth-follow or immediate follow API (same here as LookAt).
        // Call each frame with the player's world position and Viewport before drawing.
        public void follow(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;
            ClampPosition(viewport);
            UpdateMatrix(viewport);
        }

        // Ensure the camera center stays inside the level bounds.
        // Uses visible world size = viewport / Zoom so clamping adjusts when Zoom changes.
        private void ClampPosition(Viewport viewport)
        {
            float visibleWidth = viewport.Width / Zoom;
            float visibleHeight = viewport.Height / Zoom;

            float halfWidth = visibleWidth / 2f;
            float halfHeight = visibleHeight / 2f;

            float newX = Position.X;
            float newY = Position.Y;

            // If the visible area is larger than the map, center on the map instead of clamping edges.
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
        // 1) translate world so the camera center is at origin,
        // 2) scale (zoom), then
        // 3) translate so origin maps to the screen center.
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