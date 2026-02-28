using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This class manages a grid of tiles. It handles the translation from
    // the numeric 2D array (the map) to actual pixels on the screen.
    public class TileLayer
    {
        // 16x16 is a standard size for pixel art assets.
        // 64x64 is a standard size for modern 2D game world grids.
        int SourceTileSize = 16;
        int DestTileSize = 64;

        List<TileRef> tileRefs = new List<TileRef>();

        int tileMapHeight;
        int tileMapWidth;
        Tile[,] _tiles;

        public Tile[,] Tiles
        {
            get { return _tiles; }
            set { _tiles = value; }
        }

        public TileLayer(int[,] LayerMap, List<TileRef> MapSheetReferences, int destSize, int sourceSize)
        {
            DestTileSize = destSize;
            SourceTileSize = sourceSize;
            tileRefs = MapSheetReferences;

            tileMapHeight = LayerMap.GetLength(0);
            tileMapWidth = LayerMap.GetLength(1);
            Tiles = new Tile[tileMapHeight, tileMapWidth];

            // Loop through every cell in the grid to initialize the Tile objects.
            for (int x = 0; x < tileMapWidth; x++)
            {
                for (int y = 0; y < tileMapHeight; y++)
                {
                    int chosenIndex = 0;

                    // VARIETY LOGIC: If we have a list of possible textures, pick one at random.
                    // This prevents the world from looking like a repetitive grid.
                    if (tileRefs != null && tileRefs.Count > 0)
                    {
                        chosenIndex = CombatSystem.RandomInt(0, tileRefs.Count);
                    }
                    else
                    {
                        chosenIndex = LayerMap[y, x];
                    }

                    // Store the data. Note: Y is 'Row', X is 'Column'.
                    Tiles[y, x] = new Tile
                    {
                        X = x,
                        Y = y,
                        Id = chosenIndex,
                        Passable = true, // Default to walkable; LevelManager can block tiles later.
                        tileRef = (tileRefs != null && tileRefs.Count > 0) ? tileRefs[chosenIndex] : tileRefs[LayerMap[y, x]]
                    };
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Iterate through every tile we've created.
            foreach (var Tile in Tiles)
            {
                // DESTINATION RECTANGLE: Where on the screen does this tile go?
                // We multiply the grid coordinate (e.g., column 5) by the world size (64px).
                Rectangle destRect = new Rectangle(
                    Tile.X * DestTileSize,
                    Tile.Y * DestTileSize,
                    DestTileSize,
                    DestTileSize);

                // SOURCE RECTANGLE: Which specific part of the 16x16 sprite sheet are we cutting out?
                // We use the sheet coordinates stored in TileRef.
                Rectangle sourceRect = new Rectangle(
                    Tile.tileRef._sheetPosX * SourceTileSize,
                    Tile.tileRef._sheetPosY * SourceTileSize,
                    SourceTileSize,
                    SourceTileSize);

                // Draw using the master sprite sheet from the Helper class.
                spriteBatch.Draw(Helper.SpriteSheet, destRect, sourceRect, Color.White);
            }
        }
    }
}