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
    // Simple on-screen health bar helper.
    // - Not tied to Enemy/Player directly; stores its own health value and draws a colored rectangle.
    // - Uses a 1x1 white Texture2D to render the bar (scaled to width = health).
    public class Healthbar
    {
        // Current health value (also used as bar width in pixels in this simple implementation).
        public int health;

        // 1x1 texture used to draw the colored bar. Created in the constructor.
        private Texture2D TxHealthBar;

        // Backing rectangle (not used directly by drawing code because the property builds it on the fly).
        Rectangle healthRect;

        // Screen-space position where the bar is drawn (top-left).
        public Vector2 position;

        // Property that constructs a Rectangle sized to `health` and fixed height.
        // - Getter creates a Rectangle from `position` and `health`.
        // - Setter preserves API compatibility but is not used elsewhere in the codebase.
        public Rectangle 
            HealthRect { 
            get => new Rectangle((int)position.X, (int)position.Y, health, 10);
            set => healthRect = value; }

        // Constructor:
        // - Startposition: where to draw the bar on screen.
        // - healthValue: initial health (also determines bar width).
        // - Game g: used to create the Texture2D via the GraphicsDevice.
        public Healthbar(Vector2 Startposition, int healthValue, Game g)
        {
            health = healthValue;
            position = Startposition;

            // Create a shared 1x1 white pixel texture used to draw colored rectangles.
            TxHealthBar = new Texture2D(g.GraphicsDevice, 1, 1);
            TxHealthBar.SetData(new[] { Color.White });
        }

        // Simple update for testing: reduce health if Down key is held.
        // In the real game you would call TakeDamage on an ICombatant and update this value accordingly.
        public void Update()
        {
            if (health > 0)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) health--;
            }
        }

        // Draw the health bar using SpriteBatch:
        // - Green when high, Orange at medium, Red when low.
        // - Width is the numeric `health` value (so scale your health range accordingly).
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
