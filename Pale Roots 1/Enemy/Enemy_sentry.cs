using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Stationary turret that aims at the player and fires a reusable projectile.
    // Inherits visuals/movement helpers from Enemy -> RotatingSprite -> Sprite.
    internal class Enemy_sentry : Enemy
    {
        // Projectile instance this sentry controls. Assigned by level factory or engine.
        // Reusing one Projectile avoids per-shot allocations; Projectile tracks its own state.
        public Projectile MyProjectile { get; set; }

        // Detection and firing timers
        private float detectionRadius = 400f;
        private float reloadTimer = 0;
        private float timeToReload = 2000f; // milliseconds between shots

        // NOTE: These fields shadow the `MaxHealth`/`Health` members on the base `Enemy`.
        // Keep that in mind — it's fine for simple local bars but can be confusing.
        public float MaxHealth = 100;
        public float CurrentHealth = 100;

        // Shared 1x1 texture used to draw the health bar background/foreground.
        private static Texture2D healthTexture;

        public Enemy_sentry(Game g, Texture2D tx, Vector2 StartPosition, int NoOfFrames)
            : base(g, tx, StartPosition, NoOfFrames)
        {
            // How fast the turret rotates when tracking a target (inherited field on RotatingSprite).
            this.rotationSpeed = 0.15f;

            // Create the 1x1 white texture once and reuse it for all sentries.
            if (healthTexture == null)
            {
                healthTexture = new Texture2D(g.GraphicsDevice, 1, 1);
                healthTexture.SetData(new[] { Color.White });
            }
        }

        // Attach a Projectile instance so the sentry can control firing and updates.
        // Typical caller: LevelManager, ChaseAndFireEngine, or a spawn factory.
        public void LoadProjectile(Projectile p)
        {
            MyProjectile = p;
        }

        // Main update called from the level (use this instead of Enemy.Update when you need player info).
        // - Decrements reload timer
        // - If player is in range, rotate to face them and attempt to fire
        // - Keeps the projectile positioned at the turret when it's "still"
        public void UpdateSentry(GameTime gameTime, Player p)
        {
            // Count down reload timer in milliseconds
            if (reloadTimer > 0)
                reloadTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Use Sprite/RotatingSprite center to compute distance to player
            // Player exposes `CentrePos` (note British spelling in Player).
            float distance = Vector2.Distance(this.Center, p.CentrePos);

            if (distance < detectionRadius)
            {
                // `follow` is inherited from RotatingSprite/Enemy and orients the turret toward the player.
                this.follow(p);

                // Fire only if a projectile is loaded, it's idle, and the turret has reloaded.
                if (MyProjectile != null &&
                    MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL &&
                    reloadTimer <= 0)
                {
                    // Ask the projectile to fire toward the player's current position.
                    MyProjectile.fire(p.CentrePos);
                    reloadTimer = timeToReload;
                }
            }

            // If the projectile is idle, keep it visually attached to the turret; always update it.
            if (MyProjectile != null)
            {
                if (MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL)
                {
                    MyProjectile.position = this.position;
                }
                MyProjectile.Update(gameTime);
            }

            // Call base update to run animations and other shared logic in Enemy.
            base.Update(gameTime);
        }

        // Draw turret, its projectile, and a compact health bar.
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (MyProjectile != null)
            {
                MyProjectile.Draw(spriteBatch);
            }

            // Health bar above the turret (uses spriteWidth/spriteHeight from Sprite base).
            int barWidth = spriteWidth;
            int barHeight = 5;
            int barX = (int)position.X - (barWidth / 2);
            int barY = (int)position.Y - 10;

            // Background = missing health (red)
            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.Red);

            // Foreground = current health (green)
            if (CurrentHealth < 0) CurrentHealth = 0;
            int currentBarWidth = (int)(barWidth * (CurrentHealth / MaxHealth));
            spriteBatch.Draw(healthTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), Color.Green);
        }
    }
}