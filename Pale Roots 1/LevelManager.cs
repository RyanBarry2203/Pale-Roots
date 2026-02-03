using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class LevelManager
    {
        private Game _game;
        private List<Level> _allLevels = new List<Level>();

        // --- ENTITY LISTS ---
        public List<Enemy> enemies = new List<Enemy>();
        public List<WorldObject> MapObjects = new List<WorldObject>();

        // --- GRAPHICS ---
        public TileLayer CurrentLevel { get; private set; }
        private Texture2D _groundSheet;
        private Texture2D _animatedObjectSheet;
        private Texture2D _staticObjectSheet;

        public LevelManager(Game game)
        {
            _game = game;
            // We don't initialize here anymore, we do it in LoadLevel to be safe
        }

        public void LoadLevel(int index)
        {
            // 1. LOAD TEXTURES
            // Ensure these match your content names exactly (Case Sensitive!)
            _groundSheet = _game.Content.Load<Texture2D>("Tiles");
            _animatedObjectSheet = _game.Content.Load<Texture2D>("Objects_animated");
            _staticObjectSheet = _game.Content.Load<Texture2D>("Objects");

            // 2. SETUP THE STATIC HELPER
            // Crucial: This tells the TileLayer class which image to use for the floor.
            Helper.SpriteSheet = _groundSheet;

            // 3. GENERATE THE WORLD
            InitializeGameWorld();
        }

        private void InitializeGameWorld()
        {
            // Clear any old junk
            MapObjects.Clear();
            enemies.Clear(); // Clear enemies if you want a fresh start

            // --------------------------------------------------------
            // STEP A: THE FLOOR (Background)
            // --------------------------------------------------------
            List<TileRef> palette = new List<TileRef>();

            // We pick a safe tile from 'Tiles.png'. 
            // Row 1, Col 1 is usually a safe dark stone/grass in these packs.
            palette.Add(new TileRef(1, 1, 0)); // ID 0 = Floor

            // Create a 20x20 Grid (Small and manageable)
            int width = 20;
            int height = 20;
            int[,] map = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0; // Fill everything with ID 0
                }
            }

            // Create the TileLayer
            // 64, 64 is the size we want to draw them on screen (scaled up)
            CurrentLevel = new TileLayer(map, palette, 64, 64);

            // Set all floor tiles to be Walkable
            foreach (var tile in CurrentLevel.Tiles) tile.Passable = true;


            // --------------------------------------------------------
            // STEP B: THE TREES (Foreground)
            // --------------------------------------------------------
            // We place exactly 5 trees.
            for (int i = 0; i < 5; i++)
            {
                // Random position away from the edges
                int tx = CombatSystem.RandomInt(2, 18);
                int ty = CombatSystem.RandomInt(2, 18);
                Vector2 pos = new Vector2(tx * 64, ty * 64);

                // Create the Tree
                // The sheet is a grid of trees. We want the top-left one.
                // It has 4 frames of animation.
                var tree = new WorldObject(_game, _animatedObjectSheet, pos, 4, true);

                // MANUAL SLICING (Fixes the "Mess")
                // We overwrite the automatic calculation.
                // Each tree frame is roughly 32 pixels wide and 48 pixels tall in the file.
                tree.spriteWidth = 32;
                tree.spriteHeight = 48;

                // Set the Source Rectangle to the Top-Left corner of the sheet
                // Frame calculation in Sprite.cs will handle the X offset for animation.
                tree.Scale = 2.0f; // Scale up to look nice

                MapObjects.Add(tree);
            }

            // --------------------------------------------------------
            // STEP C: THE ROCKS (Static Cover)
            // --------------------------------------------------------
            // We place exactly 5 rocks.
            for (int i = 0; i < 5; i++)
            {
                int tx = CombatSystem.RandomInt(2, 18);
                int ty = CombatSystem.RandomInt(2, 18);
                Vector2 pos = new Vector2(tx * 64, ty * 64);

                // Create Rock (1 Frame, Static)
                var rock = new WorldObject(_game, _staticObjectSheet, pos, 1, true);

                // MANUAL SLICING
                // We pick a specific rock from the big Objects.png atlas.
                // Let's grab the grey rock at (0, 96) - roughly 3rd row down.
                rock.spriteWidth = 32;
                rock.spriteHeight = 32;

                // For static objects, we must hard-code the source rectangle 
                // so it doesn't try to draw the whole sheet.
                rock.sourceRectangle = new Rectangle(0, 96, 32, 32);

                rock.Scale = 2.0f;

                MapObjects.Add(rock);
            }
        }

        public void Update(GameTime gameTime, Player player)
        {
            // Update Enemies
            foreach (Enemy enemy in enemies)
            {
                enemy.CurrentCombatPartner = player;
                enemy.Update(gameTime);
            }
            enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);

            // Update Map Objects (Animations)
            foreach (var obj in MapObjects)
            {
                obj.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Only draw the floor tiles here. 
            // Objects are drawn in ChaseAndFireEngine to get the depth sorting!
            if (CurrentLevel != null)
            {
                CurrentLevel.Draw(spriteBatch);
            }
        }
    }
}