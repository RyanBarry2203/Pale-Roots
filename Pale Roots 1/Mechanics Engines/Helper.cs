using Microsoft.Xna.Framework;
using Microsoft;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public static class  Helper
    {
        public static Texture2D SpriteSheet { get; set; }

        public static Dictionary<string, Rectangle> SourceRects = new Dictionary<string, Rectangle>()
        {
            { "Big_Rock", new Rectangle(16, 192, 64, 64)},
            { "Skull_Pile", new Rectangle(15, 433, 80, 60)},

            { "Tree_Dead_Large", new Rectangle(16, 16, 112, 128)},

            { "HealthBar_Border", new Rectangle(0, 0, 1, 1)},

            { "Ruins_Column", new Rectangle(8, 16, 87, 96) },

            { "Shrine_Blue", new Rectangle(0, 128, 32, 48) },
            { "Bird_Skull", new Rectangle(16, 332, 51, 52) },
            { "Skellington", new Rectangle(64, 320, 159, 119) },
            { "Bone_In_Floor", new Rectangle(560, 432, 31, 50) },
            { "Dying_Tree", new Rectangle(415, 496, 69, 93) },
            { "Hand_In_Floor", new Rectangle(608, 523, 63, 71) },
            { "Baby_Skellington", new Rectangle(239, 227, 32, 24) },

            { "Grave_1", new Rectangle(271, 683, 32, 40) },
            { "Grave_2", new Rectangle(703, 684, 33, 33) },

            { "Smaller_Ruin", new Rectangle(108, 40, 87, 73) },
            { "Ribcage", new Rectangle(317, 221, 61, 39) },


        };

        public static Rectangle GetSourceRect(string key)
        {
            if (SourceRects.ContainsKey(key))
            {
                return SourceRects[key];
            }
            return new Rectangle(0, 0, 32, 32);

        }
    }
}