using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class BlackHoleBoss : Enemy
    {
        private float _gravityStrength = 500f;
        private float _gravityRadius = 150f;

        public BlackHoleBoss(Game game, Dictionary<string, Texture2D> textures, Vector2 pos)
            : base(game, textures, pos, 4)
        {
            Name = "Event Horizon Golem";
            MaxHealth = 5000;
            Health = MaxHealth;
            Scale = 4.0f;
            AttackDamage = 50;
            _animManager.AddAnimation("Attack", new Animation(textures["Attack"], 11, 0, 100f, false, 1, 0, false));
            _animManager.AddAnimation("Death", new Animation(textures["Death"], 12, 0, 150f, false, 1, 0, false));
            _animManager.AddAnimation("Idle", new Animation(textures["Idle"], 8, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Walk", new Animation(textures["Walk"], 10, 0, 150f, true, 1, 0, false));
            _animManager.AddAnimation("Hurt", new Animation(textures["Hurt"], 4, 0, 150f, false, 1, 0, false));


            // 3. Set initial state
            ChangeState(new BossIdleState());
        }

        public void UpdateBossLogic(GameTime gameTime, Player player)
        {
            Vector2 pull = PhysicsGlobals.CalculateGravitationalForce(this.Center, player.Center, _gravityStrength, _gravityRadius);
            player.ApplyExternalForce(pull * (float)gameTime.ElapsedGameTime.TotalSeconds);


            float xDifference = player.Center.X - this.Center.X;


            if (xDifference < 0)
            {
                _flipEffect = SpriteEffects.FlipHorizontally;

            }
            else
            {
                _flipEffect = SpriteEffects.None;
            }

            base.Update(gameTime);
        }
    }
}