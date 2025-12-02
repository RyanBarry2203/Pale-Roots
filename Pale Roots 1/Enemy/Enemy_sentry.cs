using AnimatedSprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP01Week10Lab2_2025
{
    internal class Enemy_sentry : RotatingSprite
    {
        public Projectile MyProjectile { get; set; }
        float detectionRadius = 400f;
        float reloadTimer = 0;
        float timeToReload = 2000f;

 
        public float MaxHealth = 100;
        public float CurrentHealth = 100;
        Texture2D healthTexture;


        public Enemy_sentry(Game g, Texture2D tx, Vector2 StartPosition, int NoOfFrames)
            : base(g, tx, StartPosition, NoOfFrames)
        {
            this.rotationSpeed = 0.15f;


            healthTexture = new Texture2D(g.GraphicsDevice, 1, 1);
            healthTexture.SetData(new[] { Color.White });

        }

        public void LoadProjectile(Projectile p)
        {
            MyProjectile = p;
        }

        public void UpdateSentry(GameTime gameTime, PlayerWithWeapon p)
        {
            if (reloadTimer > 0) reloadTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            float distance = Vector2.Distance(this.WorldOrigin, p.CentrePos);

            if (distance < detectionRadius)
            {
                base.follow(p);

                if (MyProjectile != null && MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL && reloadTimer <= 0)
                {
                    MyProjectile.fire(p.CentrePos);
                    reloadTimer = timeToReload;
                }
            }

            if (MyProjectile != null)
            {
                if (MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL)
                {
                    MyProjectile.position = this.position;

                }
                MyProjectile.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (MyProjectile != null)
            {
                MyProjectile.Draw(spriteBatch);
            }


            spriteBatch.Begin();

            int barWidth = spriteWidth; // Make it as wide as the sentry
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - (spriteHeight / 2) - 10;


            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);


            if (CurrentHealth < 0) CurrentHealth = 0;

            int currentBarWidth = (int)(barWidth * (CurrentHealth / MaxHealth));
            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.Green);

            spriteBatch.End();

        }
    }
}