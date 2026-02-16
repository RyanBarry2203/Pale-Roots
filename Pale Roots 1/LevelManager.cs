using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

        private string[] _treeTypes = {
            "Dying_Tree",
            "Medium_Dying_Tree",
            "Small_Dying_Tree",
        };

        private string[] _brambleTypes = {
            "Brambles_Large",
            "Brambles_Medium",
            "Brambles_Small",
            "Brambles_Tiny",
            "Brambles_Very_Tiny"
        };

        private string[] _floorDetails = {
            "Bone_In_Floor",
            "Hand_In_Floor",
            "Hand_In_Floor_Medium",
            "Hand_In_Floor_Small",
            "Hand_In_Floor_Tiny",
            "Ribcage",
            "Bird_Skull"
        };

        private string[] _graveTypes = { "Grave_1", "Grave_2", "Grave_3" };

        public LevelManager(Game game)
        {
            _game = game;
            // We don't initialize here anymore, we do it in LoadLevel to be safe
        }

        public void LoadLevel(int index)
        {
            _groundSheet = _game.Content.Load<Texture2D>("MainLev2.0");

            _animatedObjectSheet = _game.Content.Load<Texture2D>("Objects_animated");
            _staticObjectSheet = _game.Content.Load<Texture2D>("more Objects");

            Helper.SpriteSheet = _groundSheet;

            InitializeGameWorld();
        }

        private void InitializeGameWorld()
        {

            int width = 60;
            int height = 34;
            int[,] map = new int[height, width];

            // 2. CREATE FLOOR
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0;
                }
            }

            // i know i should have used a loop okay but it was faster to just brute force it than use brain rescources to figure out the maths for the palette indexing. Sue me.
            List<TileRef> palette = new List<TileRef>();
            palette.Add(new TileRef(13, 41, 0));
            palette.Add(new TileRef(14, 41, 0));
            palette.Add(new TileRef(15, 41, 0));
            palette.Add(new TileRef(16, 41, 0));
            palette.Add(new TileRef(17, 41, 0));
            palette.Add(new TileRef(13, 42, 0));
            palette.Add(new TileRef(14, 42, 0));
            palette.Add(new TileRef(15, 42, 0));
            palette.Add(new TileRef(16, 42, 0));
            palette.Add(new TileRef(17, 42, 0));
            palette.Add(new TileRef(13, 43, 0));
            palette.Add(new TileRef(14, 43, 0));
            palette.Add(new TileRef(15, 43, 0));
            palette.Add(new TileRef(16, 43, 0));
            palette.Add(new TileRef(17, 43, 0));
            palette.Add(new TileRef(13, 44, 0));
            palette.Add(new TileRef(14, 44, 0));
            palette.Add(new TileRef(15, 44, 0));
            palette.Add(new TileRef(16, 44, 0));
            palette.Add(new TileRef(17, 44, 0));
            palette.Add(new TileRef(13, 45, 0));
            palette.Add(new TileRef(14, 45, 0));
            palette.Add(new TileRef(15, 45, 0));
            palette.Add(new TileRef(16, 45, 0));
            palette.Add(new TileRef(17, 45, 0));
            palette.Add(new TileRef(13, 46, 0));
            palette.Add(new TileRef(14, 46, 0));
            palette.Add(new TileRef(15, 46, 0));
            palette.Add(new TileRef(16, 46, 0));
            palette.Add(new TileRef(17, 46, 0));
            palette.Add(new TileRef(13, 47, 0));
            palette.Add(new TileRef(14, 47, 0));
            palette.Add(new TileRef(15, 47, 0));
            palette.Add(new TileRef(16, 47, 0));
            palette.Add(new TileRef(17, 47, 0));
            palette.Add(new TileRef(13, 48, 0));
            palette.Add(new TileRef(14, 48, 0));
            palette.Add(new TileRef(15, 48, 0));
            palette.Add(new TileRef(16, 48, 0));
            palette.Add(new TileRef(17, 48, 0));

            palette.Add(new TileRef(9, 42, 0));
            palette.Add(new TileRef(10, 42, 0));
            palette.Add(new TileRef(11, 42, 0));
            palette.Add(new TileRef(12, 42, 0));
            palette.Add(new TileRef(9, 43, 0));
            palette.Add(new TileRef(10, 43, 0));
            palette.Add(new TileRef(11, 43, 0));
            palette.Add(new TileRef(12, 43, 0));
            //palette.Add(new TileRef(9, 44, 0)); 
            //palette.Add(new TileRef(10, 44, 0));
            //palette.Add(new TileRef(11, 44, 0));
            //palette.Add(new TileRef(12, 44, 0));

            palette.Add(new TileRef(9, 46, 0));
            palette.Add(new TileRef(10, 46, 0));
            palette.Add(new TileRef(11, 46, 0));
            palette.Add(new TileRef(12, 46, 0));
            palette.Add(new TileRef(9, 47, 0));
            palette.Add(new TileRef(10, 47, 0));
            palette.Add(new TileRef(11, 47, 0));
            palette.Add(new TileRef(12, 47, 0));
            palette.Add(new TileRef(9, 48, 0));
            palette.Add(new TileRef(10, 48, 0));
            palette.Add(new TileRef(11, 48, 0));
            palette.Add(new TileRef(12, 48, 0));


            CurrentLevel = new TileLayer(map, palette, 64, 32);



            float centerX = width / 2f;
            float centerY = height / 2f;

            float safeRadiusX = 25f; // Wide
            float safeRadiusY = 13f; // Short

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // --- RECTANGULAR SDF MATH ---

                    // 1. Distance from center
                    float dx = Math.Abs(x - centerX);
                    float dy = Math.Abs(y - centerY);

                    // 2. Subtract the specific radius for that axis
                    // This creates a box that is wider than it is tall
                    float excessX = Math.Max(dx - safeRadiusX, 0);
                    float excessY = Math.Max(dy - safeRadiusY, 0);

                    // 3. Calculate distance outside that box
                    float distanceOutsideBox = (float)Math.Sqrt(excessX * excessX + excessY * excessY);

                    if (distanceOutsideBox > 0)
                    {
                        // 4. Calculate Chance (Gradient)
                        // We make the gradient slightly steeper (6.0f) so the trees get dense faster
                        float chance = distanceOutsideBox / 6.0f;
                        chance *= 0.8f; // Density multiplier

                        if (CombatSystem.RandomFloat() < chance)
                        {
                            Vector2 pos = new Vector2(x * 64, y * 64);
                            pos.X += CombatSystem.RandomInt(-20, 20);
                            pos.Y += CombatSystem.RandomInt(-20, 20);


                            if (CombatSystem.RandomInt(0, 100) < 90)
                            {
                                // Pick a random tree from our new array
                                string randomTree = _treeTypes[CombatSystem.RandomInt(0, _treeTypes.Length)];
                                CreateStaticObject(randomTree, pos, _staticObjectSheet, false);
                            }
                            else
                            {

                                string randomBramble = _brambleTypes[CombatSystem.RandomInt(0, _brambleTypes.Length)];
                                CreateStaticObject(randomBramble, pos, _staticObjectSheet, false);
                            }
                        }
                    }
                }
            }

            PlaceLandMarks();
        }
        //private void PlaceLandMarks()
        //{
        //    // Animated objects are fine as they were
        //    Vector2 centerPos = new Vector2(30 * 64, 17 * 64);
        //    CreateAnimatedObject("Tree_Dead_Large", centerPos, _animatedObjectSheet, 4);

        //    Vector2 topRightPos = new Vector2(45 * 64, 5 * 64);

        //    // CHANGE: Columns and Rocks are solid (true)
        //    CreateStaticObject("Ruins_Column", topRightPos, _staticObjectSheet, true);
        //    CreateStaticObject("Big_Rock", topRightPos + new Vector2(50, 50), _staticObjectSheet, true);

        //    Vector2 bottomLeftPos = new Vector2(15 * 64, 28 * 64);
        //    CreateStaticObject("Ruins_Column", bottomLeftPos, _staticObjectSheet, true);

        //    Vector2 ringCenter = new Vector2(45 * 64, 25 * 64);
        //    int radius = 150;
        //    int skullCount = 8;

        //    for (int i = 0; i < skullCount; i++)
        //    {
        //        float angle = i * (MathHelper.TwoPi / skullCount);
        //        Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);


        //        CreateStaticObject("Skull_Pile", ringCenter + offset, _staticObjectSheet, false);
        //    }

        //    for (int i = 0; i < 15; i++)
        //    {
        //        int rx = CombatSystem.RandomInt(10, 50);
        //        int ry = CombatSystem.RandomInt(5, 29);
        //        Vector2 pos = new Vector2(rx * 64, ry * 64);

        //        if (CombatSystem.RandomInt(0, 100) > 50)

        //            CreateStaticObject("Big_Rock", pos, _staticObjectSheet, true);
        //        else

        //            CreateStaticObject("Hand_In_Floor", pos, _staticObjectSheet, false);
        //    }
        //}
        private bool IsSpaceOccupied(Vector2 pos, float minGap)
        {
            foreach (var obj in MapObjects)
            {

                float objectRadius = (obj.spriteWidth * (float)obj.Scale) / 2.5f;

                float safeDistance = minGap + objectRadius;

                // 3. Check distance
                if (Vector2.Distance(pos, obj.position) < safeDistance)
                {
                    return true; // Too close!
                }
            }
            return false;
        }
        private void PlaceLandMarks()
        {
            // ==========================================
            // 1. FIXED LANDMARKS (Spawn these FIRST)
            // ==========================================

            // Center Tree
            Vector2 centerPos = new Vector2(30 * 64, 17 * 64);
            CreateAnimatedObject("Tree_Dead_Large", centerPos, _animatedObjectSheet, 4);

            // Skeleton King (Right, slightly Up)
            Vector2 kingPos = new Vector2(42 * 64, 14 * 64);
            CreateStaticObject("Skellington", kingPos, _staticObjectSheet, true);

            // Big Ruin (Top Right)
            Vector2 bigRuinPos = new Vector2(52 * 64, 6 * 64);
            CreateStaticObject("Ruins_Column", bigRuinPos, _staticObjectSheet, true);
            CreateStaticObject("Big_Rock", bigRuinPos + new Vector2(-60, 20), _staticObjectSheet, true);
            CreateStaticObject("Ruins_Column", bigRuinPos + new Vector2(50, 40), _staticObjectSheet, true);

            // Smaller Ruin (Bottom Right)
            Vector2 smallRuinPos = new Vector2(50 * 64, 28 * 64);
            CreateStaticObject("Smaller_Ruin", smallRuinPos, _staticObjectSheet, true);


            // ==========================================
            // 2. THE GRAVEYARD (Left Side Only)
            // ==========================================
            int gravesPlaced = 0;
            int attempts = 0;

            while (gravesPlaced < 25 && attempts < 200)
            {
                attempts++;

                // Random Position on Left Side
                int gx = CombatSystem.RandomInt(4, 22);
                int gy = CombatSystem.RandomInt(4, 30);
                Vector2 gravePos = new Vector2(gx * 64, gy * 64);

                // CHECK: Increased gap to 80f (more than 1 tile width)
                if (IsSpaceOccupied(gravePos, 80f)) continue;

                string graveName = (CombatSystem.RandomInt(0, 2) == 0) ? "Grave_1" : "Grave_2";
                CreateStaticObject(graveName, gravePos, _staticObjectSheet, true);
                gravesPlaced++;
            }


            // ==========================================
            // 3. SKELETAL REMAINS CIRCLE (Around Tree)
            // ==========================================
            string[] bones = { "Skull_Pile", "Ribcage", "Bone_In_Floor", "Bird_Skull" };
            int boneCount = 12;
            float radius = 400f;

            for (int i = 0; i < boneCount; i++)
            {
                float angle = i * (MathHelper.TwoPi / boneCount);
                float jitter = CombatSystem.RandomFloat(-0.5f, 0.5f);
                float distJitter = CombatSystem.RandomInt(-50, 50);

                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle + jitter) * (radius + distJitter),
                    (float)Math.Sin(angle + jitter) * (radius + distJitter)
                );
                Vector2 finalPos = centerPos + offset;

                // CHECK: Gap of 60f for bones
                if (IsSpaceOccupied(finalPos, 60f)) continue;

                string boneItem = bones[CombatSystem.RandomInt(0, bones.Length)];
                CreateStaticObject(boneItem, finalPos, _staticObjectSheet, false);
            }


            // ==========================================
            // 4. RANDOM SCATTER (Hands, Dead Trees)
            // ==========================================
            int scatterPlaced = 0;
            attempts = 0;

            while (scatterPlaced < 15 && attempts < 200)
            {
                attempts++;

                int rx = CombatSystem.RandomInt(5, 55);
                int ry = CombatSystem.RandomInt(5, 29);
                Vector2 pos = new Vector2(rx * 64, ry * 64);

                // Don't spawn too close to center tree
                if (Vector2.Distance(pos, centerPos) < 300) continue;

                // CHECK: Increased gap to 100f so random trees don't clump
                if (IsSpaceOccupied(pos, 100f)) continue;

                if (CombatSystem.RandomInt(0, 100) > 50)
                {
                    string randomTree = _treeTypes[CombatSystem.RandomInt(0, _treeTypes.Length)];
                    CreateStaticObject(randomTree, pos, _staticObjectSheet, false);
                }
                else
                {
                    string randomHand = _floorDetails[CombatSystem.RandomInt(0, _floorDetails.Length)];
                    CreateStaticObject(randomHand, pos, _staticObjectSheet, false);
                }


                scatterPlaced++;
            }
        }
        private void CreateStaticObject(string assetName, Vector2 position, Texture2D sheet, bool isSolid)
        {
            Rectangle data = Helper.GetSourceRect(assetName);

            var obj = new WorldObject(_game, sheet, position, 1, isSolid);

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

            return new Vector2((tx * 64) + 32, (ty * 64) + 32);
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