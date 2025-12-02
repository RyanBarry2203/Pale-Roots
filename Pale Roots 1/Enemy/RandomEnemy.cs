using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sprites
{
    public class RandomEnemy : Enemy
    {
        private Vector2 TargetPosition;

        public RandomEnemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            TargetPosition = CreateTarget();
        }

        private Vector2 CreateTarget()
        {
            Random r = new Random();

            int rx = r.Next(myGame.GraphicsDevice.Viewport.Width - spriteImage.Width);
            int ry = r.Next(myGame.GraphicsDevice.Viewport.Height - spriteImage.Height);

            return new Vector2(rx, ry);
        }

        public override void Update(GameTime gameTime)
        {
            // Move (LERP) towards the target
            position = Vector2.Lerp(position, TargetPosition, 0.05f);

            // If close to target, pick a new one
            if (Vector2.Distance(position, TargetPosition) < 1)
            {
                position = TargetPosition;
                TargetPosition = CreateTarget();
            }

            base.Update(gameTime);
        }
    }
}

