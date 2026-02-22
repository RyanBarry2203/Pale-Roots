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
        private SpriteBatch _spriteBatch;
        private Texture2D _uiPixel;
        private SpriteFont _uiFont;
        private Texture2D _menuBackground;

        private ChaseAndFireEngine _gameEngine;
        private CutsceneManager _cutsceneManager;

        // --- NEW UI MANAGER ---
        private UIManager _uiManager;

        // Menu Buttons
        private Rectangle _playBtnRect;
        private Rectangle _quitBtnRect;
        private List<Texture2D> _outroImages = new List<Texture2D>();

        // Input Safety
        private float _uiInputDelay = 0f;
        private const float SAFETY_DELAY = 0.5f;

        private AudioManager _audioManager;
        private int _nextLevelThreshold = 3;
        private int _levelStep = 4;

        private UpgradeManager _upgradeManager;
        private List<UpgradeManager.UpgradeOption> _currentUpgradeOptions;

        // Progression Logic
        private const int WIN_CONDITION_KILLS = 1;

        // Spell Icons
        private Texture2D[] _spellIcons;
        private Keys[] _spellKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6 };
        private Texture2D _dashIcon;
        private Texture2D _heavyAttackIcon;

        private List<Texture2D> _outroSlides = new List<Texture2D>();

        private float _creditsScrollY = 0f;
        private string _creditsText =
            "PALE ROOTS\n\n" +
            "A Game by Ryan Barry\n\n" +
            "PROGRAMMING\nRyan Barry\n\n" +
            "ART ASSETS\nItch Artists\n\n" +
            "MUSIC\nRyan Barry\n\n" +
            "SPECIAL THANKS\nPaul Powell\nNeil Gannon\n\n\n" +
            "Thank you for playing!";

        private GameState _currentState = GameState.Menu;
        private bool _hasStarted = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            new InputEngine(this);

            _audioManager = new AudioManager();
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            int centerW = _graphics.PreferredBackBufferWidth / 2;
            int centerH = _graphics.PreferredBackBufferHeight / 2;

            _playBtnRect = new Rectangle(centerW - 120, centerH - 40, 240, 50);
            _quitBtnRect = new Rectangle(centerW - 120, centerH + 40, 240, 50);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _audioManager.MenuSong = Content.Load<Song>("PaleRootsMenu");
            _audioManager.IntroSong = Content.Load<Song>("Whimsy");
            _audioManager.DeathSong = Content.Load<Song>("Sad");
            Song outro = Content.Load<Song>("Ihavenoidea");
            _audioManager.OutroSong = outro;
            _audioManager.AddCombatSong(Content.Load<Song>("Guitar"));
            _audioManager.AddCombatSong(Content.Load<Song>("MoreGuitar"));
            _audioManager.AddCombatSong(Content.Load<Song>("Groovy"));
            _audioManager.AddCombatSong(Content.Load<Song>("ihavenoidea"));
            _audioManager.AddCombatSong(Content.Load<Song>("uhm"));

            _spellIcons = new Texture2D[10];
            _spellIcons[1] = Content.Load<Texture2D>("Effects/SmiteIcon");
            _spellIcons[2] = Content.Load<Texture2D>("Effects/HolyNovaIcon");
            _spellIcons[3] = Content.Load<Texture2D>("Effects/HeavensFuryIcon");
            _spellIcons[4] = Content.Load<Texture2D>("Effects/HolyShieldIcon");
            _spellIcons[5] = Content.Load<Texture2D>("Effects/ElectricityIcon");
            _spellIcons[6] = Content.Load<Texture2D>("Effects/SwordJusticeIcon");
            _dashIcon = Content.Load<Texture2D>("Effects/DashIcon");
            _heavyAttackIcon = Content.Load<Texture2D>("Effects/HeavyIcon");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameEngine = new ChaseAndFireEngine(this);
            _upgradeManager = new UpgradeManager(
                _gameEngine.GetPlayer(),
                _gameEngine.GetSpellManager(),
                _spellIcons,
                _dashIcon,
                _heavyAttackIcon,
                GraphicsDevice
            );
            _cutsceneManager = new CutsceneManager(this);

            Texture2D[] slides = new Texture2D[15];
            for (int i = 1; i < 9; i++)
            {
                slides[i] = Content.Load<Texture2D>("cutscene_image_" + i);
            }

            for (int i = 1; i < 8; i++)
            {
                _outroSlides.Add(Content.Load<Texture2D>("outro_image_" + i));
            }

            _uiPixel = new Texture2D(GraphicsDevice, 1, 1);
            _uiPixel.SetData(new[] { Color.White });

            try { _uiFont = Content.Load<SpriteFont>("cutsceneFont"); } catch { }
            try { _menuBackground = Content.Load<Texture2D>("menu_background"); } catch { }

            // Init UIManager
            _uiManager = new UIManager(_uiPixel, _uiFont);

            float dur = 5500f;
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[1], "Decades ago Scholars discovered that the universe was not forged from nothing,\n it was brought to fruition by beings greater than our comprehension", dur + 2000, 1.0f, 1.05f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[2], "One of these beings known as Atun created humanity, in hopes in return he would get their devoted undying love.", dur, 1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[3], "But soon after humanity discovered it was he who made civilisation the cruel unforgiving reality it was,\n a rancorous feeling was left souring their tounge.", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[4], "Insulted, Atun withdrew any power he was yeilding to the world he once held so precious.", dur, 1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[5], "The Roots of his power went Pale, and the love his people had for him turned to blaising rage as\n they were forsaken further.", dur, 1.0f, 1.08f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[6], "In news of a weakened civilisation, Predatory colonys smelled blood in the waters of the Galaxy.", dur, 1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[7], "Led by Nivellin, a war Hero with inexplicable power. He had came to take back ther land that was once his to Rule.", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[8], "War was set in Motion, as was the Justice for Humanity", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));
        }

        protected override void Update(GameTime gameTime)
        {
            _audioManager.HandleMusicState(_currentState);
            _audioManager.Update(gameTime);

            switch (_currentState)
            {
                case GameState.Intro:
                    _cutsceneManager.Update(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) || _cutsceneManager.IsFinished)
                    {
                        _currentState = GameState.Gameplay;
                        _hasStarted = true;
                    }
                    break;
                case GameState.Outro:
                    _cutsceneManager.Update(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) || _cutsceneManager.IsFinished)
                    {
                        _currentState = GameState.Credits;
                        _creditsScrollY = GraphicsDevice.Viewport.Height;
                    }
                    break;
                case GameState.Menu:
                    IsMouseVisible = true;
                    MouseState ms = Mouse.GetState();
                    Point mousePoint = new Point(ms.X, ms.Y);

                    if (ms.LeftButton == ButtonState.Pressed)
                    {
                        if (_playBtnRect.Contains(mousePoint))
                        {
                            if (!_hasStarted) _currentState = GameState.Intro;
                            else _currentState = GameState.Gameplay;
                        }
                        else if (_quitBtnRect.Contains(mousePoint)) Exit();
                    }
                    break;
                case GameState.Credits:
                    _creditsScrollY -= 60f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) || _creditsScrollY < -1500f)
                    {
                        _currentState = GameState.Menu;
                        _hasStarted = false;
                        SoftResetGame();
                    }
                    break;
                case GameState.Gameplay:
                    if (InputEngine.IsKeyPressed(Keys.Escape)) _currentState = GameState.Menu;
                    if (_gameEngine != null)
                    {
                        _gameEngine.Update(gameTime);
                        if (_gameEngine.EnemiesKilled >= WIN_CONDITION_KILLS) _currentState = GameState.Victory;
                        if (!_gameEngine.GetPlayer().IsAlive) _currentState = GameState.GameOver;

                        if (_gameEngine.EnemiesKilled >= _nextLevelThreshold)
                        {
                            _currentUpgradeOptions = _upgradeManager.GetRandomOptions(3);
                            if (_currentUpgradeOptions.Count > 0)
                            {
                                _currentState = GameState.LevelUp;
                                _uiInputDelay = SAFETY_DELAY;
                            }
                            _levelStep += 4;
                            _nextLevelThreshold += _levelStep;
                        }
                    }
                    break;
                case GameState.LevelUp:
                    IsMouseVisible = true;
                    if (_uiInputDelay > 0) _uiInputDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    else HandleLevelUpInput();
                    break;
                case GameState.Victory:
                case GameState.GameOver:
                    IsMouseVisible = true;
                    HandleEndGameInput();
                    break;
            }
            base.Update(gameTime);
        }

        private void SoftResetGame()
        {
            _gameEngine = new ChaseAndFireEngine(this);
            _upgradeManager = new UpgradeManager(
                _gameEngine.GetPlayer(),
                _gameEngine.GetSpellManager(),
                _spellIcons,
                _dashIcon,
                _heavyAttackIcon,
                GraphicsDevice
            );
            _nextLevelThreshold = 3;
            _levelStep = 4;
            InputEngine.ClearState();
        }

        private void HandleLevelUpInput()
        {
            if (InputEngine.IsMouseLeftClick())
            {
                MouseState ms = Mouse.GetState();
                Rectangle screen = GraphicsDevice.Viewport.Bounds;
                int cardWidth = 200;
                int cardHeight = 300;
                int spacing = 50;
                int totalWidth = (_currentUpgradeOptions.Count * cardWidth) + ((_currentUpgradeOptions.Count - 1) * spacing);
                int startX = (screen.Width / 2) - (totalWidth / 2);
                int startY = (screen.Height / 2) - (cardHeight / 2);

                for (int i = 0; i < _currentUpgradeOptions.Count; i++)
                {
                    Rectangle cardRect = new Rectangle(startX + (i * (cardWidth + spacing)), startY, cardWidth, cardHeight);
                    if (cardRect.Contains(ms.Position))
                    {
                        _currentUpgradeOptions[i].ApplyAction.Invoke();
                        _currentState = GameState.Gameplay;
                        InputEngine.ClearState();
                        break;
                    }
                }
            }
        }

        private void HandleEndGameInput()
        {
            MouseState ms = Mouse.GetState();
            int centerW = GraphicsDevice.Viewport.Width / 2;
            int centerH = GraphicsDevice.Viewport.Height / 2;

            Rectangle playAgainRect = new Rectangle(centerW - 100, centerH, 200, 50);
            Rectangle quitRect = new Rectangle(centerW - 100, centerH + 70, 200, 50);

            if (InputEngine.IsMouseLeftClick())
            {
                if (playAgainRect.Contains(ms.Position))
                {
                    if (_currentState == GameState.Victory)
                    {
                        StartOutroSequence();
                    }
                    else
                    {
                        SoftResetGame();
                        _currentState = GameState.Gameplay;
                        _hasStarted = true;
                        _audioManager.Stop();
                    }
                    InputEngine.ClearState();
                }
                else if (quitRect.Contains(ms.Position))
                {
                    Exit();
                }
            }
        }

        private void StartOutroSequence()
        {
            _cutsceneManager.ClearSlides();
            float dur = 6000f;

            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[0], "The Skeleton King crumbles, his reign of bone and ash finally at an end.", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[1], "Without his dark magic, the Pale Roots begin to wither and retreat.", dur, 1.1f, 1.1f, new Vector2(-50, 0), new Vector2(50, 0)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[2], "Where death once choked the land, the first sprouts of green life return.", dur, 1.2f, 1.0f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[3], "The survivors emerge from the ruins, looking up at a clear sky for the first time in years.", dur, 1.1f, 1.1f, new Vector2(0, 50), new Vector2(0, -50)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[4], "We will rebuild. Not as subjects of a tyrant, but as free people.", dur, 1.0f, 1.1f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[5], "The scars of this war will remain, a reminder of what was lost.", dur, 1.05f, 1.15f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[6 ], "But today... today we celebrate the dawn.", dur + 3000, 1.1f, 1.1f, new Vector2(-30, -30), new Vector2(30, 30)));

            _currentState = GameState.Outro;
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
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _gameEngine._camera.CurrentCameraTranslation);
                    _gameEngine.Draw(gameTime, _spriteBatch);
                    _spriteBatch.End();

                    _spriteBatch.Begin();
                    _uiManager.DrawHUD(_spriteBatch, GraphicsDevice, _gameEngine, _spellIcons, _dashIcon, _heavyAttackIcon, WIN_CONDITION_KILLS);
                    _spriteBatch.End();
                    break;
                case GameState.Credits:
                    _spriteBatch.Begin();
                    GraphicsDevice.Clear(Color.Black);
                    string[] lines = _creditsText.Split('\n');
                    float currentY = _creditsScrollY;
                    float lineHeight = _uiFont.LineSpacing;

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            Vector2 lineSize = _uiFont.MeasureString(line);
                            Vector2 linePos = new Vector2((GraphicsDevice.Viewport.Width / 2) - (lineSize.X / 2), currentY);
                            _spriteBatch.DrawString(_uiFont, line, linePos, Color.White);
                        }
                        currentY += lineHeight;
                    }
                    _spriteBatch.End();
                    break;
                case GameState.LevelUp:
                case GameState.Victory:
                case GameState.GameOver:
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _gameEngine._camera.CurrentCameraTranslation);
                    _gameEngine.Draw(gameTime, _spriteBatch);
                    _spriteBatch.End();

                    _spriteBatch.Begin();
                    if (_currentState == GameState.LevelUp) _uiManager.DrawLevelUpScreen(_spriteBatch, GraphicsDevice, _currentUpgradeOptions, _upgradeManager);
                    else _uiManager.DrawEndScreen(_spriteBatch, GraphicsDevice, _currentState == GameState.Victory);
                    _spriteBatch.End();
                    break;
                case GameState.Outro:
                    _spriteBatch.Begin();
                    _cutsceneManager.Draw(_spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    _spriteBatch.End();
                    break;
                case GameState.Menu:
                    _spriteBatch.Begin();
                    if (_menuBackground != null)
                    {
                        _spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                    }
                    else
                    {
                        _spriteBatch.End();
                        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _gameEngine._camera.CurrentCameraTranslation);
                        _gameEngine.Draw(gameTime, _spriteBatch);
                        _spriteBatch.End();
                        _spriteBatch.Begin();
                    }
                    _uiManager.DrawMenu(_spriteBatch, GraphicsDevice, _playBtnRect, _quitBtnRect, _hasStarted);
                    _spriteBatch.End();
                    break;
            }
            base.Draw(gameTime);
        }
    }
}