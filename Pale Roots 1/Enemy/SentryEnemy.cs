using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    /// <summary>
    /// Stationary turret enemy that rotates to track targets and fires projectiles.
    /// Does not move, but has ranged attacks.
    /// </summary>
    public class SentryEnemy : Enemy
    {
        // ===================
        // PROJECTILE
        // ===================
        
        public Projectile MyProjectile { get; set; }
        
        private float _reloadTimer = 0;
        private float _reloadTime;
        private float _detectionRadius;

        // ===================
        // CONSTRUCTOR
        // ===================
        
        public SentryEnemy(Game g, Texture2D tx, Vector2 startPosition, int noOfFrames)
            : base(g, tx, startPosition, noOfFrames)
        {
            rotationSpeed = 0.15f;
            _reloadTime = GameConstants.DefaultReloadTime;
            _detectionRadius = GameConstants.DefaultDetectionRadius;
            
            // Sentries don't charge or wander - they stay put
            CurrentAIState = AISTATE.Wandering; // Will idle in place
        }

        // ===================
        // PROJECTILE SETUP
        // ===================
        
        public void LoadProjectile(Projectile p)
        {
            MyProjectile = p;
        }

        // ===================
        // UPDATE
        // ===================
        
        protected override void UpdateAI(GameTime gameTime, List<WorldObject> obstacles)
        {
            // Reload timer
            if (_reloadTimer > 0)
                _reloadTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for targets in range
            if (CurrentTarget != null && CombatSystem.IsValidTarget(this, CurrentTarget))
            {
                float distance = CombatSystem.GetDistance(this, CurrentTarget);
                
                if (distance < _detectionRadius)
                {
                    Follow(CurrentTarget.Center);

                    // Fire if ready
                    if (MyProjectile != null && 
                        MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL &&
                        _reloadTimer <= 0)
                    {
                        FireAtTarget();
                    }
                }
            }

            // Update projectile position when not firing
            if (MyProjectile != null)
            {
                if (MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.STILL)
                {
                    MyProjectile.position = this.position;
                }
                MyProjectile.Update(gameTime);
            }

            // Don't call base.UpdateAI - we handle everything here
            // Sentry doesn't move, so no need for chase/wander logic
        }

        /// <summary>
        /// Fire projectile at current target
        /// </summary>
        private void FireAtTarget()
        {
            if (CurrentTarget == null || MyProjectile == null) return;
            
            MyProjectile.fire(CurrentTarget.Center);
            _reloadTimer = _reloadTime;
        }

        // ===================
        // OVERRIDE BEHAVIORS (disable movement)
        // ===================
        
        protected override void PerformCharge(List<WorldObject> obstacles)
        {
            // Sentries don't move
        }

        protected override void PerformChase(List<WorldObject> obstacles)
        {
            // Sentries don't move - just track with rotation
            if (CurrentTarget != null)
            {
                Follow(CurrentTarget.Center);
            }
        }

        protected override void PerformWander(List<WorldObject> obstacles)
        {
            // Sentries don't move - idle in place
        }

        // ===================
        // RANGED ATTACK
        // ===================
        
        public override void PerformAttack()
        {
            // Sentries use projectiles, not melee
            // Attack is handled by FireAtTarget in UpdateAI
        }

        // ===================
        // DRAW
        // ===================
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            // Draw projectile
            if (MyProjectile != null)
            {
                MyProjectile.Draw(spriteBatch);
            }
        }
    }
}
