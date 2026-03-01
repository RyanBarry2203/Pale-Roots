using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// Simple collider for a single tile used to mark solid or interactive tiles.
// The LevelManager creates these for tiles that block movement.
// Other systems use CollisionField to test overlaps with actors like Player or Enemy.

namespace Pale_Roots_1
{
    // Represents a collider positioned on a tile grid.
    public class Collider
    {
        // Tile grid coordinates where this collider is located.
        public int tileX;
        public int tileY;

        // Texture used when drawing the collider for debugging or visualization.
        public Texture2D texture;

        // Toggle rendering of the collider for debugging.
        public bool Visible = false;

        // Top-left world coordinate (pixels) derived from the tile indices and texture size.
        public Vector2 WorldPosition
        {
            get
            {
                return new Vector2(tileX * texture.Width, tileY * texture.Height);
            }
        }

        // Rectangle in world space used for intersection tests with sprites.
        public Rectangle CollisionField
        {
            get
            {
                return new Rectangle(WorldPosition.ToPoint(), new Point(texture.Width, texture.Height));
            }
        }

        // Constructor binds the texture and tile coordinates.
        public Collider(Texture2D tx, int tlx, int tly)
        {
            texture = tx;
            tileX = tlx;
            tileY = tly;
        }

        // Draw the collider rectangle when Visible is true.
        // SpriteBatch is provided by the caller's draw loop.
        public void Draw(SpriteBatch sp)
        {
            if (Visible)
                sp.Draw(texture, CollisionField, Color.White);
        }
    }
}
