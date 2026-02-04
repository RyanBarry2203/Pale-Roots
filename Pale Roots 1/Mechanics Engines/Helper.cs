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
            { "Rocks_Grey_Small", new Rectangle(0, 96, 32, 32)},
            { "Rock_Mossy_Small", new Rectangle(32, 96, 32, 32)},

            { "Tree_Dead_Large ", new Rectangle(0, 0, 96, 96)},
            { "Tree_Green_Large", new Rectangle(0, 192, 96, 96)},

            { "HealthBar_Border", new Rectangle(0, 0, 1, 1)},

            { "Ruins_Column", new Rectangle(0, 0, 32, 64) },
            { "Ruins_Wall",   new Rectangle(48, 0, 48, 48) },
            { "Bush_Green",   new Rectangle(0, 160, 16, 16) }, // Small bush

            { "Shrine_Blue", new Rectangle(0, 128, 32, 48) },
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