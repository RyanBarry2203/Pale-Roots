using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This class handles the final visual arrangement of the game world before it is drawn to the screen.
    // By keeping this separate from the GameEngine, we can easily swap out rendering rules later if needed.
    public class RenderPipeline
    {
        // Custom API method to handle depth-sorted rendering.
        // It takes the raw, unsorted list of every visible entity gathered by the GameEngine.
        public void DrawDepthSorted(SpriteBatch spriteBatch, List<Sprite> renderables)
        {
            // We use the "Painter's Algorithm" to create a faux-3D depth illusion.
            // Sprites higher up on the screen (lower Y value) are drawn first, and sprites lower on the screen 
            // (higher Y value) are drawn over top of them.
            renderables.Sort((a, b) =>
            {
                // Calculate the exact bottom edge of both sprites by adding their scaled height to their current Y position.
                // We calculate the "feet" of the sprite, rather than the "head" (position.Y), so a player standing 
                // in front of a tree correctly obscures the trunk, but standing behind it gets obscured by the leaves.
                float aY = a.position.Y + (a.spriteHeight * (float)a.Scale);
                float bY = b.position.Y + (b.spriteHeight * (float)b.Scale);

                // Compare the two bottom edges to determine which one needs to be drawn first.
                return aY.CompareTo(bY);
            });

            // Now that the list is perfectly ordered from the "back" of the map to the "front",
            // loop through and actually paint them to the screen.
            foreach (var sprite in renderables)
            {
                sprite.Draw(spriteBatch);
            }
        }
    }
}