using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    public class Level
    {
        public int[,] MapLayout { get; set; }
        public List<TileRef> TilePalette { get; set; }
        public Vector2 PlayerStartPos { get; set; }

        public enum TileType
        {
            Floor= 0,
            Wall = 1,
            Tree = 2,

        }

        public Level(int[,] map, List<TileRef> tiles, Vector2 startPos)
        {
            MapLayout = map;
            TilePalette = tiles;
            PlayerStartPos = startPos;
        }
    }
}