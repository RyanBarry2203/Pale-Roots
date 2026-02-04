using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pale_Roots_1
{
    public class TileLayer
    {
        int SourceTileSize = 16;
        int DestTileSize = 64;
        List<TileRef> tileRefs = new List<TileRef>();

        
        int tileMapHeight;  // row int[row,col]
        int tileMapWidth; // dim 0 = row, dim 1 = col
        Tile[,] _tiles;
        public Tile[,] Tiles
        {
            get { return _tiles; }
            set { _tiles = value; }
        }
        public TileLayer(int[,] LayerMap,List<TileRef> MapSheetReferences, int destSize, int sourceSize)
        {

            DestTileSize = destSize;
            SourceTileSize = sourceSize;

            tileRefs = MapSheetReferences;
            tileMapHeight = LayerMap.GetLength(0); // row int[row,col]
            tileMapWidth = LayerMap.GetLength(1); // dim 0 = row, dim 1 = col
            Tiles = new Tile[tileMapHeight, tileMapWidth];
            for (int x = 0; x < tileMapWidth; x++)  // look at columns in row
                for (int y = 0; y < tileMapHeight; y++) // look at rows
                {
                    Tiles[y, x] =
                        new Tile
                        {
                            X = x,
                            Y = y,
                            Id = LayerMap[y, x],
                            Passable = true,
                            tileRef = tileRefs[LayerMap[y, x]]
                        };
                }
        }

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
