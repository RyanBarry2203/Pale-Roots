using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Represents an item on the ground (like a health potion) that the player can pick up.
    // Inherits from Sprite so it can be depth-sorted and drawn by the RenderPipeline just like characters.
    public class Collectible : Sprite
    {
        public int HealthValue { get; private set; }

        // By making this texture static, ALL collectibles in the game share this single pixel in memory.
        // If you spawn 100 potions, you still only use 1 pixel of VRAM for their health bars!
        private static Texture2D healthBarTexture;

        public Collectible(Game game, Texture2D texture, Vector2 position, int frameCount)
            : base(game, texture, position, frameCount, 1)
        {
            // ENGINE FIX: We swapped out 'new Random()' for your custom Utility class!
            // If the engine spawns 5 collectibles in the exact same frame, they will now correctly have different values.
            HealthValue = Utility.NextRandom(50, 101);

            // Lazy loading: Only the very first Collectible ever created will run this code.
            if (healthBarTexture == null)
            {
                healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                healthBarTexture.SetData(new[] { Color.White });
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the actual sprite graphic first using the base class logic
            base.Draw(spriteBatch);

            // Calculate a small floating bar above the item based on its specific HealthValue
            Rectangle barRect = new Rectangle(
                (int)position.X,
                (int)position.Y - 10,
                HealthValue / 2, // Scale the width down so a 100HP potion isn't 100 pixels wide
                5
            );

            // Draw the stretched white pixel, tinting it green.
            spriteBatch.Draw(healthBarTexture, barRect, Color.Green);

            // --- PERFORMANCE NOTE ---
            // It is very good that this is commented out! Calling 'game.Content.Load' inside a Draw loop 
            // forces the game to try and read from the hard drive 60 times a second, which will instantly kill your framerate.
            // If you ever re-enable text here, pass the SpriteFont into the constructor just like you do the Texture2D!

            // spriteBatch.DrawString(
            //    game.Content.Load<SpriteFont>("NameID"),
            //    HealthValue.ToString(),
            //    new Vector2(position.X, position.Y - 25),
            //    Color.White
            // );
        }
    }
}