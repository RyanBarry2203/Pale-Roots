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
        private Song introMusic;

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
            MediaPlayer.IsRepeating = false;
            MediaPlayer.Volume = 0.2f;

            // --- CONFIGURE SLIDES (35 Seconds Total) ---
            float dur = 5500f; // 4.375 seconds per slide

            // Slide 1: Very subtle Zoom In (1.0 -> 1.05)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[0], "Decades ago Scholars discovered that the universe was not forged from nothing,\n it was brought to fruition by beings greater than our comprehension", dur + 2000,
                1.0f, 1.05f, Vector2.Zero, Vector2.Zero));

            // Slide 2: Pan Right (Keep zoom steady at 1.05)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[1], "One of these beings known as Atun created humanity, in hopes in return he would get their devoted unyeilding love.", dur,
                1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));

            // Slide 3: Zoom Out (1.1 -> 1.0)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[2], "But soon after humanity discovered it was he who made civilisation the cruel unforgiving reality it was,\n a rancorous feeling was left souring their tounge.", dur,
                1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

            // Slide 4: Pan Up (Steady zoom)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[3], "Insulted, Atun withdrew any power he was yeilding to the world he once held so precious.", dur,
                1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));

            // Slide 5: Slow Zoom In (1.0 -> 1.08)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[4], "The Roots of his power went Pale, and the love his people had for him turned to blaising rage as\n they were forsaken further.", dur,
                1.0f, 1.08f, Vector2.Zero, Vector2.Zero));

            // Slide 6: Pan Left
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[5], "In news of a weakened civilisation, Predatory colonys smelled blood in the waters of the Galaxy.", dur,
                1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));

            // Slide 7: Zoom In Fast (1.0 -> 1.15) - The King needs a bit more drama
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[6], "Led by Nivellin, a war Hero with inexplicable power. He had came to take back ther land that was once his to Rule.", dur,
                1.0f, 1.15f, Vector2.Zero, Vector2.Zero));

            // Slide 8: Zoom Out Final (1.1 -> 1.0)
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(
                slides[7], "War was set in Motion, as was the Justice for Humanity", dur,
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


            //if (Microsoft.Xna.Framework.Media.MediaPlayer.State != Microsoft.Xna.Framework.Media.MediaState.Playing)
            //{
            //    if (warTheme.Play() != true)
            //    warTheme.Play();
            //}

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