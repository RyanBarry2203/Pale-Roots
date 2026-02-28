//using GP01Week11Lab12025;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // This is a simple mathematical box used to create invisible walls in our tilemap.
    // The LevelManager generates these across the map so that moving entities (like the Player or Enemies) 
    // can check their own bounding boxes against these to prevent walking through solid objects.
    public class Collider
    {
        // The specific column and row in our map grid where this invisible wall sits.
        public int tileX;
        public int tileY;

        // We hold onto a texture here strictly for debugging purposes, so we can visually draw 
        // the invisible walls to screen if something is breaking.
        public Texture2D texture;

        // A debug toggle. The LevelManager can flip this to true to reveal the collision layout.
        public bool Visible = false;

        // This dynamically translates our abstract grid logic (like tile 5, 2) into actual 
        // 2D pixel coordinates on the screen based on how large the assigned texture is.
        public Vector2 WorldPosition
        {
            get
            {
                return new Vector2(tileX * texture.Width, tileY * texture.Height);
            }
        }

        // This builds the actual physical rectangle that MonoGame requires to run intersection math.
        // Other classes ask for this specific property when they want to see if they bumped into a wall.
        public Rectangle CollisionField
        {
            get
            {
                return new Rectangle(WorldPosition.ToPoint(), new Point(texture.Width, texture.Height));
            }
        }

        // When the LevelManager is parsing the map array, it feeds the texture and grid position in here 
        // to stamp out a new solid block.
        public Collider(Texture2D tx, int tlx, int tly)
        {
            texture = tx;
            tileX = tlx;
            tileY = tly;
        }

        // We only bother sending this to the graphics card if we are actively trying to debug our map boundaries.
        public void Draw(SpriteBatch sp)
        {
            if (Visible)
                sp.Draw(texture, CollisionField, Color.White);
        }
    }
}