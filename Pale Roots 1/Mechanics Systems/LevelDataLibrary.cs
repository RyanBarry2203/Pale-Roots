using System.Collections.Generic;

namespace Pale_Roots_1
{
    public static class LevelDataLibrary
    {
        // 1. Store your Zone Objects here instead of the Manager
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

        // 2. Custom API to fetch a level's tile palette
        public static List<TileRef> GetLevelPalette(int levelIndex)
        {
            List<TileRef> palette = new List<TileRef>();

            if (levelIndex == 0)
            {
                // Level 0 (Graveyard) Palette
                for (int y = 41; y <= 48; y++)
                {
                    for (int x = 13; x <= 17; x++)
                    {
                        palette.Add(new TileRef(x, y, 0));
                    }
                }

                // Add the specific edge/corner tiles
                int[] edgeRows = { 42, 43, 46, 47, 48 };
                foreach (int y in edgeRows)
                {
                    for (int x = 9; x <= 12; x++)
                    {
                        palette.Add(new TileRef(x, y, 0));
                    }
                }
            }
            // If you add Level 1 later, you just add an 'else if (levelIndex == 1)' here!

            return palette;
        }
    }
}