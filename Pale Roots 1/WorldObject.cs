using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using SharpDX.Direct2D1;
using System;

namespace Pale_Roots_1
{
    public class WorldObject : Sprite
    {
        public bool IsSolid { get; set; }
        private static Texture2D _debugTexture;

        public float BoxWidthPercentage { get; set; } = 0.5f; 
        public float BoxHeightPercentage { get; set; } = 0.2f;

        private int _pixelOffsetX;
        private int _pixelWidth;

        public WorldObject(Game g, Texture2D texture, Vector2 pos, int frameCount, bool isSolid)
            : base(g, texture, pos, frameCount, 1.0)
        {
            IsSolid = isSolid;
            mililsecondsBetweenFrames = 200; // Slow animation for trees

            // DEFAULT SCALE:
            // If the sprite feels too small, change this to 2.0f. 
            // In your screenshot, they were massive, so let's stick to 1.5f for now.
            Scale = 3.0f;
        }

        // TIGHTER HITBOX
        // This calculates the collision box at the FEET of the object only.
        public Rectangle CollisionBox
        {
            get
            {
                float scale = (float)Scale;

                int finalWidth = (int)(_pixelWidth * scale);

                int finalHeight = (int)(spriteHeight * scale * 0.2f);

                // Start at Center.
                // Move Left by half the total sprite width (to get to left edge).
                // Move Right by the pixel offset.
                int x = (int)(position.X - (spriteWidth * scale / 2) + (_pixelOffsetX * scale));

                //Calculate Y Position (Bottom aligned)
                int y = (int)(position.Y + (spriteHeight * scale / 2) - finalHeight);

                return new Rectangle(x, y, finalWidth, finalHeight);
            }
        }
        private void CalculatePixelTightBox()
        {

            Color[] rawData = new Color[spriteImage.Width * spriteImage.Height];
            spriteImage.GetData(rawData);

            Rectangle src = sourceRectangle;

            int startY = src.Y + (int)(src.Height * 0.8f);
            int endY = src.Y + src.Height;

            int minX = src.Width; // Start high
            int maxX = 0;         // Start low
            bool foundPixels = false;


            for (int y = startY; y < endY; y++)
            {
                for (int x = src.X; x < src.X + src.Width; x++)
                {
                    // Calculate index in the 1D array
                    int index = y * spriteImage.Width + x;

                    // Check Alpha (Transparency). If it's not see-through...
                    if (rawData[index].A > 20)
                    {
                        int localX = x - src.X; // X relative to the frame (0 to 64)

                        if (localX < minX) minX = localX;
                        if (localX > maxX) maxX = localX;
                        foundPixels = true;
                    }
                }
            }

            // 5. Store the results
            if (foundPixels)
            {
                _pixelOffsetX = minX;
                _pixelWidth = maxX - minX;
            }
            else
            {
                // Fallback if the image is empty for some reason
                _pixelOffsetX = 0;
                _pixelWidth = src.Width;
            }
        }
        public new void SetSpritesheetLocation(Rectangle source)
        {
            base.SetSpriteSheetLocation(source);
            CalculatePixelTightBox();
        }
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            if (_debugTexture == null)
            {
                _debugTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                _debugTexture.SetData(new[] { Color.Red });
            }

            Rectangle box = CollisionBox;

            int thickness = 2;

            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Y, box.Width, thickness), Color.Red);
            // Bottom
            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Bottom, box.Width, thickness), Color.Red);
            // Left
            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Y, thickness, box.Height), Color.Red);
            // Right
            spriteBatch.Draw(_debugTexture, new Rectangle(box.Right, box.Y, thickness, box.Height), Color.Red);
        }
    }
}