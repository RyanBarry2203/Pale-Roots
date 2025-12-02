using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AnimatedSprite;
using Utilities;
//using Sprite;

namespace Engines
{
    class ChaseAndFireEngine
    {
        PlayerWithWeapon p;
        SpriteBatch spriteBatch;
        private CircularChasingEnemy[] chasers;
        private Game _gameOwnedBy;
        RotatingSprite CrossBow;
        Projectile Arrow;
        Texture2D _background;
        SoundEffect _backingTrack;
        SoundEffectInstance _backingTrackInstance;
        //public SoundEffect shoot;

        public ChaseAndFireEngine(Game game)
            {
                // Chase engine remembers reference to the game
                _gameOwnedBy = game;
                game.IsMouseVisible = true;
                SoundEffect[] _PlayerSounds = new SoundEffect[5];
                spriteBatch = new SpriteBatch(game.GraphicsDevice);

            CrossBow = new RotatingSprite(game,
                game.Content.Load<Texture2D>("CrossBow"), 
                new Vector2(100, 100), 1);
            CrossBow.rotationSpeed = 0.01f;
            Arrow = new Projectile(game, 
                game.Content.Load<Texture2D>("Arrow"),
                new Sprite(game, 
                game.Content.Load<Texture2D>("explosion_strip8"),CrossBow.position, 8),
                CrossBow.position, 4);

            p = new PlayerWithWeapon(game, game.Content.Load<Texture2D>("viking_strip3"), new Vector2(400, 400), 3);
            //fireball = new Projectile(game, game.Content.Load<Texture2D>(@"Textures/fireball_strip4"),
            //                            new Sprite(game, game.Content.Load<Texture2D>(@"Textures/explosion_strip8"),p.position,8)
            //                            ,p.position, 4);

            p.loadProjectile(new Projectile(game, game.Content.Load<Texture2D>("explosion_strip8"),
                                        new Sprite(game, game.Content.Load<Texture2D>("explosion_strip8"), p.position, 8)
                                        , p.position, 4));

            chasers = new CircularChasingEnemy[Utility.NextRandom(2,5)];

            for (int i = 0; i < chasers.Count(); i++)
                {
                    chasers[i] = new CircularChasingEnemy(game,
                            game.Content.Load<Texture2D>("Dragon_strip3"), 
                                Vector2.Zero,
                             3);
                    chasers[i].myVelocity = (float)Utility.NextRandom(2, 5);
                    chasers[i].position = new Vector2(Utility.NextRandom(game.GraphicsDevice.Viewport.Width - chasers[i].spriteWidth),
                            Utility.NextRandom(game.GraphicsDevice.Viewport.Height - chasers[i].spriteHeight));
                }

            //game.Content.Load<Texture2D>("background"); new Vector2(0, 0); 1;
            _background = game.Content.Load<Texture2D>("background");
            _backingTrack = game.Content.Load<SoundEffect>("score");
            _backingTrackInstance = _backingTrack.CreateInstance();

            _backingTrackInstance.IsLooped = true;
            _backingTrackInstance.Play();


        }
        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(_gameOwnedBy.GraphicsDevice);
            //shoot = _gameOwnedBy.Content.Load<SoundEffect>("shoot");
            //_background = LoadContent
        }



        public void Update(GameTime gameTime)
        {
            
            p.Update(gameTime);
            //p.Site.Update(gameTime);

            CrossBow.follow(p);
            CrossBow.Update(gameTime);
            if(Keyboard.GetState().IsKeyDown(Keys.Enter) 
                    && Arrow.ProjectileState == Projectile.PROJECTILE_STATE.STILL)
            {
                Arrow.fire(p.Center);
            }
            if(Arrow.ProjectileState == Projectile.PROJECTILE_STATE.EXPOLODING)
            {
                Arrow.position = CrossBow.position;
            }
                Arrow.Update(gameTime);

            foreach (CircularChasingEnemy chaser in chasers)
            {
                if (p.MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.EXPOLODING && p.MyProjectile.collisionDetect(chaser))
                    chaser.die();
                chaser.follow(p);
                chaser.Update(gameTime);
            }
            
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(_background, Vector2.Zero, Color.White);

            p.Draw(spriteBatch);
            CrossBow.Draw(spriteBatch);

            if (Arrow.ProjectileState != Projectile.PROJECTILE_STATE.STILL)
                Arrow.Draw(spriteBatch);

            foreach (CircularChasingEnemy chaser in chasers)
                chaser.Draw(spriteBatch);

            spriteBatch.End();
        }




    }
}
