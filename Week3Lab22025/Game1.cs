using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Drawing;
using Tracker.WebAPIClient;

namespace Week3Lab22025
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D txSignPost;
        Texture2D txPlayer;
        Texture2D txBackground;
        Vector2 posSignPost, posPlayer;
        Microsoft.Xna.Framework.Rectangle rectSignPost, RectPlayer; // Explicitly specify the namespace
        Microsoft.Xna.Framework.Point playerSize; // Explicitly specify the namespace

        Song _backing;
        SoundEffect _sound;
        SoundEffectInstance _soundPlayer;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 3 Lab 2", Task: "S00250496 Sound Task Started");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _backing = Content.Load<Song>("score");
            MediaPlayer.Play(_backing);

            _sound = Content.Load<SoundEffect>("impact");
            _soundPlayer = _sound.CreateInstance();

            txBackground = Content.Load<Texture2D>("background");
            txPlayer = Content.Load<Texture2D>("body");
            txSignPost = Content.Load<Texture2D>("sign_yield");

            posPlayer = new Vector2(100, 100);
            posSignPost = new Vector2(150, 100);
            playerSize = new Microsoft.Xna.Framework.Point(txPlayer.Width, txPlayer.Height); // Use explicit namespace
            RectPlayer = new Microsoft.Xna.Framework.Rectangle(posPlayer.ToPoint(), new Microsoft.Xna.Framework.Point(txPlayer.Width, txPlayer.Height));
            rectSignPost = new Microsoft.Xna.Framework.Rectangle(posSignPost.ToPoint(), new Microsoft.Xna.Framework.Point(txSignPost.Width, txSignPost.Height));

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            // Player movement
            if (ks.IsKeyDown(Keys.A))  // Left
                posPlayer += new Vector2(-1, 0);
            if (ks.IsKeyDown(Keys.D))  // Right
                posPlayer += new Vector2(1, 0);
            if (ks.IsKeyDown(Keys.W))  // Up
                posPlayer += new Vector2(0, -1);
            if (ks.IsKeyDown(Keys.S))  // Down
                posPlayer += new Vector2(0, 1);

            // Update rectangle for collision and viewport checks
            RectPlayer = new Microsoft.Xna.Framework.Rectangle(posPlayer.ToPoint(), new Microsoft.Xna.Framework.Point(txPlayer.Width, txPlayer.Height));

            // Keep player inside the viewport
            if (posPlayer.X < 0) posPlayer.X = 0;
            if (posPlayer.Y < 0) posPlayer.Y = 0;
            if (posPlayer.X > _graphics.GraphicsDevice.Viewport.Width - txPlayer.Width)
                posPlayer.X = _graphics.GraphicsDevice.Viewport.Width - txPlayer.Width;
            if (posPlayer.Y > _graphics.GraphicsDevice.Viewport.Height - txPlayer.Height)
                posPlayer.Y = _graphics.GraphicsDevice.Viewport.Height - txPlayer.Height;

            // Check for collision with signpost
            if (RectPlayer.Intersects(rectSignPost))
            {
                if (_soundPlayer.State != SoundState.Playing)
                    _soundPlayer.Play();
            }

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            // Explicitly use Microsoft.Xna.Framework.Color to avoid ambiguity
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(txBackground, GraphicsDevice.Viewport.Bounds, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.Draw(txPlayer, posPlayer, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.Draw(txSignPost, posSignPost, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
