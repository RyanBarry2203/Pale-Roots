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
        private enum GameState
        {
            Intro,
            Gameplay
        }

        private GameState _currentState = GameState.Intro;
        private CutsceneManager _cutsceneManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            new InputEngine(this); // Keeps your Input helper working
        }

        protected override void Initialize()
        {
            // Fix: Set backbuffer size using _graphics, not GraphicsDevice
            //_graphics.PreferredBackBufferWidth = 960;
            //_graphics.PreferredBackBufferHeight = 540;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameEngine = new ChaseAndFireEngine(this);

            warTheme = Content.Load<SoundEffect>("MoreGuitar"); 
            //warTheme.Play();

            _cutsceneManager = new CutsceneManager(this);

            // Load the 8 images
            Texture2D[] slides = new Texture2D[8];
            for (int i = 0; i < 8; i++)
            {
                
                slides[i] = Content.Load<Texture2D>("cutscene_image_" + (i + 1));
            }

            // Load Music (Use MediaPlayer for long tracks)
            Song introMusic = Content.Load<Song>("Whimsy");
            MediaPlayer.Play(introMusic);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.2f;

            // --- CONFIGURE SLIDES (35 Seconds Total) ---
            float dur = 4500f; // 4.375 seconds per slide

            // Slide 1: Very subtle Zoom In (1.0 -> 1.05)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[0], "The elders spoke of a gateway...", dur,
                1.0f, 1.05f, Vector2.Zero, Vector2.Zero));

            // Slide 2: Pan Right (Keep zoom steady at 1.05)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[1], "A bridge between stars and soil.", dur,
                1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));

            // Slide 3: Zoom Out (1.1 -> 1.0)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[2], "But the connection was severed.", dur,
                1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

            // Slide 4: Pan Up (Steady zoom)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[3], "The sky turned to ash.", dur,
                1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));

            // Slide 5: Slow Zoom In (1.0 -> 1.08)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[4], "And the roots began to bleed.", dur,
                1.0f, 1.08f, Vector2.Zero, Vector2.Zero));

            // Slide 6: Pan Left
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[5], "We prayed for salvation.", dur,
                1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));

            // Slide 7: Zoom In Fast (1.0 -> 1.15) - The King needs a bit more drama
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[6], "But only the King answered.", dur,
                1.0f, 1.15f, Vector2.Zero, Vector2.Zero));

            // Slide 8: Zoom Out Final (1.1 -> 1.0)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[7], "Now, the Pale Roots claim us all.", dur,
                1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

        }

        //protected override void Update(GameTime gameTime)
        //{
        //    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        //        Exit();

        //    switch (_currentState)
        //    {
        //        case GameState.Intro:
        //            _cutsceneManager.Update(gameTime);

        //            if (_cutsceneManager.IsFinished)
        //            {
        //                _currentState = GameState.Gameplay;


        //                MediaPlayer.Volume = 1.0f;
        //            }
        //            break;


        //            base.Update(gameTime);
        //    }
        //}

        protected override void Update(GameTime gameTime)
        {
            // 1. Global Exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // 2. STATE MACHINE
            switch (_currentState)
            {
                case GameState.Intro:
                    // A. Update the Cutscene
                    _cutsceneManager.Update(gameTime);

                    // B. Check if it just finished
                    if (_cutsceneManager.IsFinished)
                    {
                        // SWITCH STATE NOW
                        _currentState = GameState.Gameplay;

                        // Optional: Turn volume up
                        Microsoft.Xna.Framework.Media.MediaPlayer.Volume = 1.0f;
                    }
                    break;

                case GameState.Gameplay:

                    if (_gameEngine != null)
                    {
                        _gameEngine.Update(gameTime);
                    }
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (_currentState)
            {
                case GameState.Intro:
                    _spriteBatch.Begin();
                    _cutsceneManager.Draw(_spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    _spriteBatch.End();
                    break;

                case GameState.Gameplay:
                    _spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        _gameEngine._camera.CurrentCameraTranslation);

                    _gameEngine.Draw(gameTime, _spriteBatch);
                    _spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }
    }
}