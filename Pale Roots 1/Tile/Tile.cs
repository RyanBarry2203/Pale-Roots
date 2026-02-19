using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pale_Roots_1
{
    // Tile: simple data holder representing one map tile.
    // - `tileRef` links to a TileRef which stores sheet coordinates and the original tilemap value.
    // - LevelManager constructs Tile instances when loading a map and uses X/Y and TileWidth/TileHeight
    //   to place/render tiles or to build collision/logic structures.
    // - `Passable` is a quick flag used by pathing / collision checks to decide if actors can walk through.
    public class Tile
    {
        // Reference to sheet position and map value (see TileRef definition).
        public TileRef tileRef { get; set; }

        // Pixel size of this tile (usually equals GameConstants.TileSize but stored per-tile for flexibility).
        int _tileWidth;
        int _tileHeight;

        // Optional identifier for the tile (useful for lookups, editor IDs or serialization).
        int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        // Human-friendly name for debug / editor tooling.
        string _tileName;
        public string TileName
        {
            get { return _tileName; }
            set { _tileName = value; }
        }

        // Whether actors can pass through this tile (true = walkable).
        bool _passable;
        public bool Passable
        {
            get { return _passable; }
            set { _passable = value; }
        }

        // Tile grid coordinates (column,row). LevelManager typically sets these when parsing the tilemap.
        int _x;
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        int _y;
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        // Width/Height in pixels for the tile; used when converting grid coords to world positions.
        public int TileWidth
        {
            get
            {
                return _tileWidth;
            }

            set
            {
                _tileWidth = value;
            }
        }
        public int TileHeight
        {
            get
            {
                return _tileHeight;
            }

            set
            {
                _tileHeight = value;
            }
        }
    }
}
