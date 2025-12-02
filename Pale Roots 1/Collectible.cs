using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Sprites
{
    public class Collectible : Sprite
    {
        public int HealthValue { get; private set; }
        private Texture2D healthBarTexture;
        private Texture2D CollectableTx;

        public Collectible(Game game, Texture2D texture, Vector2 position, int frameCount)
            : base(game, texture, position, frameCount)
        {
            // Generate random health 50–100
            Random r = new Random();
            HealthValue = r.Next(50, 101);

            // Create a plain 1x1 white texture for health bar drawing
            healthBarTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            healthBarTexture.SetData(new[] { Color.White });
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the collectible sprite (frame animation handled by base class)
            base.Draw(spriteBatch);

            // --- Draw health bar above it ---
            Rectangle barRect = new Rectangle(
                (int)position.X,
                (int)position.Y - 10,
                HealthValue / 2,   // Scale value into ~25–50px width
                5
            );

            spriteBatch.Draw(healthBarTexture, barRect, Color.Green);

            
            spriteBatch.DrawString(
                game.Content.Load<SpriteFont>("NameID"),
                HealthValue.ToString(),
                new Vector2(position.X, position.Y - 25),
                Color.White
            );
        }
    }
}

