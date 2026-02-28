using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // This class acts as a blueprint or data container for a single animation.
    // It doesn't actually "play" anything; instead, the AnimationManager reads these properties 
    // to know exactly how to slice up the sprite sheet and how fast to flip through the frames.
    public class Animation
    {
        // The raw image file containing the sequence of frames.
        public Texture2D Texture { get; private set; }

        // How many individual frames (columns) make up this specific animation.
        public int FrameCount { get; private set; }

        // Which specific horizontal row on the sprite sheet this animation lives on.
        public int SheetRow { get; private set; }

        // The delay (in milliseconds) before the manager should advance to the next frame.
        public float FrameSpeed { get; private set; }

        // True if the animation should repeat infinitely (like walking), False if it should play once and stop (like dying).
        public bool IsLooping { get; private set; }

        // How many vertical rows exist on the entire texture. Used to calculate the height of a single frame.
        public int TotalRows { get; private set; }

        // The exact pixel dimensions of a single "slice" or frame on the sprite sheet.
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }

        // A flag indicating if this sprite sheet contains multiple facings (Up, Down, Left, Right) 
        // that the manager needs to dynamically swap between.
        public bool IsGrid { get; set; }

        public Animation(Texture2D texture, int frameCount, int sheetRow, float frameSpeed, bool isLooping, int totalRows = 1, int customWidth = 0, bool isGrid = false)
        {
            Texture = texture;
            FrameCount = frameCount;
            SheetRow = sheetRow;
            FrameSpeed = frameSpeed;
            IsLooping = isLooping;
            TotalRows = totalRows;
            IsGrid = isGrid;

            // Calculate the width of a single frame.
            // If a custom width was provided, use that. Otherwise, divide the total width of the image by the number of frames.
            if (customWidth > 0)
                FrameWidth = customWidth;
            else
                FrameWidth = texture.Width / frameCount;

            // Calculate the height of a single frame by dividing the total height of the image by the number of rows.
            FrameHeight = texture.Height / totalRows;
        }
    }
}