using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnimatedSprite
{
    public class PlayerWithWeapon : RotatingSprite
    {
        protected Game myGame;
        protected float playerVelocity = 6.0f;
        private Projectile myProjectile;
        protected CrossHair Site;

        public float MaxHealth = 100;
        public float CurrentHealth = 100;
        Texture2D healthTexture;


        public Vector2 CentrePos
        {
            get { return position + new Vector2(spriteWidth / 2, spriteHeight / 2); }
        }

        public Projectile MyProjectile
        {
            get { return myProjectile; }
            set { myProjectile = value; }
        }

        public PlayerWithWeapon(Game g, Texture2D texture, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            myGame = g;
            Site = new CrossHair(g, g.Content.Load<Texture2D>("CrossHair"), userPosition, 1);


            healthTexture = new Texture2D(g.GraphicsDevice, 1, 1);
            healthTexture.SetData(new[] { Color.White });
        }

        public void loadProjectile(Projectile r)
        {
            MyProjectile = r;
        }

        public override void Update(GameTime gameTime)
        {
            Viewport gameScreen = myGame.GraphicsDevice.Viewport;

            if (Keyboard.GetState().IsKeyDown(Keys.D)) this.position.X += playerVelocity;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) this.position.X -= playerVelocity;
            if (Keyboard.GetState().IsKeyDown(Keys.W)) this.position.Y -= playerVelocity;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) this.position.Y += playerVelocity;

            position = Vector2.Clamp(position, Vector2.Zero,
                new Vector2(gameScreen.Width - spriteWidth, gameScreen.Height - spriteHeight));

            MouseState ms = Mouse.GetState();
            Vector2 mousePosition = new Vector2(ms.X, ms.Y);

            this.angleOfRotation = TurnToFace(this.CentrePos, mousePosition, this.angleOfRotation, 0.15f);

            if (MyProjectile != null)
            {
                if (MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL)
                {
                    MyProjectile.position = this.CentrePos;
                    MyProjectile.angleOfRotation = this.angleOfRotation;

                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        MyProjectile.fire(mousePosition);
                    }
                }
                MyProjectile.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (MyProjectile != null && MyProjectile.ProjectileState != Projectile.PROJECTILE_STATE.STILL)
            {
                MyProjectile.Draw(spriteBatch);
            }
            spriteBatch.Begin();
            int barWidth = 50;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - (spriteHeight / 2) - 10;


            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            
            int currentBarWidth = (int)(barWidth * (CurrentHealth / MaxHealth));
            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.Green);

            spriteBatch.End();

        }
    }
}