using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    /// <summary>
    /// Fast charging enemy for battle scenarios.
    /// Inherits from CircularChasingEnemy but with higher speed.
    /// 
    /// This class now actually has a purpose beyond just setting velocity!
    /// It represents aggressive front-line enemies.
    /// </summary>
    public class ChargingBattleEnemy : CircularChasingEnemy
    {
        /// <summary>Speed boost when charging (multiplier)</summary>
        public float ChargeSpeedMultiplier { get; set; } = 1.5f;
        
        /// <summary>Base speed for this enemy type</summary>
        private float _baseVelocity;

        public ChargingBattleEnemy(Game g, Texture2D texture, Vector2 position1, int framecount)
            : base(g, texture, position1, framecount)
        {
            _baseVelocity = 3.0f;
            Velocity = _baseVelocity;
            
            // Larger chase radius - these are aggressive
            ChaseRadius = GameConstants.DefaultChaseRadius * 1.5f;
            
            // Start charging
            CurrentAIState = AISTATE.Charging;
        }

        /// <summary>
        /// Charge faster than normal movement
        /// </summary>
        protected override void PerformCharge(List<WorldObject> obstacles)
        {
            // Boost speed while charging
            Velocity = _baseVelocity * ChargeSpeedMultiplier;
            
            // Charge left toward player side
            position.X -= Velocity;

            Vector2 target = new Vector2(position.X - 1000, position.Y);
            MoveToward(target, Velocity, obstacles);
        }

        /// <summary>
        /// Normal speed when chasing specific target
        /// </summary>
        protected override void PerformChase(List<WorldObject> obstacles)
        {
            Velocity = _baseVelocity;
            base.PerformChase(obstacles);
        }

        /// <summary>
        /// Normal speed in combat
        /// </summary>
        protected override void PerformCombat(GameTime gameTime)
        {
            Velocity = _baseVelocity;
            base.PerformCombat(gameTime);
        }
    }
}
