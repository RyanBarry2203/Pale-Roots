using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public interface IAIState
    {
        void Enter(INpcActor npc);
        void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles);
        void Exit(INpcActor npc);
    }
}