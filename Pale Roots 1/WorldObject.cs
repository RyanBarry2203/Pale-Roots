using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Pale_Roots_1
{
    public class WorldObject : Sprite
    {
        public bool IsSolid { get; set; }

        public Rectangle CollisionBox
        {
            get
            {
                int width = (int)(spriteWidth * Scale * 0.6f);
                int height = (int)(spriteHeight * Scale * 0.3f);

                int x  = (int)Center.X - (width / 2);
                int y  = (int)(position.Y + (spriteHeight * Scale) - height);

                return new Rectangle(x, y, width, height);
            }
        }
        public WorldObject(Game g, Texture2D texture, Vector2 pos, int frameCount, bool isSolid)
            : base(g, texture, pos, frameCount, 1.0)
        {
            this.IsSolid = isSolid;

            int millisecondsBetweenFrames = 150;
        }
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }
    }
}
