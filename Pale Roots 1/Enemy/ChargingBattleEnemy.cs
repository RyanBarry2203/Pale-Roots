using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // ChargingBattleEnemy: a fast, aggressive enemy.
    // Inherits movement, AI states and combat hooks from CircularChasingEnemy -> Enemy -> RotatingSprite.
    public class ChargingBattleEnemy : CircularChasingEnemy
    {
        // Tunable multiplier for how much faster this enemy moves while charging.
        // Other systems (spawn/level/balance) can change this at runtime.
        public float ChargeSpeedMultiplier { get; set; } = 1.5f;
        
        // Base movement speed used when not charging.
        private float _baseVelocity;

        // Constructor matches base signatures so factories/LevelManager can create this class like other enemies.
        public ChargingBattleEnemy(Game g, Texture2D texture, Vector2 position1, int framecount)
            : base(g, texture, position1, framecount)
        {
            // Choose default non-charging speed.
            _baseVelocity = 3.0f;

            // Set the inherited Velocity field so movement/animation systems use correct speed.
            Velocity = _baseVelocity;
            
            // Increase inherited chase detection radius so this enemy detects targets farther away.
            // Uses shared GameConstants (defined elsewhere).
            ChaseRadius = GameConstants.DefaultChaseRadius * 1.5f;
            
            // Start this instance in the Charging AI state; the base UpdateAI will treat it accordingly.
            CurrentAIState = AISTATE.Charging;
        }

        // Called by the base AI when in the Charging state.
        // 'obstacles' comes from LevelManager each frame and contains map objects (WorldObject) to avoid.
        protected override void PerformCharge(List<WorldObject> obstacles)
        {
            // Temporarily boost Velocity for charge movement/collision/animation.
            Velocity = _baseVelocity * ChargeSpeedMultiplier;
            
            // Immediate leftward nudge to create a lunge effect (project assumes left is the player side).
            position.X -= Velocity;

            // Build a distant left target and call the inherited MoveToward helper.
            // MoveToward (in a base class) performs obstacle-aware stepping and rotation.
            Vector2 target = new Vector2(position.X - 1000, position.Y);
            MoveToward(target, Velocity, obstacles);
        }

        // Called by the base AI when chasing a specific target.
        // Restores normal speed and defers to the base chase behavior that handles steering & state transitions.
        protected override void PerformChase(List<WorldObject> obstacles)
        {
            Velocity = _baseVelocity;
            base.PerformChase(obstacles); // base handles moving toward CurrentTarget and switching to InCombat.
        }

        // Called by the base AI when engaged in combat.
        // Use normal speed and let the base combat logic handle attacks/animations through CombatSystem.
        protected override void PerformCombat(GameTime gameTime)
        {
            Velocity = _baseVelocity;
            base.PerformCombat(gameTime);
        }
    }
}
