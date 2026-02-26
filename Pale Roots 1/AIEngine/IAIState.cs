using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This is the core contract for our Finite State Machine. 
    // Any behavior we want an NPC to have (like Wander, Chase, or Combat) has to implement this interface.
    // It allows the NPC's brain to easily swap out what it's currently doing without rewriting a massive, messy switch statement.
    public interface IAIState
    {
        // Triggered exactly once the moment an NPC switches into this state.
        void Enter(INpcActor npc);

        // The main brain loop for this specific behavior.
        // We pass in the game time for tracking timers, and the list of obstacles so the state can handle its own pathfinding and steering.
        void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles);

        // Triggered exactly once right before the NPC transitions out of this state to a new one.
        // This is basically our cleanup phase—used to reset cooldowns, drop old targets, or stop looping particle effects.
        void Exit(INpcActor npc);
    }
}