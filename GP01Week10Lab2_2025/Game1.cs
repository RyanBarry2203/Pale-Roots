using AnimatedSprite;
using Engines;
using GP01Week10Lab2_2025;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using System.Media;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
//using SharpDX.Direct2D1;
using Tracker.WebAPIClient;
using Utilities;



namespace GP01Week10Lab2_2025
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        SpriteFont _nameID;
        Song _backingTrack;
        Texture2D _backgroundImage;
        PlayerWithWeapon Player;
        //CircularChasingEnemy chaser;
        //ChaseAndFireEngine chaseEngine;
        //public SoundEffect firingSound;
        SoundEffect sentryExplosionSound;


        //create list of 5 Enemy_sprites
        private List<Enemy_sentry> enemySentries = new List<Enemy_sentry>();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 10 Lab 2", Task: "Implementing Game Play");

            //chaseEngine = new ChaseAndFireEngine(this);
            

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _nameID = Content.Load<SpriteFont>("nameID");
            _backingTrack = Content.Load<Song>("backing track");
            _backgroundImage = Content.Load<Texture2D>("background");
            Player = new PlayerWithWeapon(this, Content.Load<Texture2D>("wizard_strip3"), new Vector2(400, 400), 3);

            Texture2D fireballTx = Content.Load<Texture2D>("fireball_strip4"); // Make sure you have this asset!
            Texture2D explosionTx = Content.Load<Texture2D>("explosion_strip8");

            sentryExplosionSound = Content.Load<SoundEffect>("explode1");

            Sprite playerExplosion = new Sprite(this, explosionTx, Vector2.Zero, 8);
            Projectile playerFireball = new Projectile(this, fireballTx, playerExplosion, Vector2.Zero, 4);

            Player.loadProjectile(playerFireball);

            Texture2D crossbowTx = Content.Load<Texture2D>("CrossBow");
            Texture2D arrowTx = Content.Load<Texture2D>("Arrow"); 

            //firingSound = Content.Load<SoundEffect>("explode1");

            Random rand = new Random();
            Vector2 randomPosition = new Vector2(
                rand.Next(0, GraphicsDevice.Viewport.Width),
                rand.Next(0, GraphicsDevice.Viewport.Height)
            );
            //for (int i = 0; i < 5; i++)
            //{ 
            //enemySentries.Add(new Enemy_sentry(this, Content.Load<Texture2D>("CrossBow"), randomPosition, 1));
            //    enemySentries.Add(new Enemy_sentry(this, Content.Load<Texture2D>("CrossBow"), randomPosition, 1));
            //    enemySentries.Add(new Enemy_sentry(this, Content.Load<Texture2D>("CrossBow"), randomPosition, 1));
            //    enemySentries.Add(new Enemy_sentry(this, Content.Load<Texture2D>("CrossBow"), randomPosition, 1));
            //    enemySentries.Add(new Enemy_sentry(this, Content.Load<Texture2D>("CrossBow"), randomPosition, 1));
            for (int i = 0; i < 5; i++)
            {
                int x = Utility.NextRandom(0, GraphicsDevice.Viewport.Width - crossbowTx.Width);
                int y = Utility.NextRandom(0, GraphicsDevice.Viewport.Height - crossbowTx.Height);
                Vector2 newPos = new Vector2(x, y);

                // Create the Sentry
                Enemy_sentry sentry = new Enemy_sentry(this, crossbowTx, newPos, 1);

                // Create a UNIQUE Projectile for this specific sentry
                // (We pass the sentry's position as the starting point)
                Sprite explosionSprite = new Sprite(this, explosionTx, newPos, 8);
                Projectile sentryArrow = new Projectile(this, arrowTx, explosionSprite, newPos, 1); 

                // Load the projectile into the sentry
                sentry.LoadProjectile(sentryArrow);

                // Add to list
                enemySentries.Add(sentry);
            }


            //} 



            MediaPlayer.Play(_backingTrack);
            MediaPlayer.IsRepeating = true; // Loops the music

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Player.Update(gameTime);

            for (int i = enemySentries.Count - 1; i >= 0; i--)
            {
                Enemy_sentry sentry = enemySentries[i];

                // Update the sentry logic
                sentry.UpdateSentry(gameTime, Player);


                if (Player.MyProjectile != null &&
                    Player.MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.FIRING)
                {
                    if (Player.MyProjectile.collisionDetect(sentry))
                    {
                        // Hurt Sentry
                        sentry.CurrentHealth -= 25; // 4 hits to kill

                        // Make the fireball explode visually
                        Player.MyProjectile.ProjectileState = Projectile.PROJECTILE_STATE.EXPOLODING;


                        if (sentry.CurrentHealth <= 0)
                        {

                            sentryExplosionSound.Play();

                            // Remove sentry from the game (Disabled)
                            enemySentries.RemoveAt(i);

                            // Skip the rest of the loop for this sentry
                            continue;
                        }
                    }
                }


                if (sentry.MyProjectile != null &&
                    sentry.MyProjectile.ProjectileState == Projectile.PROJECTILE_STATE.FIRING)
                {
                    if (sentry.MyProjectile.collisionDetect(Player))
                    {
                        // 1. Hurt Player
                        Player.CurrentHealth -= 10;

                        // 2. Make the arrow explode visually
                        sentry.MyProjectile.ProjectileState = Projectile.PROJECTILE_STATE.EXPOLODING;


                        if (Player.CurrentHealth <= 0)
                        {
                            Exit(); // End the game
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string nameID = "S00250496 Ryan Barry";
            Vector2 textSize = _nameID.MeasureString(nameID);
            Vector2 position = new Vector2((GraphicsDevice.Viewport.Width - textSize.X) / 2, GraphicsDevice.Viewport.Height - textSize.Y - 10);

            //draw background to fit the width and height of the screen;
            Rectangle screenRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            //Vector2 backgroundDisplaySize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundImage, screenRectangle, Color.White);
            _spriteBatch.DrawString(_nameID, nameID, position, Color.CornflowerBlue);
            _spriteBatch.End();

            Player.Draw(_spriteBatch);
            foreach (Enemy_sentry enemy in enemySentries)
            {
                enemy.Draw(_spriteBatch);
            }
            // TODO: Add your drawing code here
            //chaseEngine.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
