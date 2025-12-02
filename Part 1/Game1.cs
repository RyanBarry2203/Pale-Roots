using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tracker.WebAPIClient;

namespace Part_1
{
    
    public class Game1 : Game

    {
        

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D TxOpening;
        Texture2D TxGameOver;

        bool Opening = true;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            TxOpening = Content.Load<Texture2D>("OpeningSplashScreen");
            TxGameOver = Content.Load<Texture2D>("GameOverSplashScreen");

            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry",
                activityName: "GP01 2025 Week 1 Lab 2", Task: "Week 1 Lab 2 Setup and Display Textures");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if(Opening && Keyboard.GetState().IsKeyDown(Keys.Space))
                Opening = false;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();
            if (Opening)
                _spriteBatch.Draw(TxOpening,GraphicsDevice.Viewport.Bounds, Color.White);
            else
                _spriteBatch.Draw(TxGameOver,GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.End();

            // for some reason the game runs but when i press the space bar to interact in crashes

            base.Draw(gameTime);
        }
    }
}
