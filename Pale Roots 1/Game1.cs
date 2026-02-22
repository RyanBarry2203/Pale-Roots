using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch { get; private set; }

        // --- ENGINE MANAGERS (Custom APIs) ---
        public GameStateManager StateManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public CutsceneManager CutsceneManager { get; private set; }

        // --- GAMEPLAY SYSTEMS ---
        public ChaseAndFireEngine GameEngine { get; set; }
        public UpgradeManager UpgradeManager { get; set; }
        public List<UpgradeManager.UpgradeOption> CurrentUpgradeOptions { get; set; }

        // --- GLOBAL ASSETS ---
        public Texture2D UiPixel { get; private set; }
        public SpriteFont UiFont { get; private set; }
        public Texture2D MenuBackground { get; private set; }
        public Texture2D[] SpellIcons { get; private set; }
        public Texture2D DashIcon { get; private set; }
        public Texture2D HeavyAttackIcon { get; private set; }
        public List<Texture2D> OutroSlides { get; private set; } = new List<Texture2D>();

        // --- GLOBAL STATE ---
        public bool HasStarted { get; set; } = false;
        public int NextLevelThreshold { get; set; } = 3;
        public int LevelStep { get; set; } = 4;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            new InputEngine(this);
            AudioManager = new AudioManager();
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            StateManager = new GameStateManager();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize UI Assets
            UiPixel = new Texture2D(GraphicsDevice, 1, 1);
            UiPixel.SetData(new[] { Color.White });
            try { UiFont = Content.Load<SpriteFont>("cutsceneFont"); } catch { }
            try { MenuBackground = Content.Load<Texture2D>("menu_background"); } catch { }

            // Load Audio
            AudioManager.MenuSong = Content.Load<Song>("PaleRootsMenu");
            AudioManager.IntroSong = Content.Load<Song>("Whimsy");
            AudioManager.DeathSong = Content.Load<Song>("Sad");
            AudioManager.OutroSong = Content.Load<Song>("Ihavenoidea");
            AudioManager.AddCombatSong(Content.Load<Song>("Guitar"));
            AudioManager.AddCombatSong(Content.Load<Song>("MoreGuitar"));
            AudioManager.AddCombatSong(Content.Load<Song>("Groovy"));
            AudioManager.AddCombatSong(Content.Load<Song>("ihavenoidea"));
            AudioManager.AddCombatSong(Content.Load<Song>("uhm"));

            // Load Spell Icons
            SpellIcons = new Texture2D[6];
            SpellIcons[0] = Content.Load<Texture2D>("Effects/SmiteIcon");
            SpellIcons[1] = Content.Load<Texture2D>("Effects/HolyNovaIcon");
            SpellIcons[2] = Content.Load<Texture2D>("Effects/HeavensFuryIcon");
            SpellIcons[3] = Content.Load<Texture2D>("Effects/HolyShieldIcon");
            SpellIcons[4] = Content.Load<Texture2D>("Effects/ElectricityIcon");
            SpellIcons[5] = Content.Load<Texture2D>("Effects/SwordJusticeIcon");

            DashIcon = Content.Load<Texture2D>("Effects/DashIcon");
            HeavyAttackIcon = Content.Load<Texture2D>("Effects/HeavyIcon");

            // Load Outro Slides
            for (int i = 1; i < 8; i++)
            {
                OutroSlides.Add(Content.Load<Texture2D>("outro_image_" + i));
            }

            // Initialize UI & Cutscene Managers
            UIManager = new UIManager(UiPixel, UiFont);
            CutsceneManager = new CutsceneManager(this);

            // Initialize the Gameplay Engine
            SoftResetGame();

            // Setup Intro Cutscene
            Texture2D[] slides = new Texture2D[9];
            for (int i = 1; i < 9; i++) slides[i] = Content.Load<Texture2D>("cutscene_image_" + i);

            float dur = 5500f;
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[1], "Decades ago Scholars discovered that the universe was not forged from nothing,\n it was brought to fruition by beings greater than our comprehension", dur + 2000, 1.0f, 1.05f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[2], "One of these beings known as Atun created humanity, in hopes in return he would get their devoted undying love.", dur, 1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[3], "But soon after humanity discovered it was he who made civilisation the cruel unforgiving reality it was,\n a rancorous feeling was left souring their tounge.", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[4], "Insulted, Atun withdrew any power he was yeilding to the world he once held so precious.", dur, 1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[5], "The Roots of his power went Pale, and the love his people had for him turned to blaising rage as\n they were forsaken further.", dur, 1.0f, 1.08f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[6], "In news of a weakened civilisation, Predatory colonys smelled blood in the waters of the Galaxy.", dur, 1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[7], "Led by Nivellin, a war Hero with inexplicable power. He had came to take back ther land that was once his to Rule.", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[8], "War was set in Motion, as was the Justice for Humanity", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

            // START THE ENGINE IN THE MENU STATE
            StateManager.ChangeState(new MenuState(this));
        }

        public void SoftResetGame()
        {
            GameEngine = new ChaseAndFireEngine(this);
            UpgradeManager = new UpgradeManager(
                GameEngine.GetPlayer(),
                GameEngine.GetSpellManager(),
                SpellIcons,
                DashIcon,
                HeavyAttackIcon,
                GraphicsDevice
            );
            NextLevelThreshold = 3;
            LevelStep = 4;
            InputEngine.ClearState();
        }

        public void StartOutroSequence()
        {
            CutsceneManager.ClearSlides();
            float dur = 6000f;
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(OutroSlides[0], "The Skeleton King crumbles, his reign of bone and ash finally at an end.", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(OutroSlides[1], "Without his dark magic, the Pale Roots begin to wither and retreat.", dur, 1.1f, 1.1f, new Vector2(-50, 0), new Vector2(50, 0)));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(OutroSlides[2], "Where death once choked the land, the first sprouts of green life return.", dur, 1.2f, 1.0f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(OutroSlides[3], "The survivors emerge from the ruins, looking up at a clear sky for the first time in years.", dur, 1.1f, 1.1f, new Vector2(0, 50), new Vector2(0, -50)));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(OutroSlides[4], "We will rebuild. Not as subjects of a tyrant, but as free people.", dur, 1.0f, 1.1f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(OutroSlides[5], "The scars of this war will remain, a reminder of what was lost.", dur, 1.05f, 1.15f, Vector2.Zero, Vector2.Zero));
            CutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(OutroSlides[6], "But today... today we celebrate the dawn.", dur + 3000, 1.1f, 1.1f, new Vector2(-30, -30), new Vector2(30, 30)));

            // StateManager.ChangeState(new OutroState(this)); // Uncomment when OutroState exists
        }

        protected override void Update(GameTime gameTime)
        {
            AudioManager.Update(gameTime);
            StateManager.Update(gameTime); // DELEGATED TO STATE
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            StateManager.Draw(gameTime, SpriteBatch, GraphicsDevice); // DELEGATED TO STATE
            base.Draw(gameTime);
        }
    }
}