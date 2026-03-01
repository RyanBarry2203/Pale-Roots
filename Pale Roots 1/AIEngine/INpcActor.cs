using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Interface for characters that are driven by the AI state machine.
    // Implement this for any NPC that needs autonomous behavior.
    // Inherits from ICombatant so NPCs participate in targeting and damage systems.
    public interface INpcActor : ICombatant
    {
        // Properties the AI reads and modifies for movement and attacks.
        float Velocity { get; set; }
        Vector2 StartPosition { get; set; }
        Vector2 WanderTarget { get; set; }
        float AttackCooldown { get; set; }
        float AttackRange { get; }

        // Exposes the current AI state and allows the AI to switch behaviors.
        IAIState CurrentState { get; }
        void ChangeState(IAIState newState);

        // Helpers for animation and movement used by AI states.
        void PlayAnimation(string key);
        void MoveToward(Vector2 target, float speed, List<WorldObject> obstacles);

        // Instantly rotate the sprite to face a specific point.
        void SnapToFace(Vector2 target);
    }
}