using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class LevelManager
    {
        private Game _game;

        // lists of entities managed by this level manager.
        public List<Enemy> enemies = new List<Enemy>();
        public List<WorldObject> MapObjects = new List<WorldObject>();

        // textures and the current tile layer for rendering the level.
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

            // fill the tile grid with the default floor tile id.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0;
                }
            }

            // load the palette for this level from the data library.
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

                            // pick a random nature asset from the data library and place it.
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
            // check existing map objects to ensure a minimum spacing around a candidate position.
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
            // place specific large objects used as visual landmarks and anchors for procedural placement.
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

            // repeatedly attempt to place decorative debris until quota or attempt limit is reached.
            while (itemsPlaced < maxItems && attempts < 1000)
            {
                attempts++;

                // choose a random tile inside the play area.
                int tx = CombatSystem.RandomInt(6, 54);
                int ty = CombatSystem.RandomInt(6, 28);
                Vector2 pos = new Vector2(tx * 64, ty * 64);

                // offset the position slightly for a natural look.
                pos += new Vector2(CombatSystem.RandomInt(-20, 20), CombatSystem.RandomInt(-20, 20));

                float distToCenter = Vector2.Distance(pos, centerPos);

                // skip placement if the spot is already occupied.
                if (IsSpaceOccupied(pos, 80f)) continue;

                string assetToSpawn = "";
                bool isSolid = false;

                // decide which asset to spawn based on zone rules and position.
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

                // place the selected asset if it does not violate spacing rules.
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
            // ensure we don't put two identical objects too near each other.
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
            // construct a static world object from the sprite sheet and add it to the map.
            Rectangle data = Helper.GetSourceRect(assetName);
            var obj = new WorldObject(_game, sheet, position, 1, isSolid);
            obj.AssetName = assetName;
            obj.SetSpriteSheetLocation(data);
            MapObjects.Add(obj);
        }

        private void CreateAnimatedObject(string assetName, Vector2 position, Texture2D sheet, int frames)
        {
            // construct an animated world object and add it to the map.
            Rectangle data = Helper.GetSourceRect(assetName);
            var obj = new WorldObject(_game, sheet, position, frames, false);
            obj.SetSpriteSheetLocation(data);
            MapObjects.Add(obj);
        }

        public void GenerateBossArena()
        {
            // create a base floor and populate the outer ring with trees to form arena walls.
            int width = 60;
            int height = 34;
            int[,] map = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0;
                }
            }

            // reuse the palette so the arena matches the main level visually.
            List<TileRef> palette = LevelDataLibrary.GetLevelPalette(0);
            CurrentLevel = new TileLayer(map, palette, 64, 32);

            // clear existing objects and populate the outer area with nature assets to constrain movement.
            MapObjects.Clear();

            Vector2 center = new Vector2(width * 64 / 2, height * 64 / 2);
            float arenaRadius = 900f; // the radius of the playable area

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 tilePos = new Vector2(x * 64, y * 64);
                    float dist = Vector2.Distance(tilePos, center);

                    if (dist > arenaRadius)
                    {
                        if (CombatSystem.RandomInt(0, 10) < 8)
                        {

                            string[] natureAssets = LevelDataLibrary.NatureObjects;
                            string randomAsset = natureAssets[CombatSystem.RandomInt(0, natureAssets.Length)];

                            Vector2 pos = tilePos + new Vector2(CombatSystem.RandomInt(-20, 20), CombatSystem.RandomInt(-20, 20));
                            CreateStaticObject(randomAsset, pos, _staticObjectSheet, false);
                        }
                    }
                }
            }
        }

        public void Update(GameTime gameTime, Player player)
        {
            // update all enemies and assign the player as their combat partner.
            foreach (Enemy enemy in enemies)
            {
                enemy.CurrentCombatPartner = player;
                enemy.Update(gameTime);
            }
            enemies.RemoveAll(e => e.LifecycleState == Enemy.ENEMYSTATE.DEAD);

            // update all placed world objects.
            foreach (var obj in MapObjects)
            {
                obj.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw the current tile layer if available.
            if (CurrentLevel != null)
            {
                CurrentLevel.Draw(spriteBatch);
            }
        }
    }
}