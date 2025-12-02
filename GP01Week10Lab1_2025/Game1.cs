using Engines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tracker.WebAPIClient;

namespace GP01Week10Lab1_2025
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ChaseAndFireEngine _chaseAndFireEngine;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            //_chaseAndFireEngine = new ChaseAndFireEngine(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 10 Lab 1", Task: "Changing Chase and Fire engine");


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _chaseAndFireEngine = new ChaseAndFireEngine(this);
            _chaseAndFireEngine.LoadContent();


            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            
            _chaseAndFireEngine.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            
            _chaseAndFireEngine.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
