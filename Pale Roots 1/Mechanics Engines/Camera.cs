using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        public float Zoom { get; set; } = 1.0f;
        public Matrix CurrentCameraTranslation { get; private set; }

        private Vector2 _mapSize;

        public Camera(Vector2 startPos, Vector2 mapSize)
        {
            Position = startPos;
            _mapSize = mapSize;
            Zoom = 1.0f;
        }

        // FIX 1: We must pass Viewport here so we can calculate the Matrix immediately!
        public void LookAt(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;

            // FIX 2: Check bounds immediately
            ClampPosition(viewport);

            // FIX 3: Calculate the Matrix NOW.
            UpdateMatrix(viewport);
        }

        public void follow(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;
            ClampPosition(viewport);
            UpdateMatrix(viewport);
        }

        private void ClampPosition(Viewport viewport)
        {
            float visibleWidth = viewport.Width / Zoom;
            float visibleHeight = viewport.Height / Zoom;

            float halfWidth = visibleWidth / 2f;
            float halfHeight = visibleHeight / 2f;

            // Keep the camera center inside the map
            Position = new Vector2(
                MathHelper.Clamp(Position.X, halfWidth, _mapSize.X - halfWidth),
                MathHelper.Clamp(Position.Y, halfHeight, _mapSize.Y - halfHeight)
            );
        }

        private void UpdateMatrix(Viewport viewport)
        {
            Vector2 screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);

            // Center-Pivot Math: Move World Center to (0,0) -> Zoom -> Move to Screen Center
            CurrentCameraTranslation =
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        }
    }
}