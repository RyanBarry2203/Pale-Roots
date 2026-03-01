using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    // Represents a static or animated object placed in the world.
    // Provides a tight collision box around the object's feet for blocking and pathing.
    public class WorldObject : Sprite
    {
        public bool IsSolid { get; set; }
        public string AssetName { get; set; }
        private static Texture2D _debugTexture;

        // Used when falling back to a simple bounding box calculation.
        public float BoxWidthPercentage { get; set; } = 0.5f;
        public float BoxHeightPercentage { get; set; } = 0.2f;

        private int _pixelOffsetX;
        private int _pixelWidth;

        // Create a map object; frameCount allows animated variants.
        public WorldObject(Game g, Texture2D texture, Vector2 pos, int frameCount, bool isSolid)
            : base(g, texture, pos, frameCount, 1.0)
        {
            IsSolid = isSolid;
            mililsecondsBetweenFrames = 200;
            Scale = 3.0f;
        }

        // Collision box focused on the feet area to allow better passability.
        public Rectangle CollisionBox
        {
            get
            {
                float scale = (float)Scale;

                int finalHeight = (int)(spriteHeight * scale * 0.2f);
                int finalWidth = (int)(_pixelWidth * scale);

                float leftEdge = position.X - (spriteWidth * scale / 2);
                int x = (int)(leftEdge + (_pixelOffsetX * scale));

                int y = (int)(position.Y + (spriteHeight * scale / 2) - finalHeight);

                return new Rectangle(x, y, finalWidth, finalHeight);
            }
        }

        // Scan the sprite's bottom pixels to compute a tight horizontal footprint.
        // Results are cached in _pixelOffsetX and _pixelWidth for later CollisionBox calculations.
        private void CalculatePixelTightBox()
        {
            Color[] rawData = new Color[spriteImage.Width * spriteImage.Height];
            spriteImage.GetData(rawData);

            Rectangle src = sourceRectangle;

            int startY = src.Y + (int)(src.Height * 0.8f);
            int endY = src.Y + src.Height;

            int minX = src.Width;
            int maxX = 0;
            bool foundPixels = false;

            for (int y = startY; y < endY; y++)
            {
                for (int x = src.X; x < src.X + src.Width; x++)
                {
                    int index = y * spriteImage.Width + x;
                    if (rawData[index].A > 200)
                    {
                        int localX = x - src.X;
                        if (localX < minX) minX = localX;
                        if (localX > maxX) maxX = localX;
                        foundPixels = true;
                    }
                }
            }

            if (foundPixels)
            {
                _pixelOffsetX = minX;
                _pixelWidth = maxX - minX;
            }
            else
            {
                // Fallback footprint when no opaque pixels are found near the bottom.
                _pixelOffsetX = (int)(src.Width * 0.25f);
                _pixelWidth = (int)(src.Width * 0.5f);
            }
        }

        // When the source rectangle changes, recalculate the tight collision footprint.
        public new void SetSpritesheetLocation(Rectangle source)
        {
            base.SetSpriteSheetLocation(source);
            CalculatePixelTightBox();
        }

        // Draw a red rectangle around the computed CollisionBox for debugging purposes.
        // Lazily creates a 1x1 debug texture when first needed.
        //public void DrawDebug(SpriteBatch spriteBatch)
        //{
        //    if (_debugTexture == null)
        //    {
        //        _debugTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        //        _debugTexture.SetData(new[] { Color.Red });
        //    }

        //    Rectangle box = CollisionBox;
        //    int thickness = 2;

        //    spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Y, box.Width, thickness), Color.Red);
        //    spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Bottom, box.Width, thickness), Color.Red);
        //    spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Y, thickness, box.Height), Color.Red);
        //    spriteBatch.Draw(_debugTexture, new Rectangle(box.Right, box.Y, thickness, box.Height), Color.Red);
        //}
    }
}