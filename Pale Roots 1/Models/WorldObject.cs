using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    // WorldObject is a Sprite used for static and animated map objects (trees, rocks, ruins).
    // Responsibilities:
    // - Maintain collision footprint that tightly fits visible non-transparent pixels near the sprite's feet.
    // - Provide a CollisionBox used by other actors for obstacle detection.
    // - Render itself and optionally a debug collision box.
    // Interactions:
    // - CollisionBox is used by RotatingSprite.IsColliding and Player/Enemy movement logic to block movement.
    // - Uses Helper.GetSourceRect to map asset keys to sheet locations when created by LevelManager.
    public class WorldObject : Sprite
    {
        public bool IsSolid { get; set; }
        public string AssetName { get; set; }
        private static Texture2D _debugTexture;

        // Percentages used when defaulting to simpler bounding box calculations.
        public float BoxWidthPercentage { get; set; } = 0.5f;
        public float BoxHeightPercentage { get; set; } = 0.2f;

        private int _pixelOffsetX;
        private int _pixelWidth;

        // Construct a map object; frameCount allows animated variants.
        public WorldObject(Game g, Texture2D texture, Vector2 pos, int frameCount, bool isSolid)
            : base(g, texture, pos, frameCount, 1.0)
        {
            IsSolid = isSolid;
            mililsecondsBetweenFrames = 200; // slower default for environmental animations
            Scale = 3.0f; // chosen scale to match tile grid; adjustable by level creation code
        }

        // Collision box focuses on the feet area rather than the whole sprite to allow better depth/passability.
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

        // Analyze the sprite's bottom area to determine a tight pixel footprint for collision.
        // Stores pixel offsets so CollisionBox can be computed quickly later.
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
                // fallback if bottom has no opaque pixels
                _pixelOffsetX = (int)(src.Width * 0.25f);
                _pixelWidth = (int)(src.Width * 0.5f);
            }
        }

        // Override to compute tight collision box whenever the source rectangle is set.
        public new void SetSpritesheetLocation(Rectangle source)
        {
            base.SetSpriteSheetLocation(source);
            CalculatePixelTightBox();
        }

        // Draw red rectangle lines for debug; creates a shared 1x1 texture as needed.
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
            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Bottom, box.Width, thickness), Color.Red);
            spriteBatch.Draw(_debugTexture, new Rectangle(box.X, box.Y, thickness, box.Height), Color.Red);
            spriteBatch.Draw(_debugTexture, new Rectangle(box.Right, box.Y, thickness, box.Height), Color.Red);
        }
    }
}