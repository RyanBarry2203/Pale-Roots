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

        //// --- BOSS BATTLE STATE PRESERVATION ---
        //public GameplayState SavedMainGameState { get; set; }

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
        public Texture2D BossIcon { get; private set; }

        // --- GLOBAL STATE ---
        public bool HasStarted { get; set; } = false;
        public int PreviousLevelThreshold { get; set; } = 0;
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
            _graphics.IsFullScreen = true;
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
            BossIcon = Content.Load<Texture2D>("Effects/BossIcon");


            // Initialize UI Manager
            UIManager = new UIManager(UiPixel, UiFont);

            // Initialize Cutscene Manager & Load Data
            CutsceneManager = new CutsceneManager(this);
            CutsceneLibrary.LoadAllCutscenes(CutsceneManager, this);

            // --- INPUT CONFIGURATION (Engine Feature) ---
            // This defines the "Control Scheme". In a full engine, this could be loaded from a JSON file.
            InputEngine.RegisterBinding("MoveUp", Keys.W);
            InputEngine.RegisterBinding("MoveDown", Keys.S);
            InputEngine.RegisterBinding("MoveLeft", Keys.A);
            InputEngine.RegisterBinding("MoveRight", Keys.D);

            InputEngine.RegisterBinding("Dash", Keys.LeftShift);
            InputEngine.RegisterBinding("Confirm", Keys.Space);
            InputEngine.RegisterBinding("Exit", Keys.Escape);

            // Spells
            InputEngine.RegisterBinding("CastSpell1", Keys.D1);
            InputEngine.RegisterBinding("CastSpell2", Keys.D2);
            InputEngine.RegisterBinding("CastSpell3", Keys.D3);
            InputEngine.RegisterBinding("CastSpell4", Keys.D4);
            InputEngine.RegisterBinding("CastSpell5", Keys.D5);
            InputEngine.RegisterBinding("CastSpell6", Keys.D6);

            // Mouse Actions
            InputEngine.RegisterMouseBinding("LightAttack", 0); // Left Click
            InputEngine.RegisterMouseBinding("HeavyAttack", 1); // Right Click

            // Initialize the Gameplay Engine
            SoftResetGame();

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
                BossIcon,
                GraphicsDevice
            );
            PreviousLevelThreshold = 0;
            NextLevelThreshold = 3;
            LevelStep = 4;
            InputEngine.ClearState();
        }

        protected override void Update(GameTime gameTime)
        {
            AudioManager.Update(gameTime);
            StateManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            StateManager.Draw(gameTime, SpriteBatch, GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}