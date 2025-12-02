using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnimatedSprite
{
    class CrossHair : Sprite
    {
        private Game myGame;
        private float CrossHairVelocity = 5.0f;

        public CrossHair(Game g, Texture2D texture, Vector2 position, int frames)
            : base(g, texture, position, frames)
        {
            myGame = g;
        }

        public override void Update(GameTime gametime)
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Right))
                position.X += CrossHairVelocity;
            if (ks.IsKeyDown(Keys.Left))
                position.X -= CrossHairVelocity;
            if (ks.IsKeyDown(Keys.Up))
                position.Y -= CrossHairVelocity;
            if (ks.IsKeyDown(Keys.Down))
                position.Y += CrossHairVelocity;

            // Clamp to screen
            //var vp = myGame.GraphicsDevice.Viewport;
            //position = Vector2.Clamp(position,
            //    Vector2.Zero,
            //    new Vector2(vp.Width - spriteWidth, vp.Height - spriteHeight));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw ONLY ONCE, WITH SCALING
            spriteBatch.Draw(spriteImage, position, null, Color.White, 0f,
                Vector2.Zero, 0.2f, SpriteEffects.None, 0f);
        }
    }
}
