using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Boss waits in place and faces the player when a target is assigned.
    public class BossIdleState : IAIState
    {
        public void Enter(INpcActor npc)
        {
            // No setup is required when entering idle.
        }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // Face the current target if one is assigned.
            if (npc.CurrentTarget != null)
            {
                npc.SnapToFace(npc.CurrentTarget.Position);
            }

        }

        public void Exit(INpcActor npc) 
        {
            // No cleanup is required when exiting idle.
        }
    }
}