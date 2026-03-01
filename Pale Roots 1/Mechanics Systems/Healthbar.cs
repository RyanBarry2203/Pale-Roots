using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pale_Roots_1
{
    // Draws a simple on-screen health bar using a 1x1 white texture.
    // The bar stores its own numeric health value and renders a colored rectangle.
    public class Healthbar
    {
        // Current health value used as the bar width in pixels.
        public int health;

        // 1x1 texture used to draw the colored rectangle.
        private Texture2D TxHealthBar;

        // Backing rectangle kept for API compatibility.
        Rectangle healthRect;

        public Vector2 position;

        // Rectangle sized to `health` and a fixed height.
        public Rectangle 
            HealthRect { 
            get => new Rectangle((int)position.X, (int)position.Y, health, 10);
            set => healthRect = value; }

        // Create the texture and initialize position and health.
        public Healthbar(Vector2 Startposition, int healthValue, Game g)
        {
            health = healthValue;
            position = Startposition;

            // Create a 1x1 white pixel used to draw the bar.
            TxHealthBar = new Texture2D(g.GraphicsDevice, 1, 1);
            TxHealthBar.SetData(new[] { Color.White });
        }

        // Render the health bar with color based on health tiers.
        public void draw(SpriteBatch spriteBatch)
        {
            if (health > 0)
            {
                if (health > 60)
                    spriteBatch.Draw(TxHealthBar, HealthRect, Color.Green);
                else if (health > 30 && health <= 60)
                    spriteBatch.Draw(TxHealthBar, HealthRect, Color.Orange);
                else if (health > 0 && health < 30)
                    spriteBatch.Draw(TxHealthBar, HealthRect, Color.Red);
            }
        }
    }
}
