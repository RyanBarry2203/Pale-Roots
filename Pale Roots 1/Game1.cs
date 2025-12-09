using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Engines; // Using your engine namespace
//using GP01Week11_Lab2_2025; // Using your InputEngine namespace

namespace Pale_Roots_1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D TileMap;
        
        // The Engine that manages the game logic
        private ChaseAndFireEngine _gameEngine;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Initialize InputEngine immediately so it's ready
            new InputEngine(this);  
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            TileMap = Content.Load<Texture2D>("tank tiles 64 x 64");

            // Initialize the Logic Engine
            _gameEngine = new ChaseAndFireEngine(this);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Pass the update call to your engine
            _gameEngine.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            _spriteBatch.Begin();

            // Let the engine draw everything
            _gameEngine.Draw(gameTime);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}