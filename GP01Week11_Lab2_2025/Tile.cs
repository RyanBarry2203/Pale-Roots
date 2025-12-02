using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP01Week11_Lab2_2025
{
    public class Tile
    {
        public TileRef tileRef { get; set; }
        int _tileWidth;
        int _tileHeight;
        int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        string _tileName;
        bool _passable;
        public bool Passable
        {
            get { return _passable; }
            set { _passable = value; }
        }
        public string TileName
        {
            get { return _tileName; }
            set { _tileName = value; }
        }
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
