namespace Pale_Roots_1
{
    // TileRef: small data holder that maps a tilemap value to a source cell on the tilesheet.
    // Used by TileLayer and Tile to compute the source rectangle when drawing tiles.
    public class TileRef
    {
        // Column index on the tilesheet (in tiles, not pixels).
        public int _sheetPosX;

        // Row index on the tilesheet (in tiles, not pixels).
        public int _sheetPosY;

        // The integer value stored in the map that this TileRef represents.
        // Level parsing code and editors use this to match tiles -> TileRef entries.
        public int _tileMapValue;

        // Simple constructor: supply sheet X/Y (tile coords) and the original map value.
        public TileRef(int x, int y, int val)
        {
            _sheetPosX = x;
            _sheetPosY = y;
            _tileMapValue = val;
        }
    }
}