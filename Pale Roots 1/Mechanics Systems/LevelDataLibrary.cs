using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This static class acts as a pure data container for the environment.
    // It feeds specific lists of assets and tile coordinates to the LevelManager, 
    // keeping the actual generation logic clean and separated from the hardcoded data.
    public static class LevelDataLibrary
    {
        // --- ZONE ASSET POOLS ---
        // These arrays group the string keys (which map to the Helper class) by their theme.
        // When the LevelManager decides it wants to spawn something in the "Graveyard" zone, 
        // it randomly picks one string from the GraveObjects array.

        public static readonly string[] NatureObjects = {
            "Dying_Tree", "Medium_Dying_Tree", "Small_Dying_Tree",
            "Brambles_Large", "Brambles_Medium", "Brambles_Small",
            "Brambles_Tiny", "Brambles_Very_Tiny"
        };

        public static readonly string[] GraveObjects = {
            "Grave_1", "Grave_2", "Grave_3",
            "Hand_In_Floor", "Hand_In_Floor_Medium", "Hand_In_Floor_Small"
        };

        public static readonly string[] RuinObjects = {
            "Ruins_Column", "Smaller_Ruin", "Big_Rock", "Shrine_Blue"
        };

        public static readonly string[] BoneObjects = {
            "Skull_Pile", "Ribcage", "Bone_In_Floor", "Bird_Skull",
            "Baby_Skellington"
        };

        // --- TILE PALETTES ---
        // A single giant sprite sheet might contain grass, snow, lava, and dirt.
        // This function builds a specific "Palette" (a restricted list of tiles) for a given level,
        // so the procedural floor generator only uses grass for Level 0, and doesn't accidentally spawn a random lava tile.
        public static List<TileRef> GetLevelPalette(int levelIndex)
        {
            List<TileRef> palette = new List<TileRef>();

            // Level 0: The starting Graveyard/Forest area.
            if (levelIndex == 0)
            {
                // Grab a specific solid block of grass/dirt tiles from the master sprite sheet.
                // The X and Y values correspond to the exact grid cells on the image file.
                for (int y = 41; y <= 48; y++)
                {
                    for (int x = 13; x <= 17; x++)
                    {
                        palette.Add(new TileRef(x, y, 0));
                    }
                }

                // Append a few specific edge/corner variations to the palette to break up repeating patterns.
                int[] edgeRows = { 42, 43, 46, 47, 48 };
                foreach (int y in edgeRows)
                {
                    for (int x = 9; x <= 12; x++)
                    {
                        palette.Add(new TileRef(x, y, 0));
                    }
                }
            }

            // If you add Level 1 (like a dungeon or castle) later, you just add an 'else if (levelIndex == 1)' here
            // and define the X/Y coordinates for the stone floor tiles!

            return palette;
        }
    }
}