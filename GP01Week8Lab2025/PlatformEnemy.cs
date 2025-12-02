using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Sprites
{
    class PlatformEnemy : Enemy
    {
        Vector2 EndPosition;

         public PlatformEnemy(Game g, Texture2D texture, Vector2 Position1, Vector2 Position2, int framecount) 
             : base(g,texture,Position1,framecount)
        {
            startPosition = Position1;
            TargetPosition = EndPosition = Position2;

        }

         public override void Update(GameTime gt)
         {
             position = Vector2.Lerp(position, TargetPosition, 0.05f);
             if (Vector2.Distance(position, EndPosition) < 1)
             {
                 position = TargetPosition;
                 TargetPosition = startPosition;
             }
             if (Vector2.Distance(position, startPosition) < 1)
             {
                 position = TargetPosition;
                 TargetPosition = EndPosition;
             }
             base.Update(gt);

         }

         

    }
}
