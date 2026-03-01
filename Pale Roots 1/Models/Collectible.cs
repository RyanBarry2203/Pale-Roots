using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // A simple pickup sprite that grants health when collected.
    public class Collectible : Sprite
    {
        public int HealthValue { get; private set; }

        // Shared 1x1 texture used to draw the small health bar for all collectibles.
        private static Texture2D healthBarTexture;

        public Collectible(Game game, Texture2D texture, Vector2 position, int frameCount)
            : base(game, texture, position, frameCount, 1)
        {
            // Randomize the health amount for this collectible.
            HealthValue = Utility.NextRandom(50, 101);

            // Create the shared pixel texture the first time a collectible is constructed.
            if (healthBarTexture == null)
            {
                healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                healthBarTexture.SetData(new[] { Color.White });
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // Draw a small green bar above the collectible indicating its health value.
            Rectangle barRect = new Rectangle(
                (int)position.X,
                (int)position.Y - 10,
                HealthValue / 2,
                5
            );

            spriteBatch.Draw(healthBarTexture, barRect, Color.Green);

        }
    }
}