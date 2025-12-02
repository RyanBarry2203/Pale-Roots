using Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sprites;
using System;
using Tracker.WebAPIClient;
//using Cameras;

namespace GP025Week6Lab2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Sprite Player;
        private Camera _camera;
        //private Sprite Stars;
        //private Sprite Star;

        private Sprite[] Stars = new Sprite[5];

        private Texture2D background;
        Texture2D txPlayer;
        Texture2D txStars;

        SpriteFont nameID;
        SpriteFont score;
        int scoreNum = 5;

        SoundEffect fanFare;
        //private Sprite Player;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 6 Lab 2", Task: "Implementing collectables and game play");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            nameID = Content.Load<SpriteFont>("NameID");
            score = Content.Load<SpriteFont>("score");

            background = Content.Load<Texture2D>("bigback3000x3000");
            txPlayer = Content.Load<Texture2D>("Rocket 32 x 51");
            txStars = Content.Load<Texture2D>("stars");
            fanFare = Content.Load<SoundEffect>("fanFare");

            //Player = new Sprite(txPlayer, new Vector2(400, 400), 4);

            Vector2 centerPosition = new Vector2(
            (_graphics.PreferredBackBufferWidth - txPlayer.Width / 8) / 2f,
            (_graphics.PreferredBackBufferHeight - txPlayer.Height) / 2f);

            Player = new Sprite(txPlayer, centerPosition, 4);

            _camera = new Camera(Vector2.Zero, new Vector2(background.Width, background.Height));

            for (int i = 0; i < Stars.Length; i++)
            {
                //declaring the collectables so the randow pos code knows the width and height of the sprite, declared at 0,0 first
                Stars[i] = new Sprite(txStars, Vector2.Zero, 6);

                //making sure its random but also fully on screen
                Vector2 randomPos = new Vector2(
                    Random.Shared.Next(0, _graphics.PreferredBackBufferWidth - Stars[i].SpriteWidth),
                    Random.Shared.Next(0, _graphics.PreferredBackBufferHeight - Stars[i].SpriteHeight)
                );

                //declaring the collectables with a random pos
                Stars[i] = new Sprite(txStars, randomPos, 5);


                //Star = new Sprite(txCollectable, new Vector2(200, 200), 6);
                //Player = new Sprite(txPlayer, new Vector2(400, 400), 4);





                // TODO: use this.Content to load your game content here
            }

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Player.Update(gameTime);

            var kstate = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            if (kstate.IsKeyDown(Keys.A))
                movement.X -= 5;
            if (kstate.IsKeyDown(Keys.D))
                movement.X += 5;
            if (kstate.IsKeyDown(Keys.W))
                movement.Y -= 5;
            if (kstate.IsKeyDown(Keys.S))
                movement.Y += 5;
            // TODO: Add your update logic here

            if (movement != Vector2.Zero)
                Player.Move(movement);

            // Keep player within screen bounds
            //if (Player.position.X < 0) Player.position.X = 0;
            //if (Player.position.X > _graphics.PreferredBackBufferWidth - Player.SpriteWidth)
            //Player.position.X = _graphics.PreferredBackBufferWidth - Player.SpriteWidth;
            //if (Player.position.Y < 0) Player.position.Y = 0;
            //if (Player.position.Y > _graphics.PreferredBackBufferHeight - Player.SpriteHeight)
            //Player.position.Y = _graphics.PreferredBackBufferHeight - Player.SpriteHeight;

            // had to fix a bug where the player was able to go offscreen to the left because the camerra world bound is larger than the screen
            //now directly using the camera world bound to keep the player in bounds

            //if (Player.position.X < 0)
            //Player.position.X = 0;

            //if (Player.position.X > _camera.WorldBound.X - Player.SpriteWidth)
            //Player.position.X = _camera.WorldBound.X - Player.SpriteWidth;

            //if (Player.position.Y < 0)
            //Player.position.Y = 0;

            // if (Player.position.Y > _camera.WorldBound.Y - Player.SpriteHeight)
            //Player.position.Y = _camera.WorldBound.Y - Player.SpriteHeight;

            //Player.BoundingRect = new Rectangle(
            //(int)Player.position.X,
            // (int)Player.position.Y,
            //Player.SpriteWidth,
            // Player.SpriteHeight);

            // LEFT boundary
            //if (Player.BoundingRect.Left < 0)
            //Player.position.X = 0;

            // right boundary
            //if (Player.BoundingRect.Right > _camera.WorldBound.X)
            //Player.position.X = _camera.WorldBound.X - Player.SpriteWidth;

            // top boundary
            //if (Player.BoundingRect.Top < 0)
            //Player.position.Y = 0;

            // bottom boundary
            //if (Player.BoundingRect.Bottom > _camera.WorldBound.Y)
            //Player.position.Y = _camera.WorldBound.Y - Player.SpriteHeight;

            // SO, i went down an rabbit hole for over an hour trying to fix the player going offscreen to the left, but the other boundries wewre perfect, and the camera followed the player perfectly,
            // but somehow on the left side it did not work. Eventually i figured out the problem that fixed it, SOMEHOW, was thast the origin of the sprite was drawn at the top left as it is 
            // by default and SOMEHOW that led to the bounding rect being missled and conflustered. so by making the centre the origin now the bounding rect works perfectly on all sides.
            // while i understand why this fixed it, i have no idea how the origin of the sprite would affect the bounding rect calculations as they are done independant of each other.

            Rectangle screenRect = Player.BoundingRect;
            screenRect.X -= (int)_camera.CamPos.X;
            screenRect.Y -= (int)_camera.CamPos.Y;

            // LEFT boundary (screen)
            if (screenRect.Left < 0)
                Player.position.X = _camera.CamPos.X;

            // RIGHT boundary (screen)
            if (screenRect.Right > GraphicsDevice.Viewport.Width)
                Player.position.X = _camera.CamPos.X + GraphicsDevice.Viewport.Width - Player.SpriteWidth;

            // TOP boundary (screen)
            if (screenRect.Top < 0)
                Player.position.Y = _camera.CamPos.Y;

            // BOTTOM boundary (screen)
            if (screenRect.Bottom > GraphicsDevice.Viewport.Height)
                Player.position.Y = _camera.CamPos.Y + GraphicsDevice.Viewport.Height - Player.SpriteHeight;

            _camera.follow(Player.position, GraphicsDevice.Viewport);


            foreach (var c in Stars)
            {
                if (c.alive)
                    c.Update(gameTime);
            }
            foreach (var c in Stars)
            {
                if (c.alive && Player.collisionDetect(c))
                {
                    //CoinCollect.Play();
                    c.alive = false;
                    scoreNum--;
                }
            }

            if (scoreNum == 0)
            {
                fanFare.Play();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            string nameAndID = "Ryan Barry S00250496";
            string scoreString = $"Score: {scoreNum}";

            GraphicsDevice.Clear(Color.CornflowerBlue);

            Vector2 textSize = nameID.MeasureString(nameAndID);
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2),
                GraphicsDevice.Viewport.Height - textSize.Y
            );

            //foreach (var c in Stars)
            //{
               // if (c.alive)
                   // c.Draw(_spriteBatch);
           // }

            //Player.Draw(_spriteBatch);
            _spriteBatch.Begin(transformMatrix: _camera.CurrentCameraTranslation);
            _spriteBatch.Draw(background, Vector2.Zero, Color.White);
            Player.Draw(_spriteBatch);
            foreach (var c in Stars)
            {
                if (c.alive)
                    c.Draw(_spriteBatch);
            }
            // _spriteBatch.DrawString(nameID, nameAndID, position, Color.White);
            //_spriteBatch.Draw(background, Vector2.Zero, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(nameID, nameAndID, position, Color.White);
            _spriteBatch.DrawString(score, scoreString, new Vector2(20, 20), Color.White);
            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
