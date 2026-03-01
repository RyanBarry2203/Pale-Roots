using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Interface for AI behavior states used by NPCs.
    // Implementations provide enter, update, and exit logic for a behavior.
    public interface IAIState
    {
        // Called once when the NPC enters this state.
        void Enter(INpcActor npc);

        // Called each frame to run the state's logic.
        // The state can use gameTime for timers and obstacles for pathfinding.
        void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles);

        // Called once before leaving this state to perform any cleanup.
        void Exit(INpcActor npc);
    }
}