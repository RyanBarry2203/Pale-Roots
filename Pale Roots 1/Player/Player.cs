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
            Scale = 90.0f / spriteHeight;
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

            // 2. Normalize
            if (inputDirection != Vector2.Zero)
                inputDirection.Normalize();

            // 3. Calculate Proposed Position
            Vector2 proposedPosition = position + (inputDirection * _speed);

            // 4. Collision Detection - 4 CORNER CHECK
            if (currentLayer != null)
            {
                // Calculate the ACTUAL visual size of the wizard
                float visualW = spriteWidth * Scale;
                float visualH = spriteHeight * Scale;

                // PHYSICAL SIZE (HITBOX): Shrink it to fit the body!
                // We multiply by 0.4f (40%) to shave off the empty sides
                float hitboxW = visualW * 0.4f;
                // We multiply by 0.7f (70%) to let the head overlap walls slightly (creates depth)
                float hitboxH = visualH * 0.7f;

                // Calculate the 4 corners of the player's bounding box relative to the center
                // We use a small buffer (-4) so you don't get stuck exactly on the line
                // Calculate corners based on the thinner HITBOX, not the full image
                Vector2 topLeft = new Vector2(proposedPosition.X - hitboxW / 2, proposedPosition.Y - hitboxH / 2);
                Vector2 topRight = new Vector2(proposedPosition.X + hitboxW / 2, proposedPosition.Y - hitboxH / 2);
                Vector2 bottomLeft = new Vector2(proposedPosition.X - hitboxW / 2, proposedPosition.Y + hitboxH / 2);
                Vector2 bottomRight = new Vector2(proposedPosition.X + hitboxW / 2, proposedPosition.Y + hitboxH / 2);

                // Check if ALL 4 corners are safe to walk on
                if (IsWalkable(topLeft, currentLayer) &&
                    IsWalkable(topRight, currentLayer) &&
                    IsWalkable(bottomLeft, currentLayer) &&
                    IsWalkable(bottomRight, currentLayer))
                {
                    // Only move if all corners are safe
                    position = proposedPosition;
                }
            }
            else
            {
                position = proposedPosition;
            }

            // 5. Animate
            if (inputDirection != Vector2.Zero)
                base.Update(gameTime);
        }

        // Add this helper function inside your Player class
        private bool IsWalkable(Vector2 pixelPos, TileLayer layer)
        {
            int tx = (int)(pixelPos.X / 64);
            int ty = (int)(pixelPos.Y / 64);

            // Check if out of bounds (treat as wall)
            if (tx < 0 || tx >= layer.Tiles.GetLength(1) || ty < 0 || ty >= layer.Tiles.GetLength(0))
                return false;

            // Check if the tile is passable
            return layer.Tiles[ty, tx].Passable;
        }
    }
}