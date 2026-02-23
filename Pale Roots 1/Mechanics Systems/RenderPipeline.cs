using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class RenderPipeline
    {
        // Custom API method to handle depth-sorted rendering
        public void DrawDepthSorted(SpriteBatch spriteBatch, List<Sprite> renderables)
        {
            // Painter's algorithm ordering by sprite bottom Y for simple depth illusion
            renderables.Sort((a, b) =>
            {
                float aY = a.position.Y + (a.spriteHeight * (float)a.Scale);
                float bY = b.position.Y + (b.spriteHeight * (float)b.Scale);
                return aY.CompareTo(bY);
            });

            // Draw everything in the newly sorted order
            foreach (var sprite in renderables)
            {
                sprite.Draw(spriteBatch);
            }
        }
    }
}