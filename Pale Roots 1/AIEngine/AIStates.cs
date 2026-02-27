using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Handles the logic for when an NPC is blindly charging forward.
    public class ChargeState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // Check the NPC's team to figure out which direction they should run. 
            // Player allies move right (+1), enemies move left (-1).
            float dir = npc.Team == CombatTeam.Player ? 1f : -1f;

            // Pick a point way off in the distance in the correct direction.
            Vector2 target = new Vector2(npc.Position.X + (1000 * dir), npc.Position.Y);

            // Tell the NPC to move towards that distant point while avoiding obstacles.
            npc.MoveToward(target, npc.Velocity, obstacles);
        }

        public void Exit(INpcActor npc) { }
    }

    // Handles the logic for when an NPC is actively pursuing a target.
    public class ChaseState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // First, make sure we actually have a valid target. 
            // If the target disappeared or died, drop out of the chase and go back to wandering.
            if (npc.CurrentTarget == null || !npc.CurrentTarget.IsAlive)
            {
                npc.ChangeState(new WanderState());
                return;
            }

            // Keep moving toward the center of the target.
            npc.MoveToward(npc.CurrentTarget.Center, npc.Velocity, obstacles);

            // Ask the CombatSystem to check the distance between us and the target.
            // If we close the gap and get within engagement range, switch to the CombatState.
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) < GameConstants.CombatEngageRange)
            {
                npc.ChangeState(new CombatState());
            }
        }

        public void Exit(INpcActor npc) { }
    }

    // Handles the logic for when an NPC is actively fighting their target.
    public class CombatState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // Safety check: if our target dies or vanishes mid-fight, go back to wandering.
            if (npc.CurrentTarget == null || !npc.CurrentTarget.IsAlive)
            {
                npc.ChangeState(new WanderState());
                return;
            }

            // Make sure the NPC's sprite is always looking directly at the target.
            npc.SnapToFace(npc.CurrentTarget.Center);

            // Check with the CombatSystem to see if we are close enough to swing.
            // If we are, and our attack cooldown has reset, hit them.
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) < npc.AttackRange && npc.AttackCooldown <= 0)
            {
                npc.PerformAttack();
            }

            // If the target manages to run away and gets outside our combat break range, 
            // stop fighting and go back to chasing them down.
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) > GameConstants.CombatBreakRange)
            {
                npc.ChangeState(new ChaseState());
            }
        }

        public void Exit(INpcActor npc) { }
    }

    // Handles the logic for when an NPC is idle and patrolling around their spawn point.
    public class WanderState : IAIState
    {
        public void Enter(INpcActor npc) { }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // If we haven't picked a spot to wander to yet, or if we just arrived at our current spot (within 5 pixels).
            if (npc.WanderTarget == Vector2.Zero || Vector2.Distance(npc.Position, npc.WanderTarget) < 5f)
            {
                // Use the CombatSystem's random number generator and the radius defined in GameConstants 
                // to pick a new random point near where the NPC originally spawned.
                npc.WanderTarget = npc.StartPosition + new Vector2(
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1),
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1)
                );
            }

            // Slowly walk toward the newly chosen spot at half speed.
            npc.MoveToward(npc.WanderTarget, npc.Velocity * 0.5f, obstacles);
        }

        public void Exit(INpcActor npc) { }
    }

    // Handles the logic for when an NPC takes damage and gets momentarily stunned.
    public class HurtState : IAIState
    {
        public void Enter(INpcActor npc)
        {
            // The moment we enter this state, force a long attack cooldown to act as a stun mechanic.
            // Also trigger the hurt animation so the player gets visual feedback.
            npc.AttackCooldown = 500f;
            npc.PlayAnimation("Hurt");
        }

        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // Wait for the stun (attack cooldown) to count down to zero.
            // Once they recover, immediately switch back to chasing whatever hit them.
            if (npc.AttackCooldown <= 0) npc.ChangeState(new ChaseState());
        }

        public void Exit(INpcActor npc) { }
    }
}