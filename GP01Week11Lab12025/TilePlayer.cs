using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiler
{

    public class TilePlayer
    {
        Texture2D texture;
        Vector2 position;
        int speed;
        Vector2 previousPosition;
        

        public Rectangle CollisionField
        {
            get
            {
                return new Rectangle(position.ToPoint(), 
                    new Point(texture.Width, texture.Height));
            }

        }
        // Inside TilePlayer class
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public TilePlayer(Texture2D tx,
            Vector2 startPos)
        {
            texture = tx;
            position = previousPosition = startPos;
            speed = 5;

        }
        // Change collision to return bool for collision detection
        // in calling code
        public bool Collision(Collider c)
        {
            if (CollisionField.Intersects(c.CollisionField))
            {
                position = previousPosition;
                return true;
            }
            return false;
        }

        public void update(GameTime gameTime)
        {
            previousPosition = position;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                this.position += new Vector2(1, 0) * speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                this.position += new Vector2(-1, 0) * speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                this.position += new Vector2(0, -1) * speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                this.position += new Vector2(0, 1) * speed;
            }

        }

        public void Draw(SpriteBatch sp)
        {
            
                sp.Draw(texture, CollisionField, Color.White);
        }
    }
}
