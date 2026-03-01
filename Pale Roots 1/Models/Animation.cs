using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Holds data for a single sprite animation including frame size and timing.
    public class Animation
    {
        // Texture atlas used by this animation.
        public Texture2D Texture { get; private set; }

        // Number of frames in the animation row.
        public int FrameCount { get; private set; }

        // Which row in the texture sheet this animation uses.
        public int SheetRow { get; private set; }

        // Milliseconds per frame.
        public float FrameSpeed { get; private set; }

        // Whether the animation should loop when it reaches the end.
        public bool IsLooping { get; private set; }

        // Number of rows in the texture when using a multi-row sheet.
        public int TotalRows { get; private set; }

        // Exact width of a single frame in pixels.
        public int FrameWidth { get; private set; }

        // Exact height of a single frame in pixels.
        public int FrameHeight { get; private set; }

        // True when the sheet uses directional rows instead of sprite flipping.
        public bool IsGrid { get; set; }

        // texture: the sprite sheet. frameCount: frames across the row.
        // sheetRow: which vertical row to use. frameSpeed: ms per frame.
        // isLooping: loop behavior. totalRows: rows in sheet for grid layouts.
        // customWidth: optional override for frame width. isGrid: directional sheet flag.
        public Animation(Texture2D texture, int frameCount, int sheetRow, float frameSpeed, bool isLooping, int totalRows = 1, int customWidth = 0, bool isGrid = false)
        {
            Texture = texture;
            FrameCount = frameCount;
            SheetRow = sheetRow;
            FrameSpeed = frameSpeed;
            IsLooping = isLooping;
            TotalRows = totalRows;
            IsGrid = isGrid;

            // Use custom width if provided, otherwise divide the texture width by frame count.
            if (customWidth > 0)
                FrameWidth = customWidth;
            else
                FrameWidth = texture.Width / frameCount;

            // Height is computed from the number of rows in the texture.
            FrameHeight = texture.Height / totalRows;
        }
    }
}
