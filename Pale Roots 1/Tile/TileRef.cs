namespace Pale_Roots_1
{
    // A lightweight data holder that links a number in your Level Map
    // to a specific coordinate on your master Sprite Sheet.
    public class TileRef
    {
        // The Column index on the tilesheet.
        // If your sheet has 10 tiles across, this would be a number from 0 to 9.
        public int _sheetPosX;

        // The Row index on the tilesheet.
        public int _sheetPosY;

        // The specific number used in your int[,] MapLayout array.
        // For example, if '0' represents Grass, this TileRef will store 0.
        public int _tileMapValue;

        // Simple constructor:
        // x: The column on the sheet (not pixels!)
        // y: The row on the sheet (not pixels!)
        // val: The ID used in the level data array.
        public TileRef(int x, int y, int val)
        {
            _sheetPosX = x;
            _sheetPosY = y;
            _tileMapValue = val;
        }
    }
}