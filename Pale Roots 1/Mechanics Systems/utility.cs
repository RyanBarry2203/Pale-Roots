using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pale_Roots_1
{
    // Provides simple random integer helpers backed by a single Random instance.
    static public class Utility
    {
        // Shared RNG used by all utility methods.
        static Random r = new Random();

        // Return a random integer in the range [0, max).
        public static int NextRandom(int max)
        {
            return r.Next(max);
        }

        // Return a random integer in the range [min, max).
        public static int NextRandom(int min, int max)
        {
            return r.Next(min,max);

        }
    }
}
