using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class Player : Sprite
    {
        private float _speed = 4.0f;
        private Vector2 _velocity;

        public Vector2 CentrePos
        {
            get { return position + new Vector2(spriteWidth / 2, spriteHeight / 2); }
        }

        public Player(Game game, Texture2D texture, Vector2 startPosition, int frameCount)
            : base(game, texture, startPosition, frameCount)
        {
        }

        // NEW: We accept 'TileLayer' so the player can check for walls
        public void Update(GameTime gameTime, TileLayer currentLayer)
        {
            // 1. Get Input
            Vector2 inputDirection = Vector2.Zero;
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W)) inputDirection.Y -= 1;
            if (state.IsKeyDown(Keys.S)) inputDirection.Y += 1;
            if (state.IsKeyDown(Keys.A)) inputDirection.X -= 1;
            if (state.IsKeyDown(Keys.D)) inputDirection.X += 1;

            // 2. Normalize (Stardew Movement Fix)
            if (inputDirection != Vector2.Zero)
                inputDirection.Normalize();

            // 3. Calculate Proposed Position
            Vector2 proposedPosition = position + (inputDirection * _speed);

            // 4. Collision Detection (Moved from Engine)
            if (currentLayer != null)
            {
                // Calculate which tile we are stepping on
                int tileX = (int)(proposedPosition.X + spriteWidth / 2) / 64;
                int tileY = (int)(proposedPosition.Y + spriteHeight / 2) / 64;

                // Check Map Bounds
                if (tileX >= 0 && tileX < currentLayer.Tiles.GetLength(1) &&
                    tileY >= 0 && tileY < currentLayer.Tiles.GetLength(0))
                {
                    // Check if it's a Wall (Passable = false)
                    if (currentLayer.Tiles[tileY, tileX].Passable)
                    {
                        // It's safe to walk!
                        position = proposedPosition;
                    }
                }
            }
            else
            {
                // If there is no map, just move freely
                position = proposedPosition;
            }

            // 5. Animate
            if (inputDirection != Vector2.Zero)
                base.Update(gameTime);
        }
    }
}