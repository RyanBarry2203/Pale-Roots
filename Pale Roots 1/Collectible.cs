using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using AnimatedSprite; // Needed to see the Sprite base class

namespace Pale_Roots_1
{
    public class Collectible : Sprite
    {
        public int HealthValue { get; private set; }

        // Static to share memory across all collectibles
        private static Texture2D healthBarTexture;

        public Collectible(Game game, Texture2D texture, Vector2 position, int frameCount)
            : base(game, texture, position, frameCount)
        {
            Random r = new Random();
            HealthValue = r.Next(50, 101);

            // Lazy loading the texture once
            if (healthBarTexture == null)
            {
                healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                healthBarTexture.SetData(new[] { Color.White });
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // Draw health bar above it
            Rectangle barRect = new Rectangle(
                (int)position.X,
                (int)position.Y - 10,
                HealthValue / 2,
                5
            );

            spriteBatch.Draw(healthBarTexture, barRect, Color.Green);

            // Ensure you have a font named "NameID" in your Content pipeline, 
            // otherwise this line will crash.
            // spriteBatch.DrawString(
            //    game.Content.Load<SpriteFont>("NameID"),
            //    HealthValue.ToString(),
            //    new Vector2(position.X, position.Y - 25),
            //    Color.White
            // );
        }
    }
}