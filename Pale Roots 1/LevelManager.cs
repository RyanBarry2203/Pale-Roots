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

        public List<WorldObject> MapObjects = new List<WorldObject>();

        private Texture2D _groundSheet;
        private Texture2D _animatedObjectSheet;
        private Texture2D _staticObjectSheet;

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
        private void InitializeGameWorld()
        {
            MapObjects.Clear();
            List<TileRef> palette = new List<TileRef>();

            // --- STEP A: DEFINE GROUND PALETTE (Tiles.png) ---
            // Looking at source [1], Row 1 / Col 1 looks like stone/grass.
            // We define a few floor tiles.
            palette.Add(new TileRef(1, 1, (int)TileType.Floor)); // ID 0: Dark Ground
            palette.Add(new TileRef(4, 1, (int)TileType.Floor)); // ID 1: Path Stone
            palette.Add(new TileRef(0, 6, (int)TileType.Wall));  // ID 2: A brick wall (Row 6)

            // --- STEP B: CREATE THE GRID (30x30) ---
            int width = 30;
            int height = 30;
            int[,] map = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0; // Default ground

                    // Create a random path or noise
                    if (CombatSystem.RandomInt(0, 100) > 90)
                    {
                        map[y, x] = 1; // Speckles of stone
                    }
                }
            }

            // Create Borders (Walls)
            for (int x = 0; x < width; x++) { map[0, x] = 2; map[height - 1, x] = 2; }
            for (int y = 0; y < height; y++) { map[y, 0] = 2; map[y, width - 1] = 2; }

            // Create the TileLayer for the background
            CurrentLevel = new TileLayer(map, palette, 64, 64);

            // Set Collision for walls
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // If ID is 2 (Wall), make it solid
                    if (CurrentLevel.Tiles[y, x].tileRef._tileMapValue == 2)
                        CurrentLevel.Tiles[y, x].Passable = false;
                    else
                        CurrentLevel.Tiles[y, x].Passable = true;
                }
            }

            // --- STEP C: ADD "LIVING" OBJECTS ---

            // 1. ADD ANIMATED TREES (from Objects_animated.png [2])
            // The sheet has trees in the top rows. 
            for (int i = 0; i < 15; i++)
            {
                int tx = CombatSystem.RandomInt(2, 28);
                int ty = CombatSystem.RandomInt(2, 28);

                Vector2 pos = new Vector2(tx * 64, ty * 64);

                // Create the tree. FrameCount is 4 (looks like 4 animation frames).
                // scale: 2.0 to make them look big on the 64x64 grid
                var tree = new WorldObject(_game, _animatedObjectSheet, pos, 4, true);

                // Adjust the Source Rectangle for the first tree type (Row 0)
                // Assuming 32x48 standard size for these RPG assets
                tree.spriteWidth = 32;
                tree.spriteHeight = 48;
                tree.Scale = 2.5;

                MapObjects.Add(tree);
            }

            // 2. ADD ROCKS (from Objects.png [3])
            // These are static (1 frame)
            for (int i = 0; i < 10; i++)
            {
                int tx = CombatSystem.RandomInt(2, 28);
                int ty = CombatSystem.RandomInt(2, 28);
                Vector2 pos = new Vector2(tx * 64, ty * 64);

                var rock = new WorldObject(_game, _staticObjectSheet, pos, 1, true);

                // Grab a rock from the sheet (Row 1, Col 0 approx)
                rock.spriteWidth = 32;
                rock.spriteHeight = 32;
                rock.Scale = 2.0;

                MapObjects.Add(rock);
            }
        }

        public void LoadLevel(int index)
        {

            _groundSheet = _game.Content.Load<Texture2D>("tiles");
            _animatedObjectSheet = _game.Content.Load<Texture2D>("Objects_animated");
            _staticObjectSheet = _game.Content.Load<Texture2D>("Objects");

            // Set the static helper for the TileLayer (Background)
            Helper.SpriteSheet = _groundSheet;

            // 2. Generate the Map Data using the new assets
            InitializeGameWorld();
        }
        // Example: In your LevelManager Update method
        public void Update(GameTime gameTime, Player player)
        {
            // Update logic for enemies...
            foreach (Enemy enemy in enemies)
            {
                enemy.CurrentCombatPartner = player;
                enemy.Update(gameTime);
            }
            enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);

            // --- NEW: Update Map Objects (Animations) ---
            foreach (var obj in MapObjects)
            {
                obj.Update(gameTime);
            }
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