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
    public class Healthbar
    {
        public int health;
        private Texture2D TxHealthBar; // hold the texture
        Rectangle healthRect; // display the Health bar size
        public Vector2 position; // Position on the screen

        public Rectangle 
            HealthRect { 
            get => new Rectangle((int)position.X, (int)position.Y, health, 10);
            set => healthRect = value; }

        public Healthbar(Vector2 Startposition, int healthValue, Game g)
        {
            health = healthValue;
            position = Startposition;
            TxHealthBar = new Texture2D(g.GraphicsDevice, 1, 1);
            TxHealthBar.SetData(new[] { Color.White });

        }

        public void Update()
        {
            if (health > 0)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) health--;
            }
        }

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
