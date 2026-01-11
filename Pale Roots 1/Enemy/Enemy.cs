using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class Enemy : RotatingSprite
    {
        public enum ENEMYSTATE { ALIVE, DYING, DEAD };
        public enum AISTATE { Charging, Chasing, InCombat, Wandering };

        private ENEMYSTATE enemyState;
        public AISTATE CurrentAIState = AISTATE.Wandering;

        public Sprite CurrentCombatPartner;

        public ENEMYSTATE EnemyStateza
        {
            get { return enemyState; }
            set { enemyState = value; }
        }

        protected Game myGame;
        protected float Velocity = 4.0f;
        protected Vector2 startPosition;
        protected Vector2 TargetPosition;
        public int countDown = 100;

        public Enemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            enemyState = ENEMYSTATE.ALIVE;
            myGame = g;
            startPosition = userPosition;
            TargetPosition = userPosition;
        }

        public override void Update(GameTime gametime)
        {
            // 1. Handle animations from the base Sprite class
            base.Update(gametime);

            if (EnemyStateza == ENEMYSTATE.ALIVE)
            {
                // Check if target is close enough to start chasing
                CheckForTarget();

                switch (CurrentAIState)
                {
                    case AISTATE.Wandering:
                        PerformWander();
                        break;
                    case AISTATE.Chasing:
                        PerformChase();
                        break;
                    case AISTATE.Charging:
                        PerformCharge();
                        break;
                    case AISTATE.InCombat:
                        PerformCombat();
                        break;
                }
            }
            else if (EnemyStateza == ENEMYSTATE.DYING)
            {
                countDown--;
                if (countDown <= 0) EnemyStateza = ENEMYSTATE.DEAD;
            }
        }

        public void die()
        {
            enemyState = ENEMYSTATE.DYING;
        }

        private void PerformWander()
        {
            if (Vector2.Distance(position, TargetPosition) < 5f || TargetPosition == Vector2.Zero)
            {
                Random rand = new Random();
                TargetPosition = startPosition + new Vector2(rand.Next(-200, 201), rand.Next(-200, 201));
            }

            MoveTowards(TargetPosition);
        }

        private void PerformChase()
        {
            if (CurrentCombatPartner != null)
            {
                MoveTowards(CurrentCombatPartner.Center);
                if (Vector2.Distance(this.Center, CurrentCombatPartner.Center) < 60f)
                {
                    CurrentAIState = AISTATE.InCombat;
                }
            }
        }

        private void PerformCharge()
        {
            if (CurrentCombatPartner != null)
            {
                float originalVelocity = Velocity;
                Velocity = 8.0f; // Double speed for charging
                MoveTowards(CurrentCombatPartner.Center);
                Velocity = originalVelocity;
            }
        }

        private void PerformCombat()
        {
            if (CurrentCombatPartner != null && Vector2.Distance(this.Center, CurrentCombatPartner.Center) > 80f)
            {
                CurrentAIState = AISTATE.Chasing;
            }
            // Add damage logic here later
        }

        private void MoveTowards(Vector2 target)
        {
            Vector2 direction = target - position;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                position += direction * Velocity;

                // Update the inherited rotation from RotatingSprite
                angleOfRotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        private void CheckForTarget()
        {
            if (CurrentCombatPartner != null)
            {
                float distance = Vector2.Distance(this.Center, CurrentCombatPartner.Center);
                if (distance < 300f)
                {
                    if (CurrentAIState == AISTATE.Wandering) CurrentAIState = AISTATE.Chasing;
                }
                else
                {
                    CurrentAIState = AISTATE.Wandering;
                }
            }
        }
    }
}