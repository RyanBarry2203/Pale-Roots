using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // SentryEnemy: stationary turret that rotates to track a target and fires a single reusable Projectile.
    // - Inherits AI/animation/visibility from Enemy -> RotatingSprite.
    // - Uses CombatSystem for target validation/distance checks.
    public class SentryEnemy : Enemy
    {
        // Projectile this sentry controls (assigned by level/factory). Projectile manages its own state.
        public Projectile MyProjectile { get; set; }
        
        // Timing and detection
        private float _reloadTimer = 0;            // current cooldown
        private float _reloadTime;                 // cooldown duration (ms)
        private float _detectionRadius;            // how far the sentry can detect targets

        // Constructor: configure rotation speed, reload and detection from shared constants.
        public SentryEnemy(Game g, Texture2D tx, Vector2 startPosition, int noOfFrames)
            : base(g, tx, startPosition, noOfFrames)
        {
            rotationSpeed = 0.15f;                             // how fast the turret rotates to face targets
            _reloadTime = GameConstants.DefaultReloadTime;     // configurable shared value
            _detectionRadius = GameConstants.DefaultDetectionRadius;
            
            // Sentry stays in place; use Wandering so base AI won't try to move it.
            CurrentAIState = AISTATE.Wandering;
        }

        // Attach a Projectile instance so the sentry can fire and update it.
        public void LoadProjectile(Projectile p)
        {
            MyProjectile = p;
        }

        // AI update override: handle reload, detection, orientation and projectile updates.
        protected override void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {
            // Countdown reload timer (milliseconds)
            if (_reloadTimer > 0)
                _reloadTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Only act if we have a valid target assigned by external systems (CombatSystem/LevelManager)
            if (CurrentTarget != null && CombatSystem.IsValidTarget(this, CurrentTarget))
            {
                float distance = CombatSystem.GetDistance(this, CurrentTarget);

                // If target is within detection range, rotate to face them and try to fire
                if (distance < _detectionRadius)
                {
                    Follow(CurrentTarget.Center); // inherited rotation helper

                    if (MyProjectile != null &&
                        MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL &&
                        _reloadTimer <= 0)
                    {
                        FireAtTarget();
                    }
                }
            }

            // Keep projectile visually attached while idle and always update it.
            if (MyProjectile != null)
            {
                if (MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL)
                {
                    MyProjectile.position = this.position;
                }
                MyProjectile.Update(gameTime);
            }

            // Intentionally do not call base.UpdateAI: sentry does not use movement/chase/wander state logic.
        }

        // Centralized fire logic so the timing reset and firing call are in one place.
        private void FireAtTarget()
        {
            if (CurrentTarget == null || MyProjectile == null) return;

            MyProjectile.fire(CurrentTarget.Center);
            _reloadTimer = _reloadTime;
        }

        // Disable movement behaviors: sentries are stationary.
        protected override void PerformCharge(List<WorldObject> obstacles) { }

        protected override void PerformChase(List<WorldObject> obstacles)
        {
            // When "chasing" (if base sets it), just rotate to face the current target.
            if (CurrentTarget != null)
            {
                Follow(CurrentTarget.Center);
            }
        }

        protected override void PerformWander(List<WorldObject> obstacles) { }

        // Sentry uses projectiles instead of melee; Attack is handled in UpdateAI via FireAtTarget.
        public override void PerformAttack() { }

        // Draw sentry and its projectile (projectile draws itself).
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (MyProjectile != null)
            {
                MyProjectile.Draw(spriteBatch);
            }
        }
    }
}
