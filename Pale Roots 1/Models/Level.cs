using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // Represents a game level including its tile map, tile palette, and player start location.
    public class Level
    {
        // 2D array of integers representing tile indices that TileLayer will render.
        public int[,] MapLayout { get; set; }

        // List of TileRef entries that map tile indices to source rectangles on the tilesheet.
        public List<TileRef> TilePalette { get; set; }

        // World position where the player should spawn when this level is loaded.
        public Vector2 PlayerStartPos { get; set; }

        // Simple enumeration used by level logic and editors to label tile categories.
        public enum TileType
        {
            Floor= 0,
            Wall = 1,
            Tree = 2,

        }

        // Store the provided map data so other systems like LevelManager and TileLayer can use it.
        public Level(int[,] map, List<TileRef> tiles, Vector2 startPos)
        {
            MapLayout = map;
            TilePalette = tiles;
            PlayerStartPos = startPos;
        }
    }
}