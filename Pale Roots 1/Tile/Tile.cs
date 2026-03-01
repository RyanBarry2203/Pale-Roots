using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pale_Roots_1
{
    // Represents a single map tile and metadata used by the level system.
    // LevelManager creates Tile instances and uses their size and grid position to place and render tiles.
    // Pathfinding and collision code read the Passable flag to decide walkability.
    public class Tile
    {
        // Reference to the tilesheet location and original map value.
        public TileRef tileRef { get; set; }

        // Pixel dimensions for this tile instance.
        int _tileWidth;
        int _tileHeight;

        // Optional identifier for editors or serialization.
        int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        // Human readable name for debugging or tools.
        string _tileName;
        public string TileName
        {
            get { return _tileName; }
            set { _tileName = value; }
        }

        // True when actors can move through this tile.
        bool _passable;
        public bool Passable
        {
            get { return _passable; }
            set { _passable = value; }
        }

        // Grid column index set by the level loader.
        int _x;
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        // Grid row index set by the level loader.
        int _y;
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        // Width in pixels for converting grid coords to world position.
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
        // Height in pixels for converting grid coords to world position.
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
