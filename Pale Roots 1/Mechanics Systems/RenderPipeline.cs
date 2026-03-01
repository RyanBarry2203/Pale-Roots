using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Provides depth-sorted rendering for sprites.
    public class RenderPipeline
    {
        // Sorts sprites by their bottom Y and then draws them in that order.
        public void DrawDepthSorted(SpriteBatch spriteBatch, List<Sprite> renderables)
        {
            // Compute each sprite's bottom Y and sort so lower sprites are drawn last.
            renderables.Sort((a, b) =>
            {
                float aY = a.position.Y + (a.spriteHeight * (float)a.Scale);
                float bY = b.position.Y + (b.spriteHeight * (float)b.Scale);
                return aY.CompareTo(bY);
            });

            // Draw all sprites in the sorted sequence.
            foreach (var sprite in renderables)
            {
                sprite.Draw(spriteBatch);
            }
        }
    }
}