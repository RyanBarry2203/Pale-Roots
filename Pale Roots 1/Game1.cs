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

        // Button Styling
        Color _btnNormal = new Color(20, 20, 30, 220);
        Color _btnHover = new Color(40, 60, 100, 240); 
        Color _borderNormal = new Color(60, 60, 80);   
        Color _borderHover = Color.Cyan;               

        private enum GameState
        {
            Intro,
            Menu,
            Gameplay
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

            _spellIcons = new Texture2D[6];
            _spellIcons[0] = Content.Load<Texture2D>("Effects/SmiteIcon");
            _spellIcons[1] = Content.Load<Texture2D>("Effects/HolyNovaIcon");
            _spellIcons[2] = Content.Load<Texture2D>("Effects/HeavensFuryIcon");
            _spellIcons[3] = Content.Load<Texture2D>("Effects/HolyShieldIcon");
            _spellIcons[4] = Content.Load<Texture2D>("Effects/ElectricityIcon");
            _spellIcons[5] = Content.Load<Texture2D>("Effects/SwordJusticeIcon");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameEngine = new ChaseAndFireEngine(this);

            _cutsceneManager = new CutsceneManager(this);
            Texture2D[] slides = new Texture2D[9];
            for (int i = 1; i < 9; i++)
            {
                slides[i] = Content.Load<Texture2D>("cutscene_image_" + i);
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
                        MediaPlayer.Volume = 1.0f;
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
                            IsMouseVisible = true;
                        }
                        else if (_quitBtnRect.Contains(mousePoint))
                        {
                            Exit();
                        }
                    }
                    if (_hasStarted && InputEngine.IsKeyPressed(Keys.Escape))
                    {
                        _currentState = GameState.Gameplay;
                    }
                    break;

                case GameState.Gameplay:
                    if (InputEngine.IsKeyPressed(Keys.Escape))
                    {
                        _currentState = GameState.Menu;
                    }
                    if (_gameEngine != null)
                    {
                        _gameEngine.Update(gameTime);
                    }
                    break;
            }
            base.Update(gameTime);
        }

        private void UpdateAudio(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ================================================================
            // 1. FADING LOGIC (The "Engine" of the audio)
            // ================================================================

            // Smoothly move current volume towards target
            if (_currentVolume < _targetVolume)
            {
                _currentVolume += _fadeSpeed * dt;
                if (_currentVolume > _targetVolume) _currentVolume = _targetVolume;
            }
            else if (_currentVolume > _targetVolume)
            {
                _currentVolume -= _fadeSpeed * dt;
                if (_currentVolume < _targetVolume) _currentVolume = _targetVolume;
            }

            MediaPlayer.Volume = _currentVolume;

            // If we were fading out (_targetVolume is 0) and we hit 0, swap the song!
            if (_targetVolume <= 0.01f && _currentVolume <= 0.01f && _pendingSong != null)
            {
                MediaPlayer.Play(_pendingSong);
                _currentSong = _pendingSong;
                _pendingSong = null; // Clear the queue

                // Now fade back in
                _targetVolume = 0.5f; // Set your desired max volume (0.5 is usually good for background)
            }

            // ================================================================
            // 2. STATE LOGIC (Deciding what to play)
            // ================================================================

            if (_currentState == GameState.Menu)
            {
                // If wrong song is playing, queue the Menu song
                if (_currentSong != _menuSong && _pendingSong != _menuSong)
                {
                    SwitchTrack(_menuSong);
                    MediaPlayer.IsRepeating = true;
                }
            }
            else if (_currentState == GameState.Intro)
            {
                // If wrong song is playing, queue the Intro song
                if (_currentSong != _introSong && _pendingSong != _introSong)
                {
                    SwitchTrack(_introSong);
                    MediaPlayer.IsRepeating = false;
                }
            }
            else if (_currentState == GameState.Gameplay)
            {
                // A. Handle Intro Finishing
                if (!_hasIntroFinished)
                {
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        _hasIntroFinished = true;
                    }
                }

                // B. Handle Combat Playlist
                if (_hasIntroFinished)
                {
                    // If silence, pick a new random track
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        int index = CombatSystem.RandomInt(0, _combatSongs.Count);
                        Song nextTrack = _combatSongs[index];

                        // Since the music stopped naturally, we can't fade OUT.
                        // We just start the new one at 0 volume and fade IN.
                        MediaPlayer.Play(nextTrack);
                        _currentSong = nextTrack;

                        _currentVolume = 0f;   // Start silent
                        _targetVolume = 0.5f;  // Fade to 50%
                    }
                }
            }
        }

        // Helper to trigger a smooth transition
        private void SwitchTrack(Song newSong)
        {
            _pendingSong = newSong;
            _targetVolume = 0f; // Start fading out the current song
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
                    // 1. Draw World
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _gameEngine._camera.CurrentCameraTranslation);
                    _gameEngine.Draw(gameTime, _spriteBatch);
                    _spriteBatch.End();

                    // 2. Draw HUD
                    _spriteBatch.Begin();
                    DrawHUD();
                    _spriteBatch.End();
                    break;

                case GameState.Menu:
                    // 1. Draw World OR Background
                    _spriteBatch.Begin();

                    if (_menuBackground != null)
                    {
                        // Draw custom background image stretched to fit
                        _spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                    }
                    else
                    {
                        // If no background image, draw the game world behind the menu
                        // We have to end the previous batch and start a camera-transformed one
                        _spriteBatch.End();
                        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _gameEngine._camera.CurrentCameraTranslation);
                        _gameEngine.Draw(gameTime, _spriteBatch);
                        _spriteBatch.End();
                        _spriteBatch.Begin();
                    }

                    // 2. Draw Menu Overlay
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

            // Stylish semi-transparent background plate
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding, padding, barWidth + 4, (barHeight * 2) + 15), _hudColor);

            // HEALTH BAR
            float hpPercent = (float)p.Health / p.MaxHealth;
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, barWidth, barHeight), Color.Black * 0.5f);
            _spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, (int)(barWidth * hpPercent), barHeight), _healthColor);


            // STAMINA BAR
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
            int screenH = GraphicsDevice.Viewport.Height;

            // 1. Setup Dimensions
            int iconSize = 64;
            int spacing = 30; // Increased spacing (was 10)
            int padding1 = 15; // Padding for the background box

            // Calculate total width of the 6 icons + spaces
            int totalBarWidth = (iconSize * 6) + (spacing * 5);

            // Calculate starting X to center it
            int startX = (screenW / 2) - (totalBarWidth / 2);
            int startY = screenH - iconSize - 30; // Moved up slightly

            // 2. Draw Background Bar (Semi-transparent Black)
            // We make the box slightly larger than the icons (using padding)
            Rectangle bgRect = new Rectangle(
                startX - padding1,
                startY - padding1,
                totalBarWidth + (padding1 * 2),
                iconSize + (padding1 * 2)
            );

            // Draw the background using the existing _uiPixel texture
            _spriteBatch.Draw(_uiPixel, bgRect, Color.Black * 0.6f);

            // 3. Draw Icons
            for (int i = 0; i < 6; i++)
            {
                Rectangle dest = new Rectangle(startX + (i * (iconSize + spacing)), startY, iconSize, iconSize);

                if (_spellIcons[i] != null)
                {
                    // Draw Icon
                    _spriteBatch.Draw(_spellIcons[i], dest, Color.White);

                    // Draw Key Number (Centered on the icon)
                    if (_uiFont != null)
                    {
                        string key = (i + 1).ToString();
                        Vector2 textSize = _uiFont.MeasureString(key);
                        // Position number at bottom-right of icon
                        Vector2 textPos = new Vector2(dest.Right - textSize.X, dest.Bottom - textSize.Y);

                        _spriteBatch.DrawString(_uiFont, key, textPos, Color.Gold);
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