//using AnimatedSprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1 // Consider renaming to 'PaleRoots' or 'Entities'
{
    // CHANGE: Inherits from Enemy instead of RotatingSprite
    internal class Enemy_sentry : Enemy
    {
        public Projectile MyProjectile { get; set; }
        float detectionRadius = 400f;
        float reloadTimer = 0;
        float timeToReload = 2000f;

        public float MaxHealth = 100;
        public float CurrentHealth = 100;

        // Static texture to prevent memory leaks (created once for all sentries)
        static Texture2D healthTexture;

        public Enemy_sentry(Game g, Texture2D tx, Vector2 StartPosition, int NoOfFrames)
            : base(g, tx, StartPosition, NoOfFrames)
        {
            this.rotationSpeed = 0.15f;

            // Only create the texture if it doesn't exist yet
            if (healthTexture == null)
            {
                healthTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                healthTexture.SetData(new[] { Color.White });
            }
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
                // We can use follow because we inherit from Enemy -> RotatingSprite
                this.follow(p);

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

            // Draw Health Bar
            // Note: We don't need spriteBatch.Begin() here because Draw is called inside a Begin/End block in Game1/Engine

            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2); // Center bar
            int barY = (int)position.Y - 10; // Above sprite

            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            if (CurrentHealth < 0) CurrentHealth = 0;

            int currentBarWidth = (int)(barWidth * (CurrentHealth / MaxHealth));
            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.Green);
        }
    }
}