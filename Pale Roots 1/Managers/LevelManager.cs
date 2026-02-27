using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class LevelManager
    {
        private Game _game;

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
        }

        public void LoadLevel(int index)
        {
            _groundSheet = _game.Content.Load<Texture2D>("MainLev2.0");
            _animatedObjectSheet = _game.Content.Load<Texture2D>("Objects_animated");
            _staticObjectSheet = _game.Content.Load<Texture2D>("more Objects");

            Helper.SpriteSheet = _groundSheet;
            InitializeGameWorld(index);
        }

        private void InitializeGameWorld(int levelIndex)
        {
            int width = 60;
            int height = 34;
            int[,] map = new int[height, width];

            // 1. CREATE FLOOR
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0;
                }
            }

            // 2. FETCH PALETTE FROM DATA LIBRARY (API CALL)
            List<TileRef> palette = LevelDataLibrary.GetLevelPalette(levelIndex);

            CurrentLevel = new TileLayer(map, palette, 64, 32);

            float centerX = width / 2f;
            float centerY = height / 2f;
            float safeRadiusX = 22f;
            float safeRadiusY = 11f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = Math.Abs(x - centerX);
                    float dy = Math.Abs(y - centerY);

                    float excessX = Math.Max(dx - safeRadiusX, 0);
                    float excessY = Math.Max(dy - safeRadiusY, 0);
                    float distanceOutsideBox = (float)Math.Sqrt(excessX * excessX + excessY * excessY);

                    if (distanceOutsideBox > 0)
                    {
                        float chance = distanceOutsideBox / 4.5f;
                        chance += CombatSystem.RandomFloat(-0.2f, 0.2f);

                        if (CombatSystem.RandomFloat() < chance)
                        {
                            Vector2 pos = new Vector2(x * 64, y * 64);
                            pos.X += CombatSystem.RandomInt(-24, 24);
                            pos.Y += CombatSystem.RandomInt(-24, 24);

                            // Fetching dynamically from the Data Library
                            string randomNature = LevelDataLibrary.NatureObjects[CombatSystem.RandomInt(0, LevelDataLibrary.NatureObjects.Length)];
                            CreateStaticObject(randomNature, pos, _staticObjectSheet, false);
                        }
                    }
                }
            }

            PlaceLandMarks();
        }

        private bool IsSpaceOccupied(Vector2 pos, float minGap)
        {
            foreach (var obj in MapObjects)
            {
                float objectRadius = (obj.spriteWidth * (float)obj.Scale) / 2.5f;
                float safeDistance = minGap + objectRadius;

                if (Vector2.Distance(pos, obj.position) < safeDistance)
                {
                    return true;
                }
            }
            return false;
        }

        private void PlaceLandMarks()
        {
            Vector2 centerPos = new Vector2(30 * 64, 17 * 64);
            CreateAnimatedObject("Tree_Dead_Large", centerPos, _animatedObjectSheet, 4);

            Vector2 kingPos = new Vector2(40 * 64, 16 * 64);
            CreateStaticObject("Skellington", kingPos, _staticObjectSheet, false);

            Vector2 bigRuinPos = new Vector2(8 * 64, 6 * 64);
            CreateStaticObject("Ruins_Column", bigRuinPos, _staticObjectSheet, false);
            CreateStaticObject("Big_Rock", bigRuinPos + new Vector2(200, 190), _staticObjectSheet, false);

            CreateStaticObject("Smaller_Ruin", new Vector2(50 * 64, 7 * 64), _staticObjectSheet, false);
            CreateStaticObject("Smaller_Ruin", new Vector2(48 * 64, 26 * 64), _staticObjectSheet, false);

            FillZones(centerPos);
        }

        private void FillZones(Vector2 centerPos)
        {
            int attempts = 0;
            int maxItems = 60;
            int itemsPlaced = 0;

            while (itemsPlaced < maxItems && attempts < 1000)
            {
                attempts++;
                int tx = CombatSystem.RandomInt(6, 54);
                int ty = CombatSystem.RandomInt(6, 28);
                Vector2 pos = new Vector2(tx * 64, ty * 64);
                pos += new Vector2(CombatSystem.RandomInt(-20, 20), CombatSystem.RandomInt(-20, 20));

                float distToCenter = Vector2.Distance(pos, centerPos);

                if (IsSpaceOccupied(pos, 80f)) continue;

                string assetToSpawn = "";
                bool isSolid = false;

                // --- ZONE LOGIC ---
                if (distToCenter < 350 && distToCenter > 100)
                {
                    assetToSpawn = LevelDataLibrary.BoneObjects[CombatSystem.RandomInt(0, LevelDataLibrary.BoneObjects.Length)];
                }
                else if (tx < 22)
                {
                    assetToSpawn = LevelDataLibrary.GraveObjects[CombatSystem.RandomInt(0, LevelDataLibrary.GraveObjects.Length)];

                }
                else if (tx > 38)
                {
                    if (CombatSystem.RandomInt(0, 100) > 70)
                        assetToSpawn = LevelDataLibrary.RuinObjects[CombatSystem.RandomInt(0, LevelDataLibrary.RuinObjects.Length)];
                    else
                        assetToSpawn = LevelDataLibrary.NatureObjects[CombatSystem.RandomInt(0, LevelDataLibrary.NatureObjects.Length)];

                }
                else
                {
                    if (CombatSystem.RandomInt(0, 100) > 50)
                        assetToSpawn = LevelDataLibrary.BoneObjects[CombatSystem.RandomInt(0, LevelDataLibrary.BoneObjects.Length)];
                    else
                        assetToSpawn = LevelDataLibrary.NatureObjects[CombatSystem.RandomInt(0, LevelDataLibrary.NatureObjects.Length)];
                }

                if (assetToSpawn != "")
                {
                    if (IsTooCloseToIdentical(assetToSpawn, pos, 500f)) continue;

                    CreateStaticObject(assetToSpawn, pos, _staticObjectSheet, isSolid);
                    itemsPlaced++;
                }
            }
        }

        private bool IsTooCloseToIdentical(string assetName, Vector2 pos, float minDistance)
        {
            foreach (var obj in MapObjects)
            {
                if (obj.AssetName == assetName)
                {
                    if (Vector2.Distance(pos, obj.position) < minDistance) return true;
                }
            }
            return false;
        }

        private void CreateStaticObject(string assetName, Vector2 position, Texture2D sheet, bool isSolid)
        {
            Rectangle data = Helper.GetSourceRect(assetName);
            var obj = new WorldObject(_game, sheet, position, 1, isSolid);
            obj.AssetName = assetName;
            obj.SetSpriteSheetLocation(data);
            MapObjects.Add(obj);
        }

        private void CreateAnimatedObject(string assetName, Vector2 position, Texture2D sheet, int frames)
        {
            Rectangle data = Helper.GetSourceRect(assetName);
            var obj = new WorldObject(_game, sheet, position, frames, false);
            obj.SetSpriteSheetLocation(data);
            MapObjects.Add(obj);
        }

        public void GenerateBossArena()
        {
            // 1. Use the same dimensions as the main game so it looks "Big"
            int width = 60;
            int height = 34;
            int[,] map = new int[height, width];

            // 2. Fill the floor with standard grass (ID 0)
            // This ensures no black void; the floor extends to the screen edges.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0;
                }
            }

            // 3. Create the TileLayer
            // We reuse the palette from Level 0 so the grass looks identical to the main game
            List<TileRef> palette = LevelDataLibrary.GetLevelPalette(0);
            CurrentLevel = new TileLayer(map, palette, 64, 32);

            // 4. Clear old objects
            MapObjects.Clear();

            // 5. Create the "Dense Tree Line" (The Arena Walls)
            // We will place trees in a circle or box to lock the player in.
            Vector2 center = new Vector2(width * 64 / 2, height * 64 / 2);
            float arenaRadius = 900f; // The fighting space radius

            // Loop through the whole map area
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 tilePos = new Vector2(x * 64, y * 64);
                    float dist = Vector2.Distance(tilePos, center);

                    // If we are OUTSIDE the arena radius, place a tree.
                    // We add some noise so it doesn't look like a perfect artificial circle.
                    if (dist > arenaRadius)
                    {
                        if (CombatSystem.RandomInt(0, 10) < 8)
                        {

                            string[] natureAssets = LevelDataLibrary.NatureObjects;
                            string randomAsset = natureAssets[CombatSystem.RandomInt(0, natureAssets.Length)];

                            Vector2 pos = tilePos + new Vector2(CombatSystem.RandomInt(-20, 20), CombatSystem.RandomInt(-20, 20));
                            CreateStaticObject(randomAsset, pos, _staticObjectSheet, true);
                        }
                    }
                }
            }
        }

        public void Update(GameTime gameTime, Player player)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.CurrentCombatPartner = player;
                enemy.Update(gameTime);
            }
            enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);

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