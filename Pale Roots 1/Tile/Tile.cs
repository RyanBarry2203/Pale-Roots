using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pale_Roots_1
{
    // The fundamental building block of the game world.
    // Each 'Tile' represents a single square on the level grid.
    public class Tile
    {
        // --- VISUAL REFERENCE ---
        // This links the logical tile to its "clothes"—the specific pixel coordinates 
        // on the master sprite sheet (defined in TileRef).
        public TileRef tileRef { get; set; }

        private int _tileWidth;
        private int _tileHeight;

        // --- IDENTIFICATION ---
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _tileName;
        public string TileName
        {
            get { return _tileName; }
            set { _tileName = value; }
        }

        // --- PHYSICS & LOGIC ---
        // Crucial for Pathfinding and Collision. 
        // If 'Passable' is false, the Player and Enemies will treat this as a solid wall.
        bool _passable;
        public bool Passable
        {
            get { return _passable; }
            set { _passable = value; }
        }

        // --- GRID POSITION ---
        // These are the coordinates in the TileArray (e.g., Column 5, Row 10), 
        // not the raw pixel position on the screen.
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

        // --- DIMENSIONS ---
        // Stored per-tile to allow for non-standard maps or zoom-level adjustments.
        // Usually defaulted to GameConstants.TileSize (64 pixels).
        public int TileWidth
        {
            get { return _tileWidth; }
            set { _tileWidth = value; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
            set { _tileHeight = value; }
        }
    }
}