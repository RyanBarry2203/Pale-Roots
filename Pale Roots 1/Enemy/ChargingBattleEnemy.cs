using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class ChargingBattleEnemy : CircularChasingEnemy
    {

        public ChargingBattleEnemy(Game g, Texture2D texture, Vector2 Position1, int framecount)
             : base(g, texture, Position1, framecount)
        {
            startPosition = Position1;
            this.Velocity = 3.0f;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
