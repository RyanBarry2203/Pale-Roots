//using GP01Week11Lab12025;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Simple tile-aligned collider used by the level to mark solid/interactive tiles.
    // - LevelManager creates these for tiles that should block movement or be visible for debugging.
    // - Other systems read CollisionField to test collisions against actors (Player, Enemy).
    public class Collider
    {
        // Tile grid coordinates (column/row) this collider sits on.
        public int tileX;
        public int tileY;

        // The texture used when drawing the collider (often a 1x1 white texture or a debug sprite).
        public Texture2D texture;

        // Toggle drawing for debug/visualization (LevelManager or editor can turn this on).
        public bool Visible = false;

        // WorldPosition: top-left world coordinate (pixels) calculated from tile indices and the texture size.
        // Consumers use this when placing or snapping objects to the same tile grid.
        public Vector2 WorldPosition
        {
            get
            {
                return new Vector2(tileX * texture.Width, tileY * texture.Height);
            }
        }

        // CollisionField: rectangle in world space used for intersection tests with sprites.
        // Use this when checking overlaps with actor bounding boxes.
        public Rectangle CollisionField
        {
            get
            {
                return new Rectangle(WorldPosition.ToPoint(), new Point(texture.Width, texture.Height));
            }
        }

        // Constructor: assign texture and tile coordinates.
        // Typical caller: LevelManager when parsing a tilemap and creating colliders for blocked tiles.
        public Collider(Texture2D tx, int tlx, int tly)
        {
            texture = tx;
            tileX = tlx;
            tileY = tly;
        }

        // Draw the collider rectangle (only when Visible is true).
        // Useful for debugging collision maps; SpriteBatch is provided by the calling draw loop.
        public void Draw(SpriteBatch sp)
        {
            if (Visible)
                sp.Draw(texture, CollisionField, Color.White);
        }
    }
}
