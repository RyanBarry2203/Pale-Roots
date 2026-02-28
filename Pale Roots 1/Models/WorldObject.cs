using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    // WorldObject handles trees, rocks, and ruins. 
    // It creates a 'physical' presence in the world that blocks the Player and Enemies.
    public class WorldObject : Sprite
    {
        public bool IsSolid { get; set; }
        public string AssetName { get; set; }
        private static Texture2D _debugTexture;

        // Fallback percentages if pixel-perfect scanning fails.
        public float BoxWidthPercentage { get; set; } = 0.5f;
        public float BoxHeightPercentage { get; set; } = 0.2f;

        private int _pixelOffsetX;
        private int _pixelWidth;

        public WorldObject(Game g, Texture2D texture, Vector2 pos, int frameCount, bool isSolid)
            : base(g, texture, pos, frameCount, 1.0)
        {
            IsSolid = isSolid;
            mililsecondsBetweenFrames = 200; // Environmental objects usually animate slower than characters.
            Scale = 3.0f;
        }

        // --- THE DYNAMIC COLLISION BOX ---
        // This is calculated on the fly. It centers the width based on the scanned pixels 
        // and places the box at the very bottom of the sprite.
        public Rectangle CollisionBox
        {
            get
            {
                float scale = (float)Scale;

                // We only take the bottom 20% of the sprite's height for the collision box.
                int finalHeight = (int)(spriteHeight * scale * 0.2f);
                int finalWidth = (int)(_pixelWidth * scale);

                // Calculate the left edge relative to the sprite's center origin.
                float leftEdge = position.X - (spriteWidth * scale / 2);
                int x = (int)(leftEdge + (_pixelOffsetX * scale));

                // Position the box at the feet.
                int y = (int)(position.Y + (spriteHeight * scale / 2) - finalHeight);

                return new Rectangle(x, y, finalWidth, finalHeight);
            }
        }

        // --- PIXEL SCANNING ALGORITHM ---
        // This method analyzes the actual colors of the sprite sheet to find where the trunk/base is.
        private void CalculatePixelTightBox()
        {
            // 1. Pull the raw color data from the texture.
            Color[] rawData = new Color[spriteImage.Width * spriteImage.Height];
            spriteImage.GetData(rawData);

            Rectangle src = sourceRectangle;

            // 2. Define the 'scan zone' (the bottom 20% of the sprite).
            int startY = src.Y + (int)(src.Height * 0.8f);
            int endY = src.Y + src.Height;

            int minX = src.Width;
            int maxX = 0;
            bool foundPixels = false;

            // 3. Loop through the scan zone and find the leftmost and rightmost non-transparent pixels.
            for (int y = startY; y < endY; y++)
            {
                for (int x = src.X; x < src.X + src.Width; x++)
                {
                    int index = y * spriteImage.Width + x;

                    // If the pixel is mostly opaque (Alpha > 200)...
                    if (rawData[index].A > 200)
                    {
                        int localX = x - src.X;
                        if (localX < minX) minX = localX;
                        if (localX > maxX) maxX = localX;
                        foundPixels = true;
                    }
                }
            }

            // 4. Store the results so the CollisionBox property can use them instantly.
            if (foundPixels)
            {
                _pixelOffsetX = minX;
                _pixelWidth = maxX - minX;
            }
            else
            {
                // Fallback: If the sprite is empty or transparent at the bottom, use a default 50% width.
                _pixelOffsetX = (int)(src.Width * 0.25f);
                _pixelWidth = (int)(src.Width * 0.5f);
            }
        }

        // We override this to ensure that every time the texture changes, we re-scan the pixels.
        public new void SetSpritesheetLocation(Rectangle source)
        {
            base.SetSpriteSheetLocation(source);
            CalculatePixelTightBox();
        }

        // This draws the red outline around the object's feet in debug mode.
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            if (_debugTexture == null)
            {
                _debugTexture = new Texture2D(game.GraphicsDevice, 1, 1);
                _debugTexture.SetData(new[] { Color.Red });
            }

            Rectangle box = CollisionBox;
            int thickness = 2;

            // Draw the four lines of the rectangle.
            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Y, box.Width, thickness), Color.Red);
            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Bottom, box.Width, thickness), Color.Red);
            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Y, thickness, box.Height), Color.Red);
            spriteBatch.Draw(_debugTexture, new Rectangle(box.Right, box.Y, thickness, box.Height), Color.Red);
        }
    }
}