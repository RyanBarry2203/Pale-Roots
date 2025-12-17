using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pale_Roots_1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SoundEffect warTheme;

        // The Engine now owns the Player, Enemies, Allies, and Camera
        private ChaseAndFireEngine _gameEngine;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            new InputEngine(this); // Keeps your Input helper working
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameEngine = new ChaseAndFireEngine(this);

            // --- AUDIO CODE ---
            // 1. Load the song (Make sure the file is in Content and named "BattleTheme" or whatever your file is!)
            // If your file is called "music.mp3", putting "music" here usually works.

            warTheme = Content.Load<SoundEffect>("war theme"); // <--- CHANGE "BattleTheme" TO YOUR FILE NAME

            warTheme.Play();

        }

        protected override void Update(GameTime gameTime)
        {
            // Exit on Escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // All game logic (movement, charging, camera zoom) happens here now
            _gameEngine.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // 1. Begin Draw with the Camera Matrix from the Engine
            // This ensures the Zoom and Follow logic affects everything we draw
            _spriteBatch.Begin(transformMatrix: _gameEngine._camera.CurrentCameraTranslation);

            // 2. Tell the Engine to draw everything (Map, Player, Armies)
            _gameEngine.Draw(gameTime, _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}