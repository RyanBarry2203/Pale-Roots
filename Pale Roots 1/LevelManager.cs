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

            _groundSheet = _game.Content.Load<Texture2D>("tiles");
            _animatedObjectSheet = _game.Content.Load<Texture2D>("Objects_animated");
            _staticObjectSheet = _game.Content.Load<Texture2D>("more Objects");

            // 2. SETUP THE STATIC HELPER
            // Crucial: This tells the TileLayer class which image to use for the floor.
            Helper.SpriteSheet = _groundSheet;

            // 3. GENERATE THE WORLD
            InitializeGameWorld();
        }

        private void InitializeGameWorld()
        {
            MapObjects.Clear();
            enemies.Clear();

            // THE FLOOR

            List<TileRef> palette = new List<TileRef>();
            palette.Add(new TileRef(0, 0, 0)); // ID 0 = Floor

            int width = 30; // Made map slightly bigger
            int height = 30;
            int[,] map = new int[height, width];

            // Fill map with floor
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    map[y, x] = 0;

            CurrentLevel = new TileLayer(map, palette, 64, 64);
            foreach (var tile in CurrentLevel.Tiles) tile.Passable = true;


            // DATA-DRIVEN OBJECT PLACEMENT

            // 1. Place some Ruins (Static)
            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = GetRandomPosition();
                CreateStaticObject("Ruins_Column", pos, _staticObjectSheet);
            }

            // 2. Place some Bushes (Static)
            for (int i = 0; i < 10; i++)
            {
                Vector2 pos = GetRandomPosition();
                CreateStaticObject("Bush_Green", pos, _staticObjectSheet);
            }

            // 3. Place Animated Shrines
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = GetRandomPosition();
                // Shrines have 4 frames
                CreateAnimatedObject("Shrine_Blue", pos, _animatedObjectSheet, 4);
            }
        }
        private void CreateStaticObject(string assetName, Vector2 position, Texture2D sheet)
        {
            Rectangle data = Helper.GetSourceRect(assetName);

            var obj = new WorldObject(_game, sheet, position, 1, true);

            obj.SetSpriteSheetLocation(data);

            MapObjects.Add(obj);
        }
        private void CreateAnimatedObject(string assetName, Vector2 position, Texture2D sheet, int frames)
        {
            Rectangle data = Helper.GetSourceRect(assetName);

            var obj = new WorldObject(_game, sheet, position, frames, true);

            obj.SetSpriteSheetLocation(data);

            MapObjects.Add(obj);
        }
        private Vector2 GetRandomPosition()
        {
            int tx = CombatSystem.RandomInt(2, 28);
            int ty = CombatSystem.RandomInt(2, 28);
            return new Vector2(tx * 64, ty * 64);
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