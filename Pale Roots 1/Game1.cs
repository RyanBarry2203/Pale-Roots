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

        // NEW: Background Texture for the menu
        private Texture2D _menuBackground;

        private ChaseAndFireEngine _gameEngine;
        private CutsceneManager _cutsceneManager;

        // Menu Buttons
        private Rectangle _playBtnRect;
        private Rectangle _quitBtnRect;

        private List<Texture2D> _outroImages = new List<Texture2D>();

        // Input Safety
        private float _uiInputDelay = 0f;
        private const float SAFETY_DELAY = 0.5f;

        // --- NEW AUDIO MANAGER ---
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

        // NEW: Outro List
        private List<Texture2D> _outroSlides = new List<Texture2D>();

        // UI Colors (Medieval Sci-Fantasy Theme)
        Color _hudColor = new Color(10, 10, 15, 200);
        Color _healthColor = new Color(180, 20, 20);
        Color _staminaColor = new Color(50, 205, 50);

        private float _creditsScrollY = 0f;
        private string _creditsText =
            "PALE ROOTS\n\n" +
            "A Game by Ryan Barry\n\n" +
            "PROGRAMMING\nRyan Barry\n\n" +
            "ART ASSETS\nItch Artists\n\n" +
            "MUSIC\nRyan Barry\n\n" +
            "SPECIAL THANKS\nPaul Powell\nNeil Gannon\n\n\n" +
            "Thank you for playing!";

        // Button Styling
        Color _btnNormal = new Color(20, 20, 30, 220);
        Color _btnHover = new Color(40, 60, 100, 240);
        Color _borderNormal = new Color(60, 60, 80);
        Color _borderHover = Color.Cyan;

        // GameState is now in GameState.cs (Global Enum)
        private GameState _currentState = GameState.Menu;
        private bool _hasStarted = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            new InputEngine(this);

            // Initialize the Audio Manager
            _audioManager = new AudioManager();
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            // Setup Menu Buttons (Centered)
            int centerW = _graphics.PreferredBackBufferWidth / 2;
            int centerH = _graphics.PreferredBackBufferHeight / 2;

            // Make buttons slightly wider and thinner for a sleeker look
            _playBtnRect = new Rectangle(centerW - 120, centerH - 40, 240, 50);
            _quitBtnRect = new Rectangle(centerW - 120, centerH + 40, 240, 50);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // --- AUDIO LOADING (Delegated to Manager) ---
            _audioManager.MenuSong = Content.Load<Song>("PaleRootsMenu");
            _audioManager.IntroSong = Content.Load<Song>("Whimsy");
            _audioManager.DeathSong = Content.Load<Song>("Sad");

            // Load Outro once, assign to Outro
            Song outro = Content.Load<Song>("Ihavenoidea");
            _audioManager.OutroSong = outro;

            _audioManager.AddCombatSong(Content.Load<Song>("Guitar"));
            _audioManager.AddCombatSong(Content.Load<Song>("MoreGuitar"));
            _audioManager.AddCombatSong(Content.Load<Song>("Groovy"));
            _audioManager.AddCombatSong(Content.Load<Song>("ihavenoidea"));
            _audioManager.AddCombatSong(Content.Load<Song>("uhm"));
            // --------------------------------------------

            _spellIcons = new Texture2D[6];
            _spellIcons[0] = Content.Load<Texture2D>("Effects/SmiteIcon");
            _spellIcons[1] = Content.Load<Texture2D>("Effects/HolyNovaIcon");
            _spellIcons[2] = Content.Load<Texture2D>("Effects/HeavensFuryIcon");
            _spellIcons[3] = Content.Load<Texture2D>("Effects/HolyShieldIcon");
            _spellIcons[4] = Content.Load<Texture2D>("Effects/ElectricityIcon");
            _spellIcons[5] = Content.Load<Texture2D>("Effects/SwordJusticeIcon");

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
            Texture2D[] slides = new Texture2D[9];
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

            try { _uiFont = Content.Load<SpriteFont>("cutsceneFont"); }
            catch { }

            try { _menuBackground = Content.Load<Texture2D>("menu_background"); }
            catch { }

            // Cutscene Setup
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
            // --- UPDATE AUDIO MANAGER ---
            _audioManager.HandleMusicState(_currentState);
            _audioManager.Update(gameTime);
            // ----------------------------

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
                    // Scroll text upwards
                    _creditsScrollY -= 60f * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Check for Spacebar OR if text has scrolled off screen
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) || _creditsScrollY < -1500f)
                    {
                        // 1. Change State to Menu
                        _currentState = GameState.Menu;
                        _hasStarted = false;

                        // 2. Perform Soft Reset (Reset gameplay, keep audio/assets alive)
                        SoftResetGame();
                    }
                    break;

                case GameState.Gameplay:
                    if (InputEngine.IsKeyPressed(Keys.Escape)) _currentState = GameState.Menu;

                    if (_gameEngine != null)
                    {
                        _gameEngine.Update(gameTime);

                        // Win
                        if (_gameEngine.EnemiesKilled >= WIN_CONDITION_KILLS) _currentState = GameState.Victory;

                        // Loss
                        if (!_gameEngine.GetPlayer().IsAlive) _currentState = GameState.GameOver;

                        // Level Up
                        if (_gameEngine.EnemiesKilled >= _nextLevelThreshold)
                        {
                            _currentUpgradeOptions = _upgradeManager.GetRandomOptions(3);
                            if (_currentUpgradeOptions.Count > 0)
                            {
                                _currentState = GameState.LevelUp;
                                _uiInputDelay = SAFETY_DELAY; // START THE DELAY TIMER
                            }
                            _levelStep += 4; // Smaller step (easier)
                            _nextLevelThreshold += _levelStep;
                        }
                    }
                    break;

                case GameState.LevelUp:
                    IsMouseVisible = true;
                    // Decrement Timer
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
            // Reset Game Engine (Player, Level, Enemies)
            _gameEngine = new ChaseAndFireEngine(this);

            // Re-link the UpgradeManager to the NEW Player/SpellManager
            _upgradeManager = new UpgradeManager(
                _gameEngine.GetPlayer(),
                _gameEngine.GetSpellManager(),
                _spellIcons,
                _dashIcon,
                _heavyAttackIcon,
                GraphicsDevice
            );

            // Reset Progression Variables
            _nextLevelThreshold = 3;
            _levelStep = 4;

            // Reset Input State so we don't accidentally click something immediately
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

                        // Stop audio on death/restart so it resets
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

            // Slide 1: Zoom In
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[0],
                "The Skeleton King crumbles, his reign of bone and ash finally at an end.",
                dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));

            // Slide 2: Pan Right
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[1],
                "Without his dark magic, the Pale Roots begin to wither and retreat.",
                dur, 1.1f, 1.1f, new Vector2(-50, 0), new Vector2(50, 0)));

            // Slide 3: Zoom Out
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[2],
                "Where death once choked the land, the first sprouts of green life return.",
                dur, 1.2f, 1.0f, Vector2.Zero, Vector2.Zero));

            // Slide 4: Pan Up
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[3],
                "The survivors emerge from the ruins, looking up at a clear sky for the first time in years.",
                dur, 1.1f, 1.1f, new Vector2(0, 50), new Vector2(0, -50)));

            // Slide 5: Zoom In
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[4],
                "We will rebuild. Not as subjects of a tyrant, but as free people.",
                dur, 1.0f, 1.1f, Vector2.Zero, Vector2.Zero));

            // Slide 6: Static -> Zoom
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[5],
                "The scars of this war will remain, a reminder of what was lost.",
                dur, 1.05f, 1.15f, Vector2.Zero, Vector2.Zero));

            // Slide 7: Slow Pan
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroSlides[6],
                "But today... today we celebrate the dawn.",
                dur + 3000, 1.1f, 1.1f, new Vector2(-30, -30), new Vector2(30, 30)));

            _currentState = GameState.Outro;
        }

        private void DrawLevelUpScreen()
        {
            _spriteBatch.Draw(_uiPixel, GraphicsDevice.Viewport.Bounds, Color.Black * 0.7f);

            string title = "ALTERNATE PHYSICS WITH THE STRENGTH OF THE DEAD";
            Vector2 size = _uiFont.MeasureString(title);
            _spriteBatch.DrawString(_uiFont, title, new Vector2(GraphicsDevice.Viewport.Width / 2 - size.X / 2, 100), Color.Gold);

            Rectangle screen = GraphicsDevice.Viewport.Bounds;
            int cardWidth = 200;
            int cardHeight = 300;
            int spacing = 50;
            int totalWidth = (_currentUpgradeOptions.Count * cardWidth) + ((_currentUpgradeOptions.Count - 1) * spacing);
            int startX = (screen.Width / 2) - (totalWidth / 2);
            int startY = (screen.Height / 2) - (cardHeight / 2);

            Point mousePos = Mouse.GetState().Position;

            for (int i = 0; i < _currentUpgradeOptions.Count; i++)
            {
                Rectangle cardRect = new Rectangle(startX + (i * (cardWidth + spacing)), startY, cardWidth, cardHeight);
                bool hover = cardRect.Contains(mousePos);
                _upgradeManager.DrawCard(_spriteBatch, cardRect, _currentUpgradeOptions[i], hover, _uiFont);
            }
        }

        private void DrawEndScreen(bool victory)
        {
            _spriteBatch.Draw(_uiPixel, GraphicsDevice.Viewport.Bounds, Color.Black * 0.85f);

            string title = victory ? "VICTORY" : "YOU DIED";
            Color color = victory ? Color.Gold : Color.Red;

            Vector2 size = _uiFont.MeasureString(title);
            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            _spriteBatch.DrawString(_uiFont, title, new Vector2(center.X - size.X, center.Y - 150), color, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

            if (victory)
            {
                string lore = "The Skeleton King is defeated.\nThe Pale Roots recede.";
                Vector2 loreSize = _uiFont.MeasureString(lore);
                _spriteBatch.DrawString(_uiFont, lore, new Vector2(center.X - loreSize.X / 2, center.Y - 80), Color.White);
            }

            Rectangle btn1Rect = new Rectangle((int)center.X - 100, (int)center.Y, 200, 50);
            Rectangle btn2Rect = new Rectangle((int)center.X - 100, (int)center.Y + 70, 200, 50);

            Point mousePos = Mouse.GetState().Position;

            void DrawBtn(Rectangle r, string t)
            {
                bool hover = r.Contains(mousePos);
                _spriteBatch.Draw(_uiPixel, r, hover ? Color.Gray : Color.DarkGray);

                int b = 2;
                Color bc = hover ? Color.White : Color.Black;
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Y, r.Width, b), bc);
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Bottom - b, r.Width, b), bc);
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Y, b, r.Height), bc);
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.Right - b, r.Y, b, r.Height), bc);

                Vector2 ts = _uiFont.MeasureString(t);
                _spriteBatch.DrawString(_uiFont, t, new Vector2(r.Center.X - ts.X / 2, r.Center.Y - ts.Y / 2), Color.White);
            }

            string btn1Text = victory ? "FINISH GAME" : "PLAY AGAIN";

            DrawBtn(btn1Rect, btn1Text);
            DrawBtn(btn2Rect, "QUIT");
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
                    DrawHUD();
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
                            Vector2 linePos = new Vector2(
                                (GraphicsDevice.Viewport.Width / 2) - (lineSize.X / 2),
                                currentY
                            );
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
                    if (_currentState == GameState.LevelUp) DrawLevelUpScreen();
                    else DrawEndScreen(_currentState == GameState.Victory);
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
                    DrawMenu();
                    _spriteBatch.End();
                    break;
            }
            base.Draw(gameTime);
        }

        private void DrawHUD()
        {
            Player p = _gameEngine.GetPlayer();
            if (p == null) return;

            int padding = 20;
            int barHeight = 25;
            int barWidth = 300;

            _spriteBatch.Draw(_uiPixel, new Rectangle(padding, padding, barWidth + 4, (barHeight * 2) + 15), _hudColor);

            float hpPercent = (float)p.Health / p.MaxHealth;
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, barWidth, barHeight), Color.Black * 0.5f);
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, (int)(barWidth * hpPercent), barHeight), _healthColor);

            float dashRatio = 0f;
            if (p.DashDuration > 0) dashRatio = 1.0f - (p.DashTimer / p.DashDuration);
            else dashRatio = 1.0f;
            dashRatio = MathHelper.Clamp(dashRatio, 0f, 1f);

            int stamY = padding + barHeight + 8;
            int stamH = barHeight / 2;
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, barWidth, stamH), Color.Black * 0.5f);
            Color currentStaminaColor = (dashRatio >= 0.99f) ? _staminaColor : Color.Orange;
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, (int)(barWidth * dashRatio), stamH), currentStaminaColor);

            int screenW = GraphicsDevice.Viewport.Width;
            int barW = 600;
            int barH = 20;
            int barX = (screenW / 2) - (barW / 2);
            int barY = 20;

            _spriteBatch.Draw(_uiPixel, new Rectangle(barX - 2, barY - 2, barW + 4, barH + 4), Color.Black);
            _spriteBatch.Draw(_uiPixel, new Rectangle(barX, barY, barW, barH), Color.DarkGray * 0.5f);

            float progress = (float)_gameEngine.EnemiesKilled / WIN_CONDITION_KILLS;
            if (progress > 1f) progress = 1f;
            _spriteBatch.Draw(_uiPixel, new Rectangle(barX, barY, (int)(barW * progress), barH), Color.Purple);

            string warText = "WAR DOMINANCE";
            Vector2 textSize = _uiFont.MeasureString(warText);
            _spriteBatch.DrawString(_uiFont, warText, new Vector2(screenW / 2 - textSize.X / 2, barY + barH + 5), Color.White);

            int iconSize = 64;
            int spacing = 20;
            int startY = GraphicsDevice.Viewport.Height - iconSize - 20;

            int unlockedSpells = 0;
            for (int i = 0; i < 6; i++) if (_gameEngine.GetSpellManager().IsSpellUnlocked(i)) unlockedSpells++;

            bool showDash = p.IsDashUnlocked;
            bool showHeavy = p.IsHeavyAttackUnlocked;

            int totalItems = unlockedSpells + (showDash ? 1 : 0) + (showHeavy ? 1 : 0);

            if (totalItems > 0)
            {
                int totalWidth = (totalItems * iconSize) + ((totalItems - 1) * spacing);
                int currentX = (screenW / 2) - (totalWidth / 2);

                Rectangle bgRect = new Rectangle(currentX - 10, startY - 10, totalWidth + 20, iconSize + 20);
                _spriteBatch.Draw(_uiPixel, bgRect, Color.Black * 0.6f);

                if (showDash)
                {
                    Rectangle dest = new Rectangle(currentX, startY, iconSize, iconSize);
                    _spriteBatch.Draw(_dashIcon, dest, Color.White);
                    _spriteBatch.DrawString(_uiFont, "SHFT", new Vector2(dest.X + 2, dest.Y + 2), Color.Gold, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);

                    if (p.DashTimer > 0)
                    {
                        float ratio = p.DashTimer / p.DashDuration;
                        int h = (int)(iconSize * ratio);
                        Rectangle cdRect = new Rectangle(dest.X, dest.Bottom - h, iconSize, h);
                        _spriteBatch.Draw(_uiPixel, cdRect, Color.Black * 0.7f);
                    }
                    currentX += iconSize + spacing;
                }

                if (showHeavy)
                {
                    Rectangle dest = new Rectangle(currentX, startY, iconSize, iconSize);
                    _spriteBatch.Draw(_heavyAttackIcon, dest, Color.White);
                    _spriteBatch.DrawString(_uiFont, "R-CLK", new Vector2(dest.X + 2, dest.Y + 2), Color.Gold, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    currentX += iconSize + spacing;
                }

                for (int i = 0; i < 6; i++)
                {
                    if (_gameEngine.GetSpellManager().IsSpellUnlocked(i))
                    {
                        Rectangle dest = new Rectangle(currentX, startY, iconSize, iconSize);

                        if (_spellIcons[i] != null)
                            _spriteBatch.Draw(_spellIcons[i], dest, Color.White);

                        string key = (i + 1).ToString();
                        _spriteBatch.DrawString(_uiFont, key, new Vector2(dest.X + 2, dest.Y + 2), Color.Gold);

                        Spell s = _gameEngine.GetSpellManager().GetSpell(i);
                        if (s != null && s.CurrentCooldown > 0)
                        {
                            float ratio = s.CurrentCooldown / s.CooldownDuration;
                            int h = (int)(iconSize * ratio);

                            Rectangle cdRect = new Rectangle(dest.X, dest.Bottom - h, iconSize, h);
                            _spriteBatch.Draw(_uiPixel, cdRect, Color.Black * 0.7f);

                            if (s.CurrentCooldown > 1000)
                            {
                                string sec = (s.CurrentCooldown / 1000).ToString("0");
                                Vector2 sz = _uiFont.MeasureString(sec);
                                _spriteBatch.DrawString(_uiFont, sec,
                                    new Vector2(dest.Center.X - sz.X / 2, dest.Center.Y - sz.Y / 2), Color.White);
                            }
                        }

                        currentX += iconSize + spacing;
                    }
                }
            }
        }

        private void DrawMenu()
        {
            int screenW = GraphicsDevice.Viewport.Width;
            int screenH = GraphicsDevice.Viewport.Height;

            _spriteBatch.Draw(_uiPixel, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.6f);

            Point mousePoint = new Point(Mouse.GetState().X, Mouse.GetState().Y);

            void DrawFancyButton(Rectangle rect, string text, bool isHovered)
            {
                Color fillColor = isHovered ? _btnHover : _btnNormal;
                Color borderColor = isHovered ? _borderHover : _borderNormal;

                _spriteBatch.Draw(_uiPixel, rect, fillColor);

                int borderThickness = 2;
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Bottom - borderThickness, rect.Width, borderThickness), borderColor);
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), borderColor);
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.Right - borderThickness, rect.Y, borderThickness, rect.Height), borderColor);

                if (_uiFont != null)
                {
                    Vector2 size = _uiFont.MeasureString(text);
                    Vector2 pos = new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2);
                    _spriteBatch.DrawString(_uiFont, text, pos, Color.White);
                }
            }

            bool playHover = _playBtnRect.Contains(mousePoint);
            DrawFancyButton(_playBtnRect, _hasStarted ? "RESUME" : "PLAY", playHover);

            bool quitHover = _quitBtnRect.Contains(mousePoint);
            DrawFancyButton(_quitBtnRect, "QUIT", quitHover);

            if (_uiFont != null)
            {
                string title = "PALE ROOTS";
                Vector2 titleSize = _uiFont.MeasureString(title);
                Vector2 titlePos = new Vector2(screenW / 2f, 200);
                Vector2 origin = titleSize / 2f;

                _spriteBatch.DrawString(_uiFont, title, titlePos + new Vector2(3, 3), Color.Black * 0.8f, 0f, origin, 2.5f, SpriteEffects.None, 0f);
                _spriteBatch.DrawString(_uiFont, title, titlePos, Color.PaleGoldenrod, 0f, origin, 2.5f, SpriteEffects.None, 0f);
            }
        }
    }
}