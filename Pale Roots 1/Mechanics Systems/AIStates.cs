using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class ChargeState : IAIState
    {
        public void Enter(INpcActor npc) { }
        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            // Enemies charge left (-1), Allies charge right (+1)
            float dir = npc.Team == CombatTeam.Player ? 1f : -1f;
            Vector2 target = new Vector2(npc.Position.X + (1000 * dir), npc.Position.Y);
            npc.MoveToward(target, npc.Velocity, obstacles);
        }
        public void Exit(INpcActor npc) { }
    }

    public class ChaseState : IAIState
    {
        public void Enter(INpcActor npc) { }
        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            if (npc.CurrentTarget == null || !npc.CurrentTarget.IsAlive)
            {
                npc.ChangeState(new WanderState());
                return;
            }
            npc.MoveToward(npc.CurrentTarget.Center, npc.Velocity, obstacles);
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) < GameConstants.CombatEngageRange)
            {
                npc.ChangeState(new CombatState());
            }
        }
        public void Exit(INpcActor npc) { }
    }

    public class CombatState : IAIState
    {
        public void Enter(INpcActor npc) { }
        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            if (npc.CurrentTarget == null || !npc.CurrentTarget.IsAlive)
            {
                npc.ChangeState(new WanderState());
                return;
            }
            npc.SnapToFace(npc.CurrentTarget.Center);
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) < GameConstants.MeleeAttackRange && npc.AttackCooldown <= 0)
            {
                npc.PerformAttack();
            }
            if (CombatSystem.GetDistance(npc, npc.CurrentTarget) > GameConstants.CombatBreakRange)
            {
                npc.ChangeState(new ChaseState());
            }
        }
        public void Exit(INpcActor npc) { }
    }

    public class WanderState : IAIState
    {
        public void Enter(INpcActor npc) { }
        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            if (npc.WanderTarget == Vector2.Zero || Vector2.Distance(npc.Position, npc.WanderTarget) < 5f)
            {
                npc.WanderTarget = npc.StartPosition + new Vector2(
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1),
                    CombatSystem.RandomInt(-GameConstants.WanderRadius, GameConstants.WanderRadius + 1)
                );
            }
            npc.MoveToward(npc.WanderTarget, npc.Velocity * 0.5f, obstacles);
        }
        public void Exit(INpcActor npc) { }
    }

    public class HurtState : IAIState
    {
        public void Enter(INpcActor npc)
        {
            npc.AttackCooldown = 500f; // Brief stun
            npc.PlayAnimation("Hurt");
        }
        public void Update(INpcActor npc, GameTime gameTime, List<WorldObject> obstacles)
        {
            if (npc.AttackCooldown <= 0) npc.ChangeState(new ChaseState());
        }
        public void Exit(INpcActor npc) { }
    }
}