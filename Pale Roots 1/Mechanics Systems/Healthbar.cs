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
    // A lightweight UI component for drawing floating health bars.
    // Instead of using complex art assets, it mathematically stretches a single pixel across the screen.
    public class Healthbar
    {
        // The current health of the entity. 
        // In this simple implementation, 1 HP = 1 Pixel of width on the screen.
        public int health;

        // The single white pixel we will stretch and colorize.
        private Texture2D TxHealthBar;

        // A backing variable for our property below.
        Rectangle healthRect;

        // The specific X/Y coordinate on the screen where the top-left corner of the bar will sit.
        public Vector2 position;

        // This property dynamically generates a new mathematical box every time it is called.
        // It uses the current X/Y position, sets the width directly to the 'health' value, and hardcodes the height to 10 pixels.
        public Rectangle
            HealthRect
        {
            get => new Rectangle((int)position.X, (int)position.Y, health, 10);
            set => healthRect = value;
        }

        public Healthbar(Vector2 Startposition, int healthValue, Game g)
        {
            health = healthValue;
            position = Startposition;

            // Generate the 1x1 white pixel directly in the graphics memory.
            // Because it is pure white, we can tint it to any color we want during the SpriteBatch.Draw() call.
            TxHealthBar = new Texture2D(g.GraphicsDevice, 1, 1);
            TxHealthBar.SetData(new[] { Color.White });
        }

        public void Update()
        {
            if (health > 0)
            {
                // DEBUG FEATURE: Pressing the Down Arrow key forces the health to drain.
                // In actual gameplay, the CombatSystem handles deducting health, not this UI class.
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) health--;
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
            // Only draw the bar if the entity is actually alive.
            if (health > 0)
            {
                // Draw the stretched pixel using our dynamic HealthRect, changing the color tint 
                // based on how much health is remaining.
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