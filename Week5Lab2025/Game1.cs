using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sprites;
using System;
using Tracker.WebAPIClient;

namespace Week5Lab2025
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Sprite Collectable;
        private Sprite Player;

        // initialise array of collectables
        private Sprite[] Collectables = new Sprite[5];

        SpriteFont font;
        SpriteFont Score;
        Texture2D txBackground;
        Texture2D txCollectable;
        Texture2D txPlayer;
        SoundEffect CoinCollect;
        Texture2D txGameOver;
        SoundEffect GameOver;

        int score = 0;

        //Texture2D txCollectables1;
        //Texture2D txCollectables2;
        //Texture2D txCollectables3;
        //Texture2D txCollectables4;
        //Texture2D txCollectables5;

        //AnimatedSprite Collectable = new AnimatedSprite();


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "Week 5 Lab 1 2025", Task: "Implementing Collectable collection");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("NameID");
            Score = Content.Load<SpriteFont>("Score");

            txBackground = Content.Load<Texture2D>("More Sheets/background");
            txCollectable = Content.Load<Texture2D>("More Sheets/Collectable");
            txPlayer = Content.Load<Texture2D>("More Sheets/sonicsheet");
            CoinCollect = Content.Load<SoundEffect>("More Audio/checkpoint");
            txGameOver = Content.Load<Texture2D>("gameover");
            GameOver = Content.Load<SoundEffect>("More Audio/1b");

            //txCollectables1 = Content.Load<Texture2D>("More Sheets/Collectable1");
            //txCollectables2 = Content.Load<Texture2D>("More Sheets/Collectable2");
            //txCollectables3 = Content.Load<Texture2D>("More Sheets/Collectable3");
            //txCollectables4 = Content.Load<Texture2D>("More Sheets/Collectable4");
            //txCollectables5 = Content.Load<Texture2D>("More Sheets/Collectable5");


            for (int i = 0; i < Collectables.Length; i++)
            {
                //declaring the collectables so the randow pos code knows the width and height of the sprite, declared at 0,0 first
                Collectables[i] = new Sprite(txCollectable, Vector2.Zero, 6);

                Vector2 randomPos = new Vector2(
                    Random.Shared.Next(0, _graphics.PreferredBackBufferWidth - Collectables[i].SpriteWidth),
                    Random.Shared.Next(0, _graphics.PreferredBackBufferHeight - Collectables[i].SpriteHeight)
                );

                //redeclaring the collectables with a random pos
                Collectables[i] = new Sprite(txCollectable, randomPos, 6);


                Collectable = new Sprite(txCollectable, new Vector2(200, 200), 6);
                Player = new Sprite(txPlayer, new Vector2(400, 400), 8);





                // TODO: use this.Content to load your game content here
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Collectable.alive && Player.collisionDetect(Collectable))
            {
                CoinCollect.Play();        
                Collectable.alive = false;
                score += 100;
            }

            Collectable.Update(gameTime);
            Player.Update(gameTime);

            foreach (var c in Collectables)
            {
                if (c.alive)
                    c.Update(gameTime);
            }
            foreach (var c in Collectables)
            {
                if (c.alive && Player.collisionDetect(c))
                {
                    CoinCollect.Play();
                    c.alive = false;
                    score += 100;
                }
            }

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

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                // Reset the collectable at a new random location
                Collectable.position = new Vector2(
                    Random.Shared.Next(0, _graphics.PreferredBackBufferWidth - Collectable.SpriteWidth),
                    Random.Shared.Next(0, _graphics.PreferredBackBufferHeight - Collectable.SpriteHeight)
                );
                Collectable.alive = true;
            }

            // Only move if there’s movement input
            if (movement != Vector2.Zero)
                Player.Move(movement);

            // Keep player within screen bounds
            if (Player.position.X < 0) Player.position.X = 0;
            if (Player.position.X > _graphics.PreferredBackBufferWidth - Player.SpriteWidth)
                Player.position.X = _graphics.PreferredBackBufferWidth - Player.SpriteWidth;
            if (Player.position.Y < 0) Player.position.Y = 0;
            if (Player.position.Y > _graphics.PreferredBackBufferHeight - Player.SpriteHeight)
                Player.position.Y = _graphics.PreferredBackBufferHeight - Player.SpriteHeight;

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string nameAndID = "Ryan Barry S00250496";
            string scoreString = $"Score: {score}";



            Vector2 textSize = font.MeasureString(nameAndID);
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2),
                GraphicsDevice.Viewport.Height - textSize.Y
            );

            _spriteBatch.Begin();
            _spriteBatch.Draw(txBackground, GraphicsDevice.Viewport.Bounds, Color.White);
            Collectable.Draw(_spriteBatch);

            foreach (var c in Collectables)
            {
                if (c.alive)
                    c.Draw(_spriteBatch);
            }
            if (score == 600)
            {
                _spriteBatch.Draw(txGameOver, GraphicsDevice.Viewport.Bounds, Color.White);
                //GameOver.Play();
            }
            Player.Draw(_spriteBatch);
            _spriteBatch.DrawString(font, nameAndID, position, Color.White);
            _spriteBatch.DrawString(Score, scoreString, new Vector2(20, 20), Color.Yellow);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
