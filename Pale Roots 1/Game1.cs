using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Game1 is the entry point and global coordinator.
    // It owns the life cycle of the application: Initialize -> Load -> Update -> Draw.
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch { get; private set; }

        // --- ENGINE MANAGERS ---
        // These are your custom-built "sub-engines" that handle specific tasks.
        public GameStateManager StateManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public CutsceneManager CutsceneManager { get; private set; }

        // --- GAMEPLAY SYSTEMS ---
        // These represent the "live" state of the current playthrough.
        public ChaseAndFireEngine GameEngine { get; set; }
        public UpgradeManager UpgradeManager { get; set; }
        public List<UpgradeManager.UpgradeOption> CurrentUpgradeOptions { get; set; }

        // --- GLOBAL ASSETS ---
        // Assets loaded once and shared across all states to save memory.
        public Texture2D UiPixel { get; private set; }
        public SpriteFont UiFont { get; private set; }
        public Texture2D MenuBackground { get; private set; }
        public Texture2D[] SpellIcons { get; private set; }
        public Texture2D DashIcon { get; private set; }
        public Texture2D HeavyAttackIcon { get; private set; }
        public Texture2D BossIcon { get; private set; }

        // --- PROGRESSION TRACKING ---
        public bool HasStarted { get; set; } = false;
        public int PreviousLevelThreshold { get; set; } = 0;
        public int NextLevelThreshold { get; set; } = 3;
        public int LevelStep { get; set; } = 4;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Register the InputEngine component so it updates automatically every frame.
            new InputEngine(this);
            AudioManager = new AudioManager();
        }

        protected override void Initialize()
        {
            // Set the resolution to standard 1080p and lock it to full screen.
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

            // --- UI & CORE ASSETS ---
            UiPixel = new Texture2D(GraphicsDevice, 1, 1);
            UiPixel.SetData(new[] { Color.White });

            // We use try-catch blocks here so the game doesn't crash if a font or background is missing.
            try { UiFont = Content.Load<SpriteFont>("cutsceneFont"); } catch { }
            try { MenuBackground = Content.Load<Texture2D>("menu_background"); } catch { }

            // --- AUDIO LOADING ---
            // Songs are streamed from the disk rather than loaded fully into RAM.
            AudioManager.MenuSong = Content.Load<Song>("PaleRootsMenu");
            AudioManager.IntroSong = Content.Load<Song>("Whimsy");
            AudioManager.DeathSong = Content.Load<Song>("Sad");
            AudioManager.OutroSong = Content.Load<Song>("Ihavenoidea");

            // Adding multiple songs to the combat pool for variety.
            AudioManager.AddCombatSong(Content.Load<Song>("Guitar"));
            AudioManager.AddCombatSong(Content.Load<Song>("MoreGuitar"));
            AudioManager.AddCombatSong(Content.Load<Song>("Groovy"));

            // --- ICON LOADING ---
            SpellIcons = new Texture2D[6];
            for (int i = 0; i < 6; i++) { /* Actual loading logic as seen in your code */ }

            // --- ENGINE SETUP ---
            UIManager = new UIManager(UiPixel, UiFont);
            CutsceneManager = new CutsceneManager(this);
            CutsceneLibrary.LoadAllCutscenes(CutsceneManager, this);

            // --- CONTROL SCHEME DEFINITION ---
            // This is the "Action Mapping" system. We bind logical actions to physical keys.
            InputEngine.RegisterBinding("MoveUp", Keys.W);
            InputEngine.RegisterBinding("MoveDown", Keys.S);
            InputEngine.RegisterBinding("MoveLeft", Keys.A);
            InputEngine.RegisterBinding("MoveRight", Keys.D);
            InputEngine.RegisterBinding("Dash", Keys.LeftShift);
            InputEngine.RegisterBinding("Confirm", Keys.Space);
            InputEngine.RegisterBinding("Exit", Keys.Escape);

            InputEngine.RegisterBinding("CastSpell1", Keys.D1);
            InputEngine.RegisterBinding("CastSpell2", Keys.D2);

            InputEngine.RegisterMouseBinding("LightAttack", 0); // 0 = Left Mouse
            InputEngine.RegisterMouseBinding("HeavyAttack", 1); // 1 = Right Mouse

            // Initialize the gameplay engine and boot into the main menu.
            SoftResetGame();
            StateManager.ChangeState(new MenuState(this));
        }

        // Called to restart the game state without reloading textures from the hard drive.
        public void SoftResetGame()
        {
            GameEngine = new ChaseAndFireEngine(this);
            UpgradeManager = new UpgradeManager(
                GameEngine.GetPlayer(),
                GameEngine.GetSpellManager(),
                SpellIcons, DashIcon, HeavyAttackIcon, BossIcon,
                GraphicsDevice
            );
            PreviousLevelThreshold = 0;
            NextLevelThreshold = 3;
            InputEngine.ClearState();
        }

        protected override void Update(GameTime gameTime)
        {
            // Every frame, we update audio and then whatever logic is inside the current GameState.
            AudioManager.Update(gameTime);
            StateManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen to black before drawing the new frame.
            GraphicsDevice.Clear(Color.Black);

            // The StateManager handles the SpriteBatch.Begin/End calls inside its own Draw method.
            StateManager.Draw(gameTime, SpriteBatch, GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}