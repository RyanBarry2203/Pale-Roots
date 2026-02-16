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
            //_gameEngine = new ChaseAndFireEngine(this);

            warTheme = Content.Load<SoundEffect>("MoreGuitar"); 
            //warTheme.Play();

            _cutsceneManager = new CutsceneManager(this);

            // Load the 8 images
            Texture2D[] slides = new Texture2D[8];
            for (int i = 0; i < 8; i++)
            {
                // Assumes files are named "Cutscene1", "Cutscene2", etc.
                slides[i] = Content.Load<Texture2D>("cutscene_image_" + (i + 1));
            }

            // Load Music (Use MediaPlayer for long tracks)
            Song introMusic = Content.Load<Song>("Whimsy");
            MediaPlayer.Play(introMusic);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.2f;

            // --- CONFIGURE SLIDES (35 Seconds Total) ---
            float dur = 4375f; // 4.375 seconds in milliseconds

            // Slide 1: Zoom In
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[0], "The elders spoke of a gateway...", dur,
                1.0f, 1.2f, Vector2.Zero, Vector2.Zero));

            // Slide 2: Pan Right
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[1], "A bridge between stars and soil.", dur,
                1.1f, 1.1f, new Vector2(-50, 0), new Vector2(50, 0)));

            // Slide 3: Zoom Out
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[2], "But the connection was severed.", dur,
                1.3f, 1.0f, Vector2.Zero, Vector2.Zero));

            // Slide 4: Pan Up
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[3], "The sky turned to ash.", dur,
                1.2f, 1.2f, new Vector2(0, 50), new Vector2(0, -50)));

            // Slide 5: Slow Zoom In
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[4], "And the roots began to bleed.", dur,
                1.0f, 1.15f, Vector2.Zero, Vector2.Zero));

            // Slide 6: Pan Left
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[5], "We prayed for salvation.", dur,
                1.1f, 1.1f, new Vector2(50, 0), new Vector2(-50, 0)));

            // Slide 7: Zoom In Fast
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[6], "But only the King answered.", dur,
                1.0f, 1.4f, Vector2.Zero, Vector2.Zero));

            // Slide 8: Zoom Out (Final Reveal)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[7], "Now, the Pale Roots claim us all.", dur,
                1.2f, 1.0f, Vector2.Zero, Vector2.Zero));
            _gameEngine = new ChaseAndFireEngine(this);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (_currentState)
            {
                case GameState.Intro:
                    _cutsceneManager.Update(gameTime);

                    if (_cutsceneManager.IsFinished)
                    {
                        _currentState = GameState.Gameplay;


                        MediaPlayer.Volume = 1.0f;
                    }
                    break;


                    base.Update(gameTime);
            }
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