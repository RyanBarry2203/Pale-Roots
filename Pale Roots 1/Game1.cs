using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

        // Assets
        private SoundEffect warTheme;
        private Song introMusic;
        private Texture2D _uiPixel;
        private SpriteFont _uiFont;

        // NEW: Background Texture for the menu
        private Texture2D _menuBackground;

        private ChaseAndFireEngine _gameEngine;
        private CutsceneManager _cutsceneManager;

        // Menu Buttons
        private Rectangle _playBtnRect;
        private Rectangle _quitBtnRect;

        private Song _menuSong;
        private List<Song> _combatSongs = new List<Song>();
        private Song _currentSong;
        private bool _isCombatMusicPlaying = false;

        private Song _victorySong;
        private Song _outroSong;
        private List<Texture2D> _outroImages = new List<Texture2D>(); // Store textures to load into manager later

        // Input Safety
        private float _uiInputDelay = 0f;
        private const float SAFETY_DELAY = 0.5f; // 0.5 seconds before you can click


        private int _nextLevelThreshold = 3; 
        private int _levelStep = 4;

        private UpgradeManager _upgradeManager;
        private List<UpgradeManager.UpgradeOption> _currentUpgradeOptions;

        // Progression Logic
        private const int WIN_CONDITION_KILLS = 180;

        // Spell Icons
        private Texture2D[] _spellIcons;
        private Keys[] _spellKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6 };

        // UI Colors (Medieval Sci-Fantasy Theme)
        Color _hudColor = new Color(10, 10, 15, 200);
        Color _healthColor = new Color(180, 20, 20); 
        Color _staminaColor = new Color(50, 205, 50);

        private float _currentVolume = 1.0f;
        private float _targetVolume = 1.0f;
        private float _fadeSpeed = 0.5f; 
        private Song _pendingSong = null;

        private Song _introSong;
        private bool _hasIntroFinished = false;
        private Song _deathSong;

        // Button Styling
        Color _btnNormal = new Color(20, 20, 30, 220);
        Color _btnHover = new Color(40, 60, 100, 240); 
        Color _borderNormal = new Color(60, 60, 80);   
        Color _borderHover = Color.Cyan;

        private enum GameState
        {
            Intro,
            Menu,
            Gameplay,
            LevelUp,
            Victory,
            Outro,
            GameOver
        }

        private GameState _currentState = GameState.Menu;
        private bool _hasStarted = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            new InputEngine(this);
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

            _menuSong = Content.Load<Song>("PaleRootsMenu"); 

            _combatSongs.Add(Content.Load<Song>("Guitar"));
            _combatSongs.Add(Content.Load<Song>("MoreGuitar"));
            _combatSongs.Add(Content.Load<Song>("Groovy"));
            _combatSongs.Add(Content.Load<Song>("ihavenoidea"));
            _combatSongs.Add(Content.Load<Song>("uhm"));
            _introSong = Content.Load<Song>("Whimsy");
            _outroSong = Content.Load<Song>("Whimsy");
            _deathSong = Content.Load<Song>("Sad");
            _victorySong = Content.Load<Song>("uhm");

            _spellIcons = new Texture2D[6];
            _spellIcons[0] = Content.Load<Texture2D>("Effects/SmiteIcon");
            _spellIcons[1] = Content.Load<Texture2D>("Effects/HolyNovaIcon");
            _spellIcons[2] = Content.Load<Texture2D>("Effects/HeavensFuryIcon");
            _spellIcons[3] = Content.Load<Texture2D>("Effects/HolyShieldIcon");
            _spellIcons[4] = Content.Load<Texture2D>("Effects/ElectricityIcon");
            _spellIcons[5] = Content.Load<Texture2D>("Effects/SwordJusticeIcon");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameEngine = new ChaseAndFireEngine(this);

            _upgradeManager = new UpgradeManager(
                _gameEngine.GetPlayer(),
                _gameEngine.GetSpellManager(),
                _spellIcons,
                GraphicsDevice
            );

            _cutsceneManager = new CutsceneManager(this);
            Texture2D[] slides = new Texture2D[9];
            for (int i = 1; i < 9; i++)
            {
                slides[i] = Content.Load<Texture2D>("cutscene_image_" + i);
            }

            for (int i = 1; i < 9; i++)
            {
                _outroImages.Add(Content.Load<Texture2D>("cutscene_image_" + i));
            }

            //Song introMusic = Content.Load<Song>("Whimsy");
            //MediaPlayer.Play(introMusic);
            //MediaPlayer.IsRepeating = false;
            //MediaPlayer.Volume = 0.2f;

            _uiPixel = new Texture2D(GraphicsDevice, 1, 1);
            _uiPixel.SetData(new[] { Color.White });

            try { _uiFont = Content.Load<SpriteFont>("cutsceneFont"); }
            catch { }

            // NEW: Try loading a menu background, fallback to null if missing
            try { _menuBackground = Content.Load<Texture2D>("menu_background"); }
            catch { }

            // Cutscene Setup
            float dur = 5500f;
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[1], "Decades ago Scholars discovered...", dur + 2000, 1.0f, 1.05f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[2], "One of these beings known as Atun...", dur, 1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[3], "But soon after humanity discovered...", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[4], "Insulted, Atun withdrew any power...", dur, 1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[5], "The Roots of his power went Pale...", dur, 1.0f, 1.08f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[6], "In news of a weakened civilisation...", dur, 1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[7], "Led by Nivellin, a war Hero...", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(slides[8], "War was set in Motion...", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));
        }

        protected override void Update(GameTime gameTime)
        {
            UpdateAudio(gameTime);

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

                // NEW: Outro Logic
                case GameState.Outro:
                    _cutsceneManager.Update(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) || _cutsceneManager.IsFinished)
                    {
                        // Return to Menu after Outro
                        _currentState = GameState.Menu;
                        _hasStarted = false;
                        Initialize(); // Reset game
                        LoadContent();
                    }
                    break;

                case GameState.Menu:
                    // ... existing menu logic ...
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
        private void HandleLevelUpInput()
        {
            // We don't need to check ButtonState.Pressed here because IsMouseLeftClick handles the "Click" event (Press -> Release)
            if (InputEngine.IsMouseLeftClick())
            {
                MouseState ms = Mouse.GetState();

                // Recalculate layout exactly as drawn
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
                        // Apply Upgrade
                        _currentUpgradeOptions[i].ApplyAction.Invoke();

                        // Resume Game
                        _currentState = GameState.Gameplay;

                        // Clear state to prevent clicking through to the game immediately
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

            // RECALCULATE RECTANGLES EXACTLY AS DRAWN
            Rectangle playAgainRect = new Rectangle(centerW - 100, centerH, 200, 50);
            Rectangle quitRect = new Rectangle(centerW - 100, centerH + 70, 200, 50);

            // Check for click
            if (ms.LeftButton == ButtonState.Pressed && InputEngine.IsMouseLeftClick())
            {
                if (playAgainRect.Contains(ms.Position))
                {
                    // --- RESET PROGRESSION VARIABLES ---
                    _nextLevelThreshold = 5; // Reset to starting difficulty
                    _levelStep = 5;          // Reset scaling

                    // Reset Game Engine
                    Initialize();
                    LoadContent();

                    _currentState = GameState.Gameplay;
                    _hasStarted = true;

                    // Reset Audio Volume
                    _targetVolume = 0.5f;

                    InputEngine.ClearState();
                }
                else if (quitRect.Contains(ms.Position))
                {
                    Exit();
                }
            }
        }

        private void UpdateAudio(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. FADING LOGIC
            if (_currentVolume < _targetVolume)
            {
                _currentVolume += _fadeSpeed * dt;
                if (_currentVolume > _targetVolume) _currentVolume = _targetVolume;
            }

            MediaPlayer.Volume = _currentVolume;

            // 2. STATE MACHINE
            switch (_currentState)
            {
                case GameState.Menu:
                    EnsureLoopingTrack(_menuSong);
                    break;

                case GameState.Intro:
                case GameState.Outro: // Use Intro music for Outro too, or _victorySong
                    EnsureLoopingTrack(_introSong);
                    break;

                case GameState.Gameplay:
                case GameState.LevelUp: // Keep gameplay music during level up
                    // If we are playing a "State" song (Menu/Death/Victory), STOP IT immediately
                    if (_currentSong == _menuSong || _currentSong == _deathSong || _currentSong == _victorySong || _currentSong == _introSong)
                    {
                        MediaPlayer.Stop();
                        _currentSong = null;
                    }

                    // If nothing is playing, pick a random combat track
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        PlayRandomCombatTrack();
                    }

                    // Ensure volume is up (in case we came from a fade)
                    _targetVolume = 0.5f;
                    break;

                case GameState.GameOver:
                    EnsureLoopingTrack(_deathSong);
                    break;

                case GameState.Victory:
                    EnsureLoopingTrack(_victorySong);
                    break;
            }
        }

        private void StartOutroSequence()
        {
            _cutsceneManager.ClearSlides();

            float dur = 5000f;
            // Add your lore slides here. I'm reusing images but changing text.
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroImages[0], "The Skeleton King has fallen...", dur, 1.0f, 1.05f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroImages[1], "The Pale Roots begin to wither...", dur, 1.05f, 1.0f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroImages[2], "Humanity has reclaimed its future.", dur, 1.0f, 1.1f, Vector2.Zero, Vector2.Zero));
            _cutsceneManager.AddSlide(new CutsceneManager.CutsceneSlide(_outroImages[3], "The War is over.", dur + 2000, 1.0f, 1.0f, Vector2.Zero, Vector2.Zero));

            _currentState = GameState.Outro;
        }

        // Helper to ensure a specific track is looping
        private void EnsureLoopingTrack(Song song)
        {
            if (_currentSong != song)
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(song);
                MediaPlayer.IsRepeating = true;
                _currentSong = song;
                _targetVolume = 0.5f;
                _currentVolume = 0.5f; // Snap volume immediately to avoid silence
            }
        }

        private void PlayRandomCombatTrack()
        {
            int index = CombatSystem.RandomInt(0, _combatSongs.Count);
            Song nextTrack = _combatSongs[index];
            MediaPlayer.Play(nextTrack);
            MediaPlayer.IsRepeating = false; // Don't loop combat tracks, let them end so we pick a new one
            _currentSong = nextTrack;
            _targetVolume = 0.5f;
        }

        // Helper to trigger a smooth transition
        private void SwitchTrack(Song newSong)
        {
            _pendingSong = newSong;
            _targetVolume = 0f; // Start fading out the current song
        }

        private void DrawLevelUpScreen()
        {
            // Darken background
            _spriteBatch.Draw(_uiPixel, GraphicsDevice.Viewport.Bounds, Color.Black * 0.7f);

            // Title
            string title = "ALTERNATE PHYSICS WITH THE STRENGTH OF THE DEAD";
            Vector2 size = _uiFont.MeasureString(title);
            _spriteBatch.DrawString(_uiFont, title, new Vector2(GraphicsDevice.Viewport.Width / 2 - size.X / 2, 100), Color.Gold);

            // Draw Cards
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
            _spriteBatch.Draw(_uiPixel, GraphicsDevice.Viewport.Bounds, Color.Black * 0.85f); // Darker background

            string title = victory ? "VICTORY" : "YOU DIED";
            Color color = victory ? Color.Gold : Color.Red;

            Vector2 size = _uiFont.MeasureString(title);
            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            _spriteBatch.DrawString(_uiFont, title, new Vector2(center.X - size.X, center.Y - 150), color, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

            // Lore Text for Victory
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

                // Border
                int b = 2;
                Color bc = hover ? Color.White : Color.Black;
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Y, r.Width, b), bc);
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Bottom - b, r.Width, b), bc);
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Y, b, r.Height), bc);
                _spriteBatch.Draw(_uiPixel, new Rectangle(r.Right - b, r.Y, b, r.Height), bc);

                Vector2 ts = _uiFont.MeasureString(t);
                _spriteBatch.DrawString(_uiFont, t, new Vector2(r.Center.X - ts.X / 2, r.Center.Y - ts.Y / 2), Color.White);
            }

            // Change Button Text based on state
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

                case GameState.LevelUp:
                case GameState.Victory:   // ADDED HERE
                case GameState.GameOver:  // ADDED HERE
                    // 1. Draw World (Frozen)
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _gameEngine._camera.CurrentCameraTranslation);
                    _gameEngine.Draw(gameTime, _spriteBatch);
                    _spriteBatch.End();

                    // 2. Draw UI Overlay
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

            // --- HEALTH & STAMINA (Existing Code) ---
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding, padding, barWidth + 4, (barHeight * 2) + 15), _hudColor);

            // Health
            float hpPercent = (float)p.Health / p.MaxHealth;
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, barWidth, barHeight), Color.Black * 0.5f);
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, (int)(barWidth * hpPercent), barHeight), _healthColor);

            // Stamina
            float dashRatio = 0f;
            if (p.DashDuration > 0) dashRatio = 1.0f - (p.DashTimer / p.DashDuration);
            else dashRatio = 1.0f;
            dashRatio = MathHelper.Clamp(dashRatio, 0f, 1f);

            int stamY = padding + barHeight + 8;
            int stamH = barHeight / 2;
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, barWidth, stamH), Color.Black * 0.5f);
            Color currentStaminaColor = (dashRatio >= 0.99f) ? _staminaColor : Color.Orange;
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, (int)(barWidth * dashRatio), stamH), currentStaminaColor);

            // --- WAR DOMINANCE BAR ---
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

            int unlockedCount = 0;
            for (int i = 0; i < 6; i++)
            {
                if (_gameEngine.GetSpellManager().IsSpellUnlocked(i)) unlockedCount++;
            }

            if (unlockedCount > 0)
            {
                int totalWidth = (unlockedCount * iconSize) + ((unlockedCount - 1) * spacing);
                int startX = (screenW / 2) - (totalWidth / 2);
                int startY = GraphicsDevice.Viewport.Height - iconSize - 20;
                int currentX = startX;

                // Draw Background for the bar
                Rectangle bgRect = new Rectangle(startX - 10, startY - 10, totalWidth + 20, iconSize + 20);
                _spriteBatch.Draw(_uiPixel, bgRect, Color.Black * 0.6f);

                for (int i = 0; i < 6; i++)
                {
                    if (_gameEngine.GetSpellManager().IsSpellUnlocked(i))
                    {
                        Rectangle dest = new Rectangle(currentX, startY, iconSize, iconSize);

                        // Draw Icon
                        if (_spellIcons[i] != null)
                            _spriteBatch.Draw(_spellIcons[i], dest, Color.White);

                        // Draw Key Number (Top Left of icon to avoid overlap)
                        string key = (i + 1).ToString();
                        _spriteBatch.DrawString(_uiFont, key, new Vector2(dest.X + 2, dest.Y + 2), Color.Gold);

                        currentX += iconSize + spacing;
                    }
                }
            }
        }

        private void DrawMenu()
        {
            int screenW = GraphicsDevice.Viewport.Width;
            int screenH = GraphicsDevice.Viewport.Height;

            // Dark Overlay to make text pop (darker if we have a background image)
            _spriteBatch.Draw(_uiPixel, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.6f);

            Point mousePoint = new Point(Mouse.GetState().X, Mouse.GetState().Y);

            // --- HELPER TO DRAW BUTTONS ---
            void DrawFancyButton(Rectangle rect, string text, bool isHovered)
            {
                Color fillColor = isHovered ? _btnHover : _btnNormal;
                Color borderColor = isHovered ? _borderHover : _borderNormal;

                // 1. Fill
                _spriteBatch.Draw(_uiPixel, rect, fillColor);

                // 2. Borders (Top, Bottom, Left, Right)
                int borderThickness = 2;
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Bottom - borderThickness, rect.Width, borderThickness), borderColor);
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), borderColor);
                _spriteBatch.Draw(_uiPixel, new Rectangle(rect.Right - borderThickness, rect.Y, borderThickness, rect.Height), borderColor);

                // 3. Text
                if (_uiFont != null)
                {
                    Vector2 size = _uiFont.MeasureString(text);
                    Vector2 pos = new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2);
                    _spriteBatch.DrawString(_uiFont, text, pos, Color.White);
                }
            }

            // Draw Play/Resume
            bool playHover = _playBtnRect.Contains(mousePoint);
            DrawFancyButton(_playBtnRect, _hasStarted ? "RESUME" : "PLAY", playHover);

            // Draw Quit
            bool quitHover = _quitBtnRect.Contains(mousePoint);
            DrawFancyButton(_quitBtnRect, "QUIT", quitHover);

            // --- DRAW TITLE ---
            if (_uiFont != null)
            {
                string title = "PALE ROOTS";
                Vector2 titleSize = _uiFont.MeasureString(title);
                Vector2 titlePos = new Vector2(screenW / 2f, 200);
                Vector2 origin = titleSize / 2f;

                // 1. Shadow (Offset by 3 pixels)
                _spriteBatch.DrawString(_uiFont, title, titlePos + new Vector2(3, 3), Color.Black * 0.8f, 0f, origin, 2.5f, SpriteEffects.None, 0f);

                // 2. Main Text (Cyan/White gradient look)
                _spriteBatch.DrawString(_uiFont, title, titlePos, Color.PaleGoldenrod, 0f, origin, 2.5f, SpriteEffects.None, 0f);
            }
        }
    }
}