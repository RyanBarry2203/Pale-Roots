using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class CircularChasingEnemy : Enemy
    {
        // Fixed typo: chaseRdaius -> chaseRadius
        public float chaseRadius = 200;
        bool FullOnChase = false;

        public float myVelocity { get { return base.Velocity; } set { base.Velocity = value; } }

        public CircularChasingEnemy(Game g, Texture2D texture, Vector2 Position1, int framecount)
             : base(g, texture, Position1, framecount)
        {
            startPosition = Position1;
            this.Velocity = 2.0f;
        }

        // UPDATED: Now accepts generic 'Sprite' so it can follow Player OR Allies
        public void follow(Sprite target)
        {
            float stopDistance = 60f; // The "border" distance for swinging weapons
            float distance = Vector2.Distance(this.Center, target.Center);

            // Only move if we aren't at the "Battle Line" yet
            if (distance > stopDistance)
            {
                Vector2 direction = target.Center - this.Center;
                if (direction != Vector2.Zero)
                    direction.Normalize();

                this.position += direction * Velocity;
            }
            else
            {
                // Transition to InCombat once the distance is closed
                this.CurrentAIState = Enemy.AISTATE.InCombat;
            }
        }

        // UPDATED: Checks distance to any Sprite
        public bool inChaseZone(Sprite target)
        {
            float distance = Vector2.Distance(this.Center, target.Center);
            if (distance <= chaseRadius)
                return true;
            return false;
        }
    }
}