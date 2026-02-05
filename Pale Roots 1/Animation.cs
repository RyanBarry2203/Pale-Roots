using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Animation
    {
        public Texture2D Texture { get; private set; }
        public int FrameCount { get; private set; }
        public int SheetRow { get; private set; }
        public float FrameSpeed { get; private set; }
        public bool IsLooping { get; private set; }
        public int TotalRows { get; private set; }

        // NEW: Store the exact width of a single frame
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }

        public Animation(Texture2D texture, int frameCount, int sheetRow, float frameSpeed, bool isLooping, int totalRows = 1, int customWidth = 0)
        {
            Texture = texture;
            FrameCount = frameCount;
            SheetRow = sheetRow;
            FrameSpeed = frameSpeed;
            IsLooping = isLooping;
            TotalRows = totalRows;

            // THE LOGIC:
            // If we pass a custom width (like 125), use it.
            // If we pass 0, calculate it automatically (Texture / Count).
            if (customWidth > 0)
            {
                // Manual Override for messy sheets
                FrameWidth = customWidth;
            }
            else
            {
                // Automatic Calculation for clean sheets
                FrameWidth = texture.Width / frameCount;
            }

            FrameHeight = texture.Height / totalRows;
        }
    }
}
