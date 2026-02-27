using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This is the essential bridge between our AI state machine and the physical entities in the game.
    // Any character that needs an independent brain (like an Enemy or an Ally) must implement this interface.
    // Because it inherits from ICombatant, any NPC Actor is automatically hooked into the targeting and damage systems.
    public interface INpcActor : ICombatant
    {
        // Core variables that the AI states need to read and modify to handle movement, patrolling, and attack pacing.
        float Velocity { get; set; }
        Vector2 StartPosition { get; set; }
        Vector2 WanderTarget { get; set; }
        float AttackCooldown { get; set; }
        float AttackRange { get; }

        // Gives the AI states a way to check what the NPC is currently doing and explicitly force them into a new behavior.
        IAIState CurrentState { get; }
        void ChangeState(IAIState newState);

        void PlayAnimation(string key);
        void MoveToward(Vector2 target, float speed, List<WorldObject> obstacles);

        // A quick helper function to force the NPC's sprite to instantly turn and look at a specific point, 
        void SnapToFace(Vector2 target);
    }
}