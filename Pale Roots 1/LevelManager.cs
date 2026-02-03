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
            // ---------------------------------------------------------
            // 1. DEFINE PALETTE
            // ---------------------------------------------------------
            // We strictly define our IDs here so we know what numbers to use later.
            // IDs 0, 1, 2 = GRASS (Walkable)
            // IDs 3, 4, 5 = WALLS (Solid)
            // ID  6       = PATH  (Walkable)
            // IDs 10+     = TREE  (Solid)

            List<TileRef> palette = new List<TileRef>();

            // --- GRASS (IDs 0, 1, 2) ---
            // Row 0, Cols 0-2 (Walkable)
            for (int i = 0; i < 3; i++)
            {
                palette.Add(new TileRef(i, 0, (int)TileType.Floor));
            }

            // --- WALLS (IDs 3, 4, 5) ---
            // Row 1, Cols 0-2 (Solid)
            for (int i = 0; i < 3; i++)
            {
                palette.Add(new TileRef(i, 1, (int)TileType.Wall));
            }

            // --- DIRT PATH (IDs 6, 7, 8) ---
            // Row 2, Cols 0-2 (Walkable)
            // We add a loop here so the path can look "scuffed" and natural
            for (int i = 0; i < 3; i++)
            {
                palette.Add(new TileRef(i, 2, (int)TileType.Floor));
            }

            // --- TREE (IDs 9+) ---
            // Starts at current count (should be 9)
            int treeStartID = palette.Count;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    palette.Add(new TileRef(j, i + 3, (int)TileType.Tree));
                }
            }

            // ---------------------------------------------------------
            // 2. GENERATE MAP
            // ---------------------------------------------------------
            int width = 30;
            int height = 30;
            int[,] map = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // FILL MAP WITH RANDOM GRASS
                    // We use IDs 0, 1, and 2 which we defined as Grass above
                    map[y, x] = CombatSystem.RandomInt(0, 3);
                }
            }

            // CREATE BORDERS (Walls)
            // We use ID 3 (The first wall variant)
            // You could use RandomInt(3, 6) if you wanted mixed walls!
            int wallID = 3;

            for (int x = 0; x < width; x++)
            {
                map[0, x] = wallID;             // Top Edge
                map[height - 1, x] = wallID;    // Bottom Edge
            }

            for (int y = 0; y < height; y++)
            {
                map[y, 0] = wallID;             // Left Edge
                map[y, width - 1] = wallID;     // Right Edge
            }

            // CREATE PATH
            // We use ID 6 (Dirt Path)
            int pathRow = height / 2;
            for (int x = 0; x < width - 1; x++) // Start at 1 to avoid overwriting the border wall
            {
                map[pathRow, x] = CombatSystem.RandomInt(6, 9);
            }

            // CREATE TREE
            // We use the treeStartID we captured earlier
            int treeX = 10;
            int treeY = 5;
            int currentTreeTile = treeStartID;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (treeY + i < height && treeX + j < width)
                    {
                        map[treeY + i, treeX + j] = currentTreeTile;
                    }
                    currentTreeTile++;
                }
            }

            // Add the level to the list
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