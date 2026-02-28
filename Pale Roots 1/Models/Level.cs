using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pale_Roots_1
{
    // This class represents a single loaded stage or floor in the game.
    // It acts as a container for the physical layout (the grid) and the artistic style (the palette).
    public class Level
    {
        // A 2D grid of integers where each number corresponds to a TileType.
        // For example, if MapLayout[5, 5] is 1, the engine knows there is a Wall at that coordinate.
        public int[,] MapLayout { get; set; }

        // The list of textures/coordinates from the sprite sheet that this level is allowed to use.
        public List<TileRef> TilePalette { get; set; }

        // The exact X/Y coordinate where the player should be placed when the level loads.
        public Vector2 PlayerStartPos { get; set; }

        // This defines the "Rules" of the grid. 
        // Using an enum ensures that '0' always means Floor and '1' always means Wall across the whole project.
        public enum TileType
        {
            Floor = 0,
            Wall = 1,
            Tree = 2,
        }

        // The constructor bundles the generated map data and the visual assets together into one object.
        public Level(int[,] map, List<TileRef> tiles, Vector2 startPos)
        {
            MapLayout = map;
            TilePalette = tiles;
            PlayerStartPos = startPos;
        }
    }
}