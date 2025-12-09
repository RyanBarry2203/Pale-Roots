using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class Player : Sprite
    {
        private float _speed = 4.0f; // Constant walk speed
        private Vector2 _velocity;   // Direction * Speed

        // Helper to get the center for the Camera to follow
        public Vector2 CentrePos
        {
            get { return position + new Vector2(spriteWidth / 2, spriteHeight / 2); }
        }

        public Player(Game game, Texture2D texture, Vector2 startPosition, int frameCount)
            : base(game, texture, startPosition, frameCount)
        {
            // You can add player-specific setup here later (e.g. Health = 100)
        }

        public override void Update(GameTime gameTime)
        {
            // 1. Get Input
            // We use a separate Vector2 for input so we can normalize it.
            Vector2 inputDirection = Vector2.Zero;
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W)) inputDirection.Y -= 1;
            if (state.IsKeyDown(Keys.S)) inputDirection.Y += 1;
            if (state.IsKeyDown(Keys.A)) inputDirection.X -= 1;
            if (state.IsKeyDown(Keys.D)) inputDirection.X += 1;

            // 2. Normalize (The Stardew Fix)
            // If the length is > 0, we scale it to exactly 1.
            // This ensures Diagonal movement (length ~1.4) is slowed down to 1.0.
            if (inputDirection != Vector2.Zero)
            {
                inputDirection.Normalize();
            }

            // 3. Apply Velocity
            // Velocity = Direction * Speed
            _velocity = inputDirection * _speed;
            position += _velocity;

            // 4. Handle Animation
            // Only play the walking animation if we are actually moving.
            if (inputDirection != Vector2.Zero)
            {
                base.Update(gameTime); // Advances the animation frames
            }
            else
            {
                // Optional: Reset to specific frame when standing still
                // currentFrame = 0; 
            }
        }
    }
}