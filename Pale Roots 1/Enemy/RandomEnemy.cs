using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class RandomEnemy : Enemy
    {
        // Note: TargetPosition already exists in Enemy base class, 
        // so we use 'new' to hide it (or just remove this if you want to use the base one)
        public new Vector2 TargetPosition;

        public RandomEnemy(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            TargetPosition = CreateTarget();
        }

        public Vector2 CreateTarget()
        {
            Random r = new Random();

            // FIX: Changed 'myGame' to 'game' - which is the protected field in Sprite base class
            int rx = r.Next(game.GraphicsDevice.Viewport.Width - spriteImage.Width);
            int ry = r.Next(game.GraphicsDevice.Viewport.Height - spriteImage.Height);

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