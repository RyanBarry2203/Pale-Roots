using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Pale_Roots_1.Level;

namespace Pale_Roots_1
{
    public class LevelManager
    {
        private Game _game;
        private List<Level> _allLevels = new List<Level>();
        public List<Enemy> enemies = new List<Enemy>();
        //private Texture2D _mapSheet;

        // The Engine reads this to know about walls and floors
        public TileLayer CurrentLevel { get; private set; }

        public LevelManager(Game game)
        {
            _game = game;
            InitializeLevels();
        }

        private void InitializeLevels()
        {
            //// 1. DEFINE PALETTE
            //// This tells the game what the numbers mean.
            //// 0 = Wall, 1 = Floor
            //List<TileRef> palette = new List<TileRef>();

            //// Wall (ID 0) -> Row 4, Col 0 on sheet
            //palette.Add(new TileRef(0, 4, (int)TileType.Wall));

            //// Floor (ID 1) -> Row 2, Col 3 on sheet
            //palette.Add(new TileRef(3, 2, (int)TileType.Floor));

            //// 2. DEFINE MAP
            //// Let's make a Big Open Plane (30x30 tiles)
            //// 0 = Wall, 1 = Floor
            //int width = 30;
            //int height = 30;
            //int[,] bigMap = new int[height, width];

            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        // Make the borders Walls, everything else Floor
            //        if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
            //            bigMap[y, x] = 0; // Wall
            //        else
            //            bigMap[y, x] = 1; // Floor
            //    }
            //}

            List<TileRef> palette = new List<TileRef>();

            //normal floor
            palette.Add(new TileRef(0, 0, (int)TileType.Floor)); // Grass
            // floer floor
            palette.Add(new TileRef(1, 0, (int)TileType.Floor)); // flower grass
            // wall
            palette.Add(new TileRef(0, 1, (int)TileType.Wall)); // rock wall
            //Path
            palette.Add(new TileRef(0, 2, (int)TileType.Floor)); // dirt path
            // Cracked wall
            palette.Add(new TileRef(3, 2, (int)TileType.Wall)); // cracked wall


            int treeStartID = 10;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    palette.Add(new TileRef(j, i + 3, (int)TileType.Tree)); // Trees
                }
            }

            // Add Level 1 with the big map
            //_allLevels.Add(new Level(bigMap, palette, new Vector2(100, 100)));

            int width = 30;
            int height = 30;
            int[,] map = new int[height, width];

            for (int  i = 0;  i < height;  i++)
            {
                for (int j = 0; j < width; j++)
                {
                    map[i, j] = 0;
                    if (CombatSystem.RandomInt(0, 10) > 8)
                    {
                        map[i, j] = 1;
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                map[0, x] = 2;
                map[height - 1, x] = 2;
            }
            for (int y = 0; y < height; y++)
            {
                map[y, 0] = 2;
                map[y, width - 1] = 2;
            }

            int pathRow = height / 2;
            for (int x = 0; x < width; x++)
            {
                map[pathRow, x] = 4;
            }

            int treeX = 10;
            int treeY = 5;
            int currentTreeID = 5;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    map[treeY + i, treeX + j] = currentTreeID;
                    currentTreeID++;
                }
            }

            _allLevels.Add(new Level(map, palette, new Vector2(100, 100)));
        }

        public void LoadLevel(int index)
        {
            if (index < 0 || index >= _allLevels.Count) return;
            Level data = _allLevels[index];

            // 1. LOAD TEXTURE
            // Ensure this file name is EXACTLY right in your Content folder!
            Texture2D tileSheet = _game.Content.Load<Texture2D>("MapSheet");

            // CRITICAL: We must set this so TileLayer knows what image to draw
            Helper.SpriteSheet = tileSheet;



            // 2. CREATE LAYER
            CurrentLevel = new TileLayer(data.MapLayout, data.TilePalette, 64, 64);

            // 3. SET COLLISION
            // Loop through the map and make walls solid
            for (int y = 0; y < CurrentLevel.Tiles.GetLength(0); y++)
            {
                for (int x = 0; x < CurrentLevel.Tiles.GetLength(1); x++)
                {
                    //// If Tile ID is Wall, make it impassable
                    //if (CurrentLevel.Tiles[y, x].Id == (int)TileType.Wall)
                    //{
                    //    CurrentLevel.Tiles[y, x].Passable = false;
                    //}

                    int typeOfTile = CurrentLevel.Tiles[y, x].tileRef._tileMapValue;

                    if (typeOfTile == (int)TileType.Wall || typeOfTile == (int)TileType.Tree)
                    {
                        CurrentLevel.Tiles[y, x].Passable = false;  
                    }
                    else                    
                    {
                        CurrentLevel.Tiles[y, x].Passable = true;
                    }
                }
            }
        }
        // Example: In your LevelManager Update method
        public void Update(GameTime gameTime, Player player)
        {
            foreach (Enemy enemy in enemies)
            {
                // Give the enemy the player reference so 'CheckForTarget' actually works
                enemy.CurrentCombatPartner = player;

                // Call the enemy's update logic we just wrote
                enemy.Update(gameTime);
            }

            // Clean up dead enemies from the list to save memory
            enemies.RemoveAll(e => e.EnemyStateza == Enemy.ENEMYSTATE.DEAD);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentLevel != null)
            {
                CurrentLevel.Draw(spriteBatch);
            }
        }
    }
}