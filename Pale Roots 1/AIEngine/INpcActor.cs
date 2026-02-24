using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // The master contract for any AI-controlled entity in your engine
    public interface INpcActor : ICombatant
    {
        float Velocity { get; set; }
        Vector2 StartPosition { get; set; }
        Vector2 WanderTarget { get; set; }
        float AttackCooldown { get; set; }

        IAIState CurrentState { get; }
        void ChangeState(IAIState newState);
        void PlayAnimation(string key);
        void MoveToward(Vector2 target, float speed, List<WorldObject> obstacles);
        void SnapToFace(Vector2 target);
    }
}