using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Provides static data used when the LevelManager procedurally places scenery and builds tile palettes.
    public static class LevelDataLibrary
    {
        // Named nature assets the LevelManager can spawn when decorating the map.
        public static readonly string[] NatureObjects = {
            "Dying_Tree", "Medium_Dying_Tree", "Small_Dying_Tree",
            "Brambles_Large", "Brambles_Medium", "Brambles_Small",
            "Brambles_Tiny", "Brambles_Very_Tiny"
        };

        // Named grave-themed assets for cemetery zones.
        public static readonly string[] GraveObjects = {
            "Grave_1", "Grave_2", "Grave_3",
            "Hand_In_Floor", "Hand_In_Floor_Medium", "Hand_In_Floor_Small"
        };

        // Named ruin assets the LevelManager can place in ruin zones.
        public static readonly string[] RuinObjects = {
            "Ruins_Column", "Smaller_Ruin", "Big_Rock", "Shrine_Blue"
        };

        // Named bone-related assets used near the central landmarks.
        public static readonly string[] BoneObjects = {
            "Skull_Pile", "Ribcage", "Bone_In_Floor", "Bird_Skull",
            "Baby_Skellington"
        };

        // Returns a list of TileRef entries that form the tileset palette for a given level.
        // The returned palette is consumed by TileLayer when constructing the CurrentLevel.
        public static List<TileRef> GetLevelPalette(int levelIndex)
        {
            List<TileRef> palette = new List<TileRef>();

            if (levelIndex == 0)
            {
                // Build the graveyard palette by adding specific tiles from the tilesheet.
                for (int y = 41; y <= 48; y++)
                {
                    for (int x = 13; x <= 17; x++)
                    {
                        palette.Add(new TileRef(x, y, 0));
                    }
                }

                // Add edge and corner tile references to complete the palette layout.
                int[] edgeRows = { 42, 43, 46, 47, 48 };
                foreach (int y in edgeRows)
                {
                    for (int x = 9; x <= 12; x++)
                    {
                        palette.Add(new TileRef(x, y, 0));
                    }
                }
            }

            // Add more levelIndex branches here to support additional level palettes.
            return palette;
        }
    }
}