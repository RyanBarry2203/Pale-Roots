using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace AnimatedSprite
{
    class Enemy : Sprite
    {
        public enum ENEMYSTATE { ALIVE, DYING, DEAD };
        private ENEMYSTATE enemyState;

        public ENEMYSTATE EnemyState
        {
            get { return enemyState; }
            set { enemyState = value; }
        }
        protected Game myGame;
        protected float Velocity = 4.0f;
        protected Vector2 startPosition;
        protected Vector2 TargetPosition;

        public bool alive;
        public int countDown = 100;

        public Enemy(Game g, Texture2D texture, Vector2 userPosition, int framecount) : base(g,texture,userPosition,framecount)
        {
            // Need to see the Game to access Viewport
            enemyState = ENEMYSTATE.ALIVE;
            myGame = g; 
            startPosition = userPosition;
        }

       

        public override void Update(GameTime gametime)
        {
            switch (enemyState)
            {
                case ENEMYSTATE.ALIVE:
                    break;
                case ENEMYSTATE.DYING:
                    countDown --;
                    if(countDown < 0)
                        enemyState = ENEMYSTATE.DEAD;
                    break;
                case ENEMYSTATE.DEAD:
                    countDown = 100;
                    Visible=false;
                    break;
            }
                
            base.Update(gametime);
        }

        public void die()
        {
            enemyState = ENEMYSTATE.DYING;
        }
        
    }
}
