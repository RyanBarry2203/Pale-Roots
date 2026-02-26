using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This acts as the initial waiting phase for the boss before the fight officially kicks off.
    // It just sits there in the center of the room, tracking the player until another script forces it into a ChaseState.
    public class BossIdleState : IAIState
    {
        public void Enter(INpcActor npc)
        {
        }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // Even though the boss isn't attacking or chasing yet, we want it to look menacing by tracking the player.
            // If the combat system has assigned a target (the player), we force the boss's sprite to continually turn and face them.
            if (npc.CurrentTarget != null)
            {
                npc.SnapToFace(npc.CurrentTarget.Position);
            }

        }

        public void Exit(INpcActor npc) { }
    }
}