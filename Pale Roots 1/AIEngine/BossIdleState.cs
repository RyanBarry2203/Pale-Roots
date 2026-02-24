using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class BossIdleState : IAIState
    {
        public void Enter(INpcActor npc)
        {
            // Optional: Play a roar sound?
        }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {

            if (npc.CurrentTarget != null)
            {
                npc.SnapToFace(npc.CurrentTarget.Position);
            }

        }

        public void Exit(INpcActor npc) { }
    }
}