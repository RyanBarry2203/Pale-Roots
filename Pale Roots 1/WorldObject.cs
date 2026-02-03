using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class WorldObject : Sprite
    {
        public bool IsSolid { get; set; }

        public WorldObject(Game g, Texture2D texture, Vector2 pos, int frameCount, bool isSolid)
            : base(g, texture, pos, frameCount, 1.0)
        {
            IsSolid = isSolid;
            mililsecondsBetweenFrames = 200; // Slow animation for trees

            // DEFAULT SCALE:
            // If the sprite feels too small, change this to 2.0f. 
            // In your screenshot, they were massive, so let's stick to 1.5f for now.
            Scale = 2f;
        }

        // TIGHTER HITBOX
        // This calculates the collision box at the FEET of the object only.
        public Rectangle CollisionBox
        {
            get
            {
                int width = (int)(spriteWidth * Scale * 0.5f); // 50% width
                int height = (int)(spriteHeight * Scale * 0.2f); // 20% height (just the base)

                // Center the box at the bottom
                int x = (int)Center.X - (width / 2);
                int y = (int)(position.Y + (spriteHeight * Scale) - height);

                return new Rectangle(x, y, width, height);
            }
        }
    }
}