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


            Scale = 3.0f;
        }


        public Rectangle CollisionBox
        {
            get
            {
                float scale = (float)Scale;


                int finalWidth = (int)(_pixelWidth * scale * 0.8f);

                int finalHeight = (int)(spriteHeight * scale * 0.2f);


                float leftEdge = position.X - (spriteWidth * scale / 2);
                int x = (int)(leftEdge + (_pixelOffsetX * scale) + (finalWidth * 0.1f));

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


            int minX = src.Width;
            int maxX = 0;
            bool foundPixels = false;

            for (int y = startY; y < endY; y++)
            {
                for (int x = src.X; x < src.X + src.Width; x++)
                {
                    // Get exact pixel index
                    int index = y * spriteImage.Width + x;

                    // Check Alpha (Is it not transparent?)
                    if (rawData[index].A > 200)
                    {
                        // Convert global X to local X (0 to Width)
                        int localX = x - src.X;

                        if (localX < minX) minX = localX;
                        if (localX > maxX) maxX = localX;
                        foundPixels = true;
                    }
                }
            }

            // 5. Save the results
            if (foundPixels)
            {
                _pixelOffsetX = minX;
                _pixelWidth = maxX - minX;
            }
            else
            {
                // Fallback: If the bottom is empty, just use the center 50%
                _pixelOffsetX = (int)(src.Width * 0.25f);
                _pixelWidth = (int)(src.Width * 0.5f);
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