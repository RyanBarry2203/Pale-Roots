using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // NPC charges straight in the direction of its team.
    public class ChargeState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // Choose direction based on npc team.
            float dir = npc.Team == CombatTeam.Player ? 1f : -1f;

            // Set a distant target along the x axis.
            Vector2 target = new Vector2(npc.Position.X + (1000 * dir), npc.Position.Y);

            // Move toward the distant target while avoiding obstacles.
            npc.MoveToward(target, npc.Velocity, obstacles);
        }

        public void Exit(INpcActor npc) { }
    }

    // Pursue a valid target until it is lost or within attack range.
    public class ChaseState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // If there is no valid target, switch to wandering.
            if (npc.CurrentTarget == null || !npc.CurrentTarget.IsAlive)
            {
                npc.ChangeState(new WanderState());
                return;
            }

            // Move toward the target center avoiding obstacles.
            npc.MoveToward(npc.CurrentTarget.Center, npc.Velocity, obstacles);

            // If within attack range according to CombatSystem, switch to combat.
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) < npc.AttackRange)
            {
                npc.ChangeState(new CombatState());
            }
        }

        public void Exit(INpcActor npc) { }
    }

    // Fight the current target and manage attack timing and facing.
    public class CombatState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // If the target is gone or dead, return to wandering.
            if (npc.CurrentTarget == null || !npc.CurrentTarget.IsAlive)
            {
                npc.ChangeState(new WanderState());
                return;
            }

            // Face the target immediately.
            npc.SnapToFace(npc.CurrentTarget.Center);

            // If in range and the attack cooldown has reset, perform the attack.
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) < npc.AttackRange && npc.AttackCooldown <= 0)
            {
                npc.PerformAttack();
            }

            // If the target moves far enough away, switch back to chasing.
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) > npc.AttackRange + 30f)
            {
                npc.ChangeState(new ChaseState());
            }
        }

        public void Exit(INpcActor npc) { }
    }

    // Patrol near the spawn point by choosing random nearby targets.
    public class WanderState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // If no wander target is set or the npc is close to it, pick a new one.
            if (npc.WanderTarget == Vector2.Zero || Vector2.Distance(npc.Position, npc.WanderTarget) < 5f)
            {
                // Use CombatSystem random to compute a new wander target within the radius.
                npc.WanderTarget = npc.StartPosition + new Vector2(
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1),
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1)
                );
            }

            // Move slowly toward the wander target at half speed.
            npc.MoveToward(npc.WanderTarget, npc.Velocity * 0.5f, obstacles);
        }

        public void Exit(INpcActor npc) { }
    }

    // Apply a short stun and play the hurt animation when damaged.
    public class HurtState : IAIState
    {
        public void Enter(INpcActor npc)
        {
            // Set attack cooldown as a stun and play hurt animation.
            npc.AttackCooldown = 500f;
            npc.PlayAnimation("Hurt");
        }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // When the cooldown expires, return to the chase state.
            if (npc.AttackCooldown <= 0) npc.ChangeState(new ChaseState());
        }

        public void Exit(INpcActor npc) { }
    }
}