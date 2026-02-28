using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // This is our 2D camera system. 
    // It tracks a specific point in the game world (usually the player) and generates a mathematical matrix.
    // Other classes like the GameEngine and LevelManager pass this matrix into SpriteBatch.Begin() so the screen draws relative to this camera.
    public class Camera
    {
        // The exact X/Y coordinate in the game world that the camera is currently looking at.
        public Vector2 Position { get; private set; }

        // Controls the scale of the matrix. 1.0f is normal size, 2.0f is zoomed in 200%.
        public float Zoom { get; set; } = 1.0f;

        // The final calculated matrix that gets handed over to the graphics card to offset all the sprites.
        public Matrix CurrentCameraTranslation { get; private set; }

        // We store the total size of the current level so we know where the edges of the world are.
        private Vector2 _mapSize;

        public Camera(Vector2 startPos, Vector2 mapSize)
        {
            Position = startPos;
            _mapSize = mapSize;
            Zoom = 1.0f;
        }

        // Instantly snaps the camera to a new target position. 
        // We need the Viewport passed in so the camera knows how wide the physical game window is for its boundary math.
        public void LookAt(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;

            // Make sure snapping to this new position didn't accidentally push the camera lens out of bounds.
            ClampPosition(viewport);

            // Rebuild the math matrix based on the new, clamped position.
            UpdateMatrix(viewport);
        }

        // This functions identically to LookAt, but is named to imply it should be called every single frame inside an Update loop.
        public void follow(Vector2 targetPos, Viewport viewport)
        {
            Position = targetPos;
            ClampPosition(viewport);
            UpdateMatrix(viewport);
        }

        // This prevents the camera from panning past the edges of the map and showing the black void outside the level.
        private void ClampPosition(Viewport viewport)
        {
            // Calculate exactly how much of the game world the player can currently see, factoring in the zoom level.
            float visibleWidth = viewport.Width / Zoom;
            float visibleHeight = viewport.Height / Zoom;

            float halfWidth = visibleWidth / 2f;
            float halfHeight = visibleHeight / 2f;

            float newX = Position.X;
            float newY = Position.Y;

            // If the player is zoomed out so far that the screen is wider than the entire map, just lock the camera dead center.
            if (visibleWidth > _mapSize.X)
            {
                newX = _mapSize.X / 2f;
            }
            // Otherwise, restrict the X position so the left/right edges of the lens never cross the 0 or Max boundaries.
            else
            {
                newX = MathHelper.Clamp(Position.X, halfWidth, _mapSize.X - halfWidth);
            }

            // Do the exact same boundary checks for the Y axis (top and bottom edges).
            if (visibleHeight > _mapSize.Y)
            {
                newY = _mapSize.Y / 2f;
            }
            else
            {
                newY = MathHelper.Clamp(Position.Y, halfHeight, _mapSize.Y - halfHeight);
            }

            // Apply the safely restricted coordinates back to the camera.
            Position = new Vector2(newX, newY);
        }



        // This is the core engine math that converts our position and zoom into a format the graphics card understands.
        private void UpdateMatrix(Viewport viewport)
        {
            Vector2 screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);

            // Matrix multiplication order matters! 
            // 1. We shift the entire world negatively by the camera's position to center our target on the screen's top-left origin (0,0).
            // 2. We apply the zoom scale.
            // 3. We shift everything positively by exactly half the screen resolution to push the target from the top-left corner into the dead center of the monitor.
            CurrentCameraTranslation =
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        }
    }
}