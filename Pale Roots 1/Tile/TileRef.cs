namespace Pale_Roots_1
{
    // Maps a numeric tile value to a tile cell on the tilesheet.
    // TileLayer and Tile use this to compute source rectangles when drawing.
    public class TileRef
    {
        // Column index on the tilesheet in tile units.
        public int _sheetPosX;

        // Row index on the tilesheet in tile units.
        public int _sheetPosY;

        // The integer value stored in the map that this TileRef represents.
        public int _tileMapValue;

        // Initialize the sheet coordinates and the map value.
        public TileRef(int x, int y, int val)
        {
            _sheetPosX = x;
            _sheetPosY = y;
            _tileMapValue = val;
        }
    }
}