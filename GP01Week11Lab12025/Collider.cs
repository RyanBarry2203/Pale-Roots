using GP01Week11Lab12025;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiler
{
     
    public class Collider
    {
        
        public int tileX;
        public int tileY;
        public Texture2D texture;
        public bool Visible = false;
        // Add TileType property for easier exit identification
        public TileType CollisionType;

        public Vector2 WorldPosition
        {
            get
            {
                return new Vector2(tileX * texture.Width,tileY * texture.Height);
            }

        }

        public Rectangle CollisionField
        {
            get
            {
                return new Rectangle(WorldPosition.ToPoint(),new Point(texture.Width,texture.Height));
            }

        }

        public Collider(Texture2D tx, int tlx, int tly, TileType type 
            )
            // Add TileType parameter for easier exit identification
        {
            texture = tx;
            tileX = tlx;
            tileY = tly;
            CollisionType = type;
        }

        public void Draw(SpriteBatch sp)
        {
            if (Visible)
                sp.Draw(texture, CollisionField, Color.White);
        }
    }
}
