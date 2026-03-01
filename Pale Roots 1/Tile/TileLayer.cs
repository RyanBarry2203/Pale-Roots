using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pale_Roots_1
{
    // Represents a grid of tiles and renders them from a shared tilesheet.
    public class TileLayer
    {
        // Source tile size in the tilesheet in pixels.
        int SourceTileSize = 16;
        // Destination tile size in world pixels.
        int DestTileSize = 64;

        // Palette mapping tile indices to positions on the tilesheet.
        List<TileRef> tileRefs = new List<TileRef>();

        // Grid dimensions and storage for Tile objects.
        int tileMapHeight;
        int tileMapWidth;
        Tile[,] _tiles;
        public Tile[,] Tiles
        {
            get { return _tiles; }
            set { _tiles = value; }
        }

        // Construct a tile layer from a numeric map and optional TileRef palette.
        public TileLayer(int[,] LayerMap,List<TileRef> MapSheetReferences, int destSize, int sourceSize)
        {
            DestTileSize = destSize;
            SourceTileSize = sourceSize;

            tileRefs = MapSheetReferences;
            tileMapHeight = LayerMap.GetLength(0);
            tileMapWidth = LayerMap.GetLength(1);
            Tiles = new Tile[tileMapHeight, tileMapWidth];

            // Fill the Tiles array with Tile objects the rest of the engine uses.
            for (int x = 0; x < tileMapWidth; x++)
                for (int y = 0; y < tileMapHeight; y++)
                {
                    // Choose a tile index from the palette for visual variety,
                    // or use the numeric map value when no palette was provided.
                    int chosenIndex = 0;
                    if (tileRefs != null && tileRefs.Count > 0)
                    {
                        chosenIndex = CombatSystem.RandomInt(0, tileRefs.Count);
                    }
                    else
                    {
                        chosenIndex = LayerMap[y, x];
                    }

                    // Create a Tile with grid coordinates, id, passability and a reference to the tilesheet cell.
                    Tiles[y, x] =
                        new Tile
                        {
                            X = x,
                            Y = y,
                            Id = chosenIndex,
                            Passable = true,
                            tileRef = (tileRefs != null && tileRefs.Count > 0) ? tileRefs[chosenIndex] : tileRefs[LayerMap[y, x]]
                        };
                }
        }

        // Draw every tile using the shared Helper.SpriteSheet texture.
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var Tile in Tiles)
            {
                // Destination rectangle in world coordinates.
                Rectangle destRect = new Rectangle(
                    Tile.X * DestTileSize,
                    Tile.Y * DestTileSize,
                    DestTileSize,
                    DestTileSize);

                // Source rectangle inside the tilesheet.
                Rectangle sourceRect = new Rectangle(
                    Tile.tileRef._sheetPosX * SourceTileSize,
                    Tile.tileRef._sheetPosY * SourceTileSize,
                    SourceTileSize,
                    SourceTileSize);

                // Draw the tile. Caller should supply the sprite batch transform matrix.
                spriteBatch.Draw(Helper.SpriteSheet, destRect, sourceRect, Color.White);
            }
        }
    }
}
