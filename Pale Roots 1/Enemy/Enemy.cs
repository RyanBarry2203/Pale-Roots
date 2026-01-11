using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Enemy : RotatingSprite
    {
        public enum ENEMYSTATE { ALIVE, DYING, DEAD };
        public enum AISTATE { Charging, Chasing, InCombat, Wandering };

        private ENEMYSTATE enemyState = ENEMYSTATE.ALIVE;
        public ENEMYSTATE EnemyStateza { get { return enemyState; } set { enemyState = value; } }

        protected float Velocity = 3.0f;
        protected Vector2 startPosition;
        protected Vector2 TargetPosition;
        public int countDown = 100;

        public Enemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            startPosition = userPosition;
            CurrentAIState = AISTATE.Charging;
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);

            if (enemyState == ENEMYSTATE.ALIVE)
            {
                switch (CurrentAIState)
                {
                    case AISTATE.Charging:
                        // Basic movement is handled in the Engine charge logic
                        break;
                    case AISTATE.Chasing:
                        if (CurrentCombatPartner != null)
                        {
                            MoveTowards(CurrentCombatPartner.Center);
                            if (Vector2.Distance(Center, CurrentCombatPartner.Center) < 70f)
                                CurrentAIState = AISTATE.InCombat;
                        }
                        break;
                    case AISTATE.InCombat:
                        PerformCombat(gametime);
                        break;
                    case AISTATE.Wandering:
                        PerformWander();
                        break;
                }
            }
            else if (enemyState == ENEMYSTATE.DYING)
            {
                countDown--;
                if (countDown <= 0) enemyState = ENEMYSTATE.DEAD;
            }
        }

        private void PerformCombat(GameTime gameTime)
        {
            if (CurrentCombatPartner == null || !CurrentCombatPartner.Visible)
            {
                CurrentAIState = AISTATE.Wandering;
                return;
            }

            // Face the target
            Vector2 dir = CurrentCombatPartner.Center - Center;
            angleOfRotation = (float)Math.Atan2(dir.Y, dir.X);

            // Attack Cooldown logic
            if (AttackCooldown > 0)
                AttackCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            else
            {
                if (Vector2.Distance(Center, CurrentCombatPartner.Center) < 85f)
                {
                    // Partner.Health -= damage; (Future implementation)
                    AttackCooldown = AttackSpeed;
                }
            }

            if (Vector2.Distance(Center, CurrentCombatPartner.Center) > 100f)
                CurrentAIState = AISTATE.Chasing;
        }

        protected void MoveTowards(Vector2 target)
        {
            Vector2 direction = target - position;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                position += direction * Velocity;
                angleOfRotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        private void PerformWander()
        {
            if (Vector2.Distance(position, TargetPosition) < 5f || TargetPosition == Vector2.Zero)
            {
                Random rand = new Random();
                TargetPosition = startPosition + new Vector2(rand.Next(-100, 101), rand.Next(-100, 101));
            }
            MoveTowards(TargetPosition);
        }
    }
}