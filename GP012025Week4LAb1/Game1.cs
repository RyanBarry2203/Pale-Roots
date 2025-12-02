using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sprites;
using System;

//
//using SharpDX.Direct2D1;
using Tracker.WebAPIClient;
using Tracker.WebAPIClient;

namespace GP012025Week4LAb1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SimpleSprite _simpleSprite1;
        private float _speed = 200f;

        private SimpleSprite _simpleSprite2;
        private float _speed2 = 400f;

        private SoundEffect _collisionSound;
        private bool _isColliding = false;  


        Texture2D txBackground;
        SpriteFont font;
        Texture2D _txLips;
        Texture2D _txBody;
        //Texture2D txBody2;
        //Texture2D txLips;
        // Add this field to the Game1 class to fix the missing 'posPlayer' error
        //figuring out where i am
        private Vector2 posPlayer = Vector2.Zero;



        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 4 Lab 1", Task: "Attaching Text to Simple Sprite");

            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _collisionSound = Content.Load<SoundEffect>("boom");
            _collisionSound.Play(1.0f, 0f, 0f);


            txBackground = Content.Load<Texture2D>("backgroundImage");
            font = Content.Load<SpriteFont>("MyFont");

            Texture2D lipsTexture = Content.Load<Texture2D>("lips");
            Texture2D bodyTexture = Content.Load<Texture2D>("body");

            _simpleSprite1 = new SimpleSprite(lipsTexture, new Vector2(200, 200));
            _simpleSprite2 = new SimpleSprite(bodyTexture, new Vector2(500, 200));

            //initiate sprite to draw its position above itself
            //_simpleSprite1.DrawMessage(_spriteBatch, font, "Sprite 1 Position: " + _simpleSprite1.Position);
        }


        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 move1 = Vector2.Zero;
            Vector2 move2 = Vector2.Zero;

            if (state.IsKeyDown(Keys.W)) move1.Y -= 1;
            if (state.IsKeyDown(Keys.S)) move1.Y += 1;
            if (state.IsKeyDown(Keys.A)) move1.X -= 1;
            if (state.IsKeyDown(Keys.D)) move1.X += 1;

            if (state.IsKeyDown(Keys.Up)) move2.Y -= 1;
            if (state.IsKeyDown(Keys.Down)) move2.Y += 1;
            if (state.IsKeyDown(Keys.Left)) move2.X -= 1;
            if (state.IsKeyDown(Keys.Right)) move2.X += 1;

            if (move1 != Vector2.Zero) move1.Normalize();
            if (move2 != Vector2.Zero) move2.Normalize();

            Vector2 newPosition1 = _simpleSprite1.Position + move1 * _speed * delta;
            Vector2 newPosition2 = _simpleSprite2.Position + move2 * _speed2 * delta;

            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            newPosition1.X = MathHelper.Clamp(newPosition1.X, 0, w - _simpleSprite1.Image.Width);
            newPosition1.Y = MathHelper.Clamp(newPosition1.Y, 0, h - _simpleSprite1.Image.Height);

            newPosition2.X = MathHelper.Clamp(newPosition2.X, 0, w - _simpleSprite2.Image.Width);
            newPosition2.Y = MathHelper.Clamp(newPosition2.Y, 0, h - _simpleSprite2.Image.Height);

            _simpleSprite1.Position = newPosition1;
            _simpleSprite2.Position = newPosition2;

            _simpleSprite1.Move(Vector2.Zero);
            _simpleSprite2.Move(Vector2.Zero);

            bool currentlyColliding = _simpleSprite1.Collision(_simpleSprite2);

            if (currentlyColliding && !_isColliding)
            {
                _collisionSound.Play(1.0f, 0.0f, 0.0f);
            }

            _isColliding = currentlyColliding;


            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string nameAndID = "Ryan Barry S00250496";
            Vector2 textSize = font.MeasureString(nameAndID);

            Vector2 position = new Vector2(
        GraphicsDevice.Viewport.Width - textSize.X - 10,
        10
    );

            _spriteBatch.Begin();
            _spriteBatch.Draw(txBackground, GraphicsDevice.Viewport.Bounds, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, nameAndID, position, Color.White);
            _simpleSprite1.draw(_spriteBatch);
            _simpleSprite2.draw(_spriteBatch);
            _simpleSprite1.DrawMessage(_spriteBatch, font, "Sprite 1 Position: " + _simpleSprite1.Position);
            //_spriteBatch.Draw(txBody, posPlayer, Microsoft.Xna.Framework.Color.White);
            //_spriteBatch.Draw(txBody2, posSignPost, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.End();

            // TODO: Add your drawing code here
            //simpleSprite1.DrawMessage(_spriteBatch, font, "Sprite 1 Position: " + _simpleSprite1.Position);

            base.Draw(gameTime);
        }
    }
}
