using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pale_Roots_1
{
    // TileLayer: holds a 2D grid of Tile objects and draws them from a shared spritesheet.
    // - Constructed from a numeric LayerMap (int[,]) and a list of TileRef entries that map tilesheet cells -> world tiles.
    // - LevelManager creates TileLayer instances when loading tilemap data and then calls Draw each frame.
    public class TileLayer
    {
        // Size (pixels) of a single tile in the source spritesheet and the destination (world) tile size.
        int SourceTileSize = 16;
        int DestTileSize = 64;

        // Palette of tile references (sheet X/Y + map value). If empty, LayerMap values are used directly.
        List<TileRef> tileRefs = new List<TileRef>();

        // Dimensions and storage for tile grid
        int tileMapHeight;  // rows (LayerMap.GetLength(0))
        int tileMapWidth;   // columns (LayerMap.GetLength(1))
        Tile[,] _tiles;
        public Tile[,] Tiles
        {
            get { return _tiles; }
            set { _tiles = value; }
        }

        // Constructor:
        // - LayerMap: int[row, col] that describes which tile value belongs at each cell.
        // - MapSheetReferences: list of TileRef entries mapping indices -> sheet positions.
        // - destSize/sourceSize: destination (world) tile size and source spritesheet tile size in pixels.
        public TileLayer(int[,] LayerMap,List<TileRef> MapSheetReferences, int destSize, int sourceSize)
        {
            DestTileSize = destSize;
            SourceTileSize = sourceSize;

            tileRefs = MapSheetReferences;
            tileMapHeight = LayerMap.GetLength(0); // rows
            tileMapWidth = LayerMap.GetLength(1);  // cols
            Tiles = new Tile[tileMapHeight, tileMapWidth];

            // Note: Tiles[y, x] uses [row, col] indexing (y = row, x = col).
            for (int x = 0; x < tileMapWidth; x++)      // columns
                for (int y = 0; y < tileMapHeight; y++) // rows
                {
                    // If we have a palette (tileRefs), choose a random entry from it for visual variety.
                    // Otherwise fall back to the numeric value in LayerMap.
                    int chosenIndex = 0;
                    if (tileRefs != null && tileRefs.Count > 0)
                    {
                        chosenIndex = CombatSystem.RandomInt(0, tileRefs.Count);
                    }
                    else
                    {
                        chosenIndex = LayerMap[y, x];
                    }

                    // Create the Tile data object the rest of the engine will query.
                    // - X/Y are grid coordinates used when computing world positions.
                    // - Id stores the index used to pick a TileRef.
                    // - Passable defaults to true here (LevelManager can override later).
                    // - tileRef points at the sheet coordinates; when tileRefs is empty this may attempt to index by LayerMap value.
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

        // Draw the entire layer:
        // - Uses Helper.SpriteSheet (shared Texture2D) as the source atlas.
        // - For each Tile we compute destination rect in world space and source rect inside the atlas.
        // - Dest uses grid coords * DestTileSize; source uses TileRef._sheetPosX/_sheetPosY * SourceTileSize.
        // - LevelManager / Game.Draw should ensure SpriteBatch.Begin was called with the camera matrix first.
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var Tile in Tiles)
            {
                Rectangle destRect = new Rectangle(
                    Tile.X * DestTileSize,
                    Tile.Y * DestTileSize,
                    DestTileSize,
                    DestTileSize);

                Rectangle sourceRect = new Rectangle(
                    Tile.tileRef._sheetPosX * SourceTileSize,
                    Tile.tileRef._sheetPosY * SourceTileSize,
                    SourceTileSize,
                    SourceTileSize);

                spriteBatch.Draw(Helper.SpriteSheet, destRect, sourceRect, Color.White);
            }
        }
    }
}
