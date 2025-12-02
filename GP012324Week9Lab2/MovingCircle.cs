using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Utilities;

namespace GP012324Week9Lab2
{
    public class MovingCircle : DrawableGameComponent
    {
        Texture2D movingCircle;
        Vector2 movingCirclePosition;
        Vector2 Target;
        float speed;

        // Sound played when the circle is clicked
        SoundEffect captureSound;

        public MovingCircle(Game game) : base(game)
        {
            game.Components.Add(this);
        }

        protected override void LoadContent()
        {
            movingCircle = Game.Content.Load<Texture2D>("circle");
            // load capture sound effect (ensure an asset named "capture" exists in Content)
            captureSound = Game.Content.Load<SoundEffect>("capture");

            speed = Utility.NextRandomFloat(0.03f); // Added 'f' for float precision safety

            // Start in the center
            movingCirclePosition = Game.GraphicsDevice.Viewport.Bounds.Center.ToVector2();

            // Set the initial target using the new safe logic
            PickNewTarget();
        }

        public override void Update(GameTime gameTime)
        {
            MouseState m = Mouse.GetState();

            // Click detection logic
            if (m.LeftButton == ButtonState.Pressed)
            {
                Rectangle r = new Rectangle(
                    (int)movingCirclePosition.X,
                    (int)movingCirclePosition.Y,
                    movingCircle.Width,
                    movingCircle.Height);

                // If the mouse is over the visible circle, play sound and deactivate component
                if (r.Contains(m.Position) && Visible && Enabled)
                {
                    // Play the capture sound
                    captureSound?.Play();

                    // Deactivate the circle component so it stops updating and drawing
                    Enabled = false;
                    Visible = false;
                }
            }

            // Movement Logic (only run while component is enabled)
            if (Enabled)
            {
                movingCirclePosition = Vector2.Lerp(movingCirclePosition, Target, speed);

                // Check if we reached the target
                if (Vector2.Distance(movingCirclePosition, Target) < speed)
                {
                    movingCirclePosition = Target;

                    // Pick a new safe target and random speed
                    PickNewTarget();
                    speed = Utility.NextRandomFloat(0.4f);
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible) return;

            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();

            spriteBatch.Begin();
            spriteBatch.Draw(movingCircle, movingCirclePosition, Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);
        }
        private void PickNewTarget()
        {
            // 1. Get the current screen width and height
            int screenWidth = Game.GraphicsDevice.Viewport.Width;
            int screenHeight = Game.GraphicsDevice.Viewport.Height;

            // 2. Calculate the maximum allowed X and Y
            // We subtract the texture width/height so the circle doesn't hang off the right/bottom edge
            int maxX = screenWidth - movingCircle.Width;
            int maxY = screenHeight - movingCircle.Height;

            // 3. Ensure we don't crash if texture is larger than screen (sanity check)
            if (maxX < 0) maxX = 0;
            if (maxY < 0) maxY = 0;

            // 4. Generate random position within safe bounds
            Target = new Vector2(Utility.NextRandom(0, maxX), Utility.NextRandom(0, maxY));
        }
    }
}