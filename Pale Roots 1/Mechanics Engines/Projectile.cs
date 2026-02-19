using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Pale_Roots_1
{
    // Projectile: simple reusable projectile that moves toward a target, then plays an explosion sprite and sound.
    // - Inherits rotation/movement helpers from RotatingSprite (TurnToFace, Follow, MoveToward, etc.).
    // - Uses an external Sprite instance for the explosion so the visual effect can be animated separately.
    // - Typical usage: create one Projectile, call Load/assign it to a Sentry/Enemy, call fire(target) to launch.
    public class Projectile : RotatingSprite
    {
        // Projectile lifecycle states.
        public enum PROJECTILE_STATE { STILL, FIRING, EXPOLODING };

        // Backing state field; controls Update() behavior.
        PROJECTILE_STATE projectileState = PROJECTILE_STATE.STILL;

        // Reference to the game for content access (sound).
        protected Game myGame;

        // How fast the projectile moves; used as a multiplier in the Lerp step.
        protected float RocketVelocity = 4.0f;

        // Precomputed half-size used to offset explosion sprite.
        Vector2 textureCenter;

        // Target world position the projectile moves toward.
        Vector2 Target;

        // Explosion visual handled by a separate Sprite (AnimationManager on that sprite will run independently).
        Sprite explosion;

        // Explosion timing: how long the explosion is visible before resetting to STILL.
        float ExplosionTimer = 0;
        float ExplosionVisibleLimit = 1000;

        // Where the projectile started (useful if you want to reset it later).
        Vector2 StartPosition;

        // Explosion sound effect loaded from Content.
        private SoundEffect explosionSound;

        // Public accessors for state and the explosion sprite.
        public PROJECTILE_STATE ProjectileState
        {
            get { return projectileState; }
            set { projectileState = value; }
        }

        public Sprite Explosion
        {
            get { return explosion; }
            set { explosion = value; }
        }

        // Constructor:
        // - texture: projectile image
        // - rocketExplosion: a Sprite instance used to render the explosion animation
        // - userPosition: spawn position for the projectile
        // - framecount: frames for the projectile (passed to base)
        public Projectile(Game g, Texture2D texture, Sprite rocketExplosion, Vector2 userPosition, int framecount)
            : base(g, texture, userPosition, framecount)
        {
            Target = Vector2.Zero;
            myGame = g;

            // store half-size so we can position the explosion origin sensibly
            textureCenter = new Vector2(texture.Width / 2, texture.Height / 2);

            explosion = rocketExplosion;

            // Offset explosion sprite so its position lines up with projectile visually.
            explosion.position -= textureCenter;
            explosion.Visible = false;

            StartPosition = position;
            ProjectileState = PROJECTILE_STATE.STILL;

            // Load an explosion SFX from Content (file name must exist in your Content).
            explosionSound = myGame.Content.Load<SoundEffect>("explosion");
        }

        // Per-frame logic:
        // - STILL: hide projectile/explosion
        // - FIRING: move toward Target and rotate to face it
        // - EXPOLODING: show explosion sprite and play sound
        public override void Update(GameTime gametime)
        {
            switch (projectileState)
            {
                case PROJECTILE_STATE.STILL:
                    // Hidden and idle while still.
                    this.Visible = false;
                    explosion.Visible = false;
                    break;

                case PROJECTILE_STATE.FIRING:
                    // Make projectile visible and step toward target.
                    this.Visible = true;

                    // Using Lerp to move smoothly toward the target; RocketVelocity scales the lerp speed.
                    position = Vector2.Lerp(position, Target, 0.02f * RocketVelocity);

                    // Rotate to face travel direction using the helper on RotatingSprite.
                    this.angleOfRotation = TurnToFace(position, Target, angleOfRotation, 1f);

                    // If we are very close to the target, trigger explosion state.
                    if (Vector2.Distance(position, Target) < 2)
                        projectileState = PROJECTILE_STATE.EXPOLODING;
                    break;

                case PROJECTILE_STATE.EXPOLODING:
                    // Place explosion at the impact point and show it.
                    explosion.position = Target;
                    explosion.Visible = true;

                    // Play sound once when explosion becomes visible.
                    explosionSound.Play();
                    break;
            }

            // If explosion is visible run its animation and accumulate the display timer.
            if (explosion.Visible)
            {
                explosion.Update(gametime);
                ExplosionTimer += gametime.ElapsedGameTime.Milliseconds;
            }

            // When the explosion has been visible long enough, reset everything to STILL.
            if (ExplosionTimer > ExplosionVisibleLimit)
            {
                explosion.Visible = false;
                ExplosionTimer = 0;
                projectileState = PROJECTILE_STATE.STILL;

                // Optionally reset projectile position so it can be reused from its start.
                position = StartPosition;
            }

            base.Update(gametime);
        }

        // Launch the projectile toward a world-space point.
        public void fire(Vector2 SiteTarget)
        {
            projectileState = PROJECTILE_STATE.FIRING;
            Target = SiteTarget;
        }

        // Draw projectile then explosion (explosion draws itself when visible).
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (explosion.Visible)
                explosion.Draw(spriteBatch);
        }
    }
}
