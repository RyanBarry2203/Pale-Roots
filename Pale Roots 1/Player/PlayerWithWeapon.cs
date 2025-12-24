using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
//using Engines;

namespace Pale_Roots_1
{
    
        public class PlayerWithWeapon : Sprite
        {
            protected Game myGame;
            protected float playerVelocity = 6.0f;
        private Projectile myProjectile;
        //public CrossHair Site;
        //public SoundEffect shoot;
        private SoundEffect shoot;
        //had to add logic for previous keyboard state to prevent multiple firings on a single key press as the shoot sound was constanly playing
        private KeyboardState previousKeyboardState;


        public Vector2 CentrePos
            {
                get { return position + new Vector2(spriteWidth/ 2, spriteHeight / 2); }
                
            }

        public Projectile MyProjectile
        {
            get
            {
                return myProjectile;
            }

            set
            {
                myProjectile = value;
            }
        }

        public PlayerWithWeapon(Game g, Texture2D texture, Vector2 userPosition, int framecount) : base(g,texture,userPosition,framecount, 1)
            {
                myGame = g;
            var vp = g.GraphicsDevice.Viewport;
            //Site = new CrossHair(g,
            //                     g.Content.Load<Texture2D>("scope2"),
            //                     new Vector2(vp.Width / 2, vp.Height / 2),
            //                     1);
            //shoot = g.Content.Load<SoundEffect>("shoot");
            //didnt know where to load the asset as there is no load content method here so loaded it in the constructor

        }

        public void loadProjectile(Projectile r)
            {
                MyProjectile = r;
            }


        public void Update(GameTime gameTime, TileLayer layer)
        {
           
            Viewport gameScreen = myGame.GraphicsDevice.Viewport;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                this.position += new Vector2(1, 0) * playerVelocity;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                this.position += new Vector2(-1, 0) * playerVelocity;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                this.position += new Vector2(0, -1) * playerVelocity;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                this.position += new Vector2(0, 1) * playerVelocity;
            }
            // check for site change
            
            //Site.Update(gameTime);
            // Whenever the rocket is still and loaded it follows the player posiion
            if (MyProjectile != null 
                && MyProjectile.ProjectileState 
                == Projectile.PROJECTILE_STATE.STILL)
                MyProjectile.position = this.CentrePos;
            // if a roecket is loaded
            if (MyProjectile != null && MyProjectile.ProjectileState
                == Projectile.PROJECTILE_STATE.STILL)
            {
                KeyboardState ks = Keyboard.GetState();
                // fire the rocket and it looks for the target
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                {     //had a probloem here where because the asset i got for the new crosshair was massive, i shrunk the scale but the project was still firing to the top left corner of the origiuonal
                    //image size not the scaled down size. So i adjusted the target position by half the width and height of the original image to get it to fire to the right place.
                    //MyProjectile.fire(Site.position);
                    //MyProjectile.fire(Site.position + new Vector2(Site.spriteWidth * 0.1f, Site.spriteHeight * 0.1f));
                    //shoot = Content.Load<SoundEffect>("shoot");
                    shoot.Play();
                }

            }

            // Make sure the player stays in the bounds see previous lab for details
            position = Vector2.Clamp(position, Vector2.Zero,
                                            new Vector2(gameScreen.Width - spriteWidth,
                                                        gameScreen.Height - spriteHeight));
            
            // Update the Camera with respect to the players new position
            //Vector2 delta = cam.Pos - this.position;
            //cam.Pos += delta;
            
            if (MyProjectile != null)
                MyProjectile.Update(gameTime);
            // Update the players site
            //Site.Update(gameTime);
            // call Sprite Update to get it to animated 
            base.Update(gameTime);
        }
            
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            //Site.Draw(spriteBatch);
            if (MyProjectile != null 
                && MyProjectile.ProjectileState 
                != Projectile.PROJECTILE_STATE.STILL)
                    MyProjectile.Draw(spriteBatch);
            
        }

    }
}
