using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    /// <summary>
    /// A stationary turret enemy that detects and fires projectiles at the player.
    /// Inherits from Enemy (which inherits from RotatingSprite -> Sprite).
    /// </summary>
    internal class Enemy_sentry : Enemy
    {
        public Projectile MyProjectile { get; set; }

        private float detectionRadius = 400f;
        private float reloadTimer = 0;
        private float timeToReload = 2000f;

        public float MaxHealth = 100;
        public float CurrentHealth = 100;

        // Static texture to prevent memory leaks (created once for all sentries)
        private static Texture2D healthTexture;

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
            if (reloadTimer > 0)
                reloadTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // FIX: Changed 'WorldOrigin' to 'Center' - which exists in Sprite base class
            // Center returns: position + new Vector2((spriteWidth * Scale) / 2f, (spriteHeight * Scale) / 2f)
            float distance = Vector2.Distance(this.Center, p.CentrePos);

            if (distance < detectionRadius)
            {
                // We can use follow because we inherit from Enemy -> RotatingSprite
                this.follow(p);

                if (MyProjectile != null &&
                    MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL &&
                    reloadTimer <= 0)
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
            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - 10;

            // Background (red = missing health)
            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            // Foreground (green = current health)
            if (CurrentHealth < 0) CurrentHealth = 0;
            int currentBarWidth = (int)(barWidth * (CurrentHealth / MaxHealth));
            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.Green);
        }
    }
}