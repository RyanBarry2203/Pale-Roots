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

        public bool IsGrid { get; set; }

        public Animation(Texture2D texture, int frameCount, int sheetRow, float frameSpeed, bool isLooping, int totalRows = 1, int customWidth = 0, bool isGrid = false)
        {
            Texture = texture;
            FrameCount = frameCount;
            SheetRow = sheetRow;
            FrameSpeed = frameSpeed;
            IsLooping = isLooping;
            TotalRows = totalRows;
            IsGrid = isGrid; // NEW: Stores if this is a directional sheet

            if (customWidth > 0)
                FrameWidth = customWidth;
            else
                FrameWidth = texture.Width / frameCount;

            // If it's a grid, the height is the full texture divided by rows
            FrameHeight = texture.Height / totalRows;
        }
    }
}
