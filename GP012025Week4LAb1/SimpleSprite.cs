using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Sprites
{
    public class SimpleSprite
    {
        public Texture2D Image;
        public Vector2 Position;
        public Rectangle BoundingRect;
        public bool Visible = true;

        public SimpleSprite(Texture2D spriteImage,
                            Vector2 startPosition)
        {
            Image = spriteImage;
            Position = startPosition;
            BoundingRect = new Rectangle((int)startPosition.X, (int)startPosition.Y, Image.Width, Image.Height);

        }

        public void draw(SpriteBatch sp)
        {
            if(Visible)
                sp.Draw(Image, Position, Color.White);
        }

        public void Move(Vector2 delta)
        {
            Position += delta;
            BoundingRect = new Rectangle((int)Position.X, (int)Position.Y, Image.Width, Image.Height);
            BoundingRect.X = (int)Position.X;
            BoundingRect.Y = (int)Position.Y;
        }
        public bool Collision(SimpleSprite other)
        {
            return this.BoundingRect.Intersects(other.BoundingRect);
        }
        //add functionality and variable to the simple sprite class so it can write a message above its current position
                public void DrawMessage(SpriteBatch sp, SpriteFont font, string message)
        {
            if (Visible)
            {
                Vector2 messagePosition = new Vector2(Position.X, Position.Y - 20); // Position above the sprite
                //centre text above the sprite
                Vector2 textSize = font.MeasureString(message);
                messagePosition.X += (Image.Width - textSize.X) / 2; // Center the text horizontally

                sp.DrawString(font, message, messagePosition, Color.White);
                
            }

        }
    }
}
