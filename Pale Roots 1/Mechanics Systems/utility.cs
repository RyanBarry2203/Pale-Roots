using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pale_Roots_1
{
    // A globally accessible helper class for generating random numbers.
    // Keeping this separate allows any class in the game (like particle systems or loot drops) 
    // to easily grab a random value without needing to instantiate their own generator.
    static public class Utility
    {
        // By making the Random instance 'static', it is created exactly once when the game boots up.
        // This prevents the common C# bug where rapidly creating new Random() objects in a loop 
        // generates the exact same sequence of numbers because they share the same clock-based seed.
        static Random r = new Random();

        // Returns a random integer starting from 0 up to (but not including) the max value.
        // Example: NextRandom(10) will return a number between 0 and 9.
        public static int NextRandom(int max)
        {
            return r.Next(max);
        }

        // Returns a random integer within a specific bounded range.
        // Example: NextRandom(5, 10) will return a number between 5 and 9.
        public static int NextRandom(int min, int max)
        {
            return r.Next(min, max);
        }
    }
}