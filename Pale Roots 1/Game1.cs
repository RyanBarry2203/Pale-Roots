using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Remove 'TileMap' and 'layer' variables from here. 
        // The Engine and LevelManager own them now.

        private ChaseAndFireEngine _gameEngine;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            new InputEngine(this);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Initialize the Engine. It will load the LevelManager and the Map.
            _gameEngine = new ChaseAndFireEngine(this);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Just call Update. The Engine knows about the map internally.
            _gameEngine.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // The Engine draws the LevelManager (Map) and the Player
            _gameEngine.Draw(gameTime);

            // REMOVED: The foreach loop that was crashing because 'layer' was null.

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}