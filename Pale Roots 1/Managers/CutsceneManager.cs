using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This class handles the playback of narrative image sequences (like the Intro and Outro).
    // It reads a predefined list of "Slides", handling the timers, image scaling, and text rendering for each one.
    public class CutsceneManager
    {
        // A dictionary holding the full sequence of slides for any given cinematic.
        private Dictionary<string, Cutscene> _cutscenes = new Dictionary<string, Cutscene>();
        private Cutscene _currentCutscene;

        // Tracks which specific image in the sequence we are currently showing, and how long it's been on screen.
        private int _currentIndex = 0;
        private float _timer = 0f;

        // Basic drawing assets used for the text background.
        private Texture2D _pixel;
        private SpriteFont _font;

        // A flag checked by states like IntroState to know when to hand control back to the game.
        public bool IsFinished { get; private set; } = false;

        public CutsceneManager(Game game)
        {
            // Create a single white pixel to use for drawing the semi-transparent text backing box.
            _pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            try { _font = game.Content.Load<SpriteFont>("cutsceneFont"); } catch { }
        }

        public void AddCutscene(string key, Cutscene cutscene)
        {
            _cutscenes[key] = cutscene;
        }

        public void Play(string key)
        {
            // If the requested sequence exists, reset all our playback trackers to start it from the beginning.
            if (_cutscenes.ContainsKey(key))
            {
                _currentCutscene = _cutscenes[key];
                _currentIndex = 0;
                _timer = 0f;
                IsFinished = false;
            }
        }

        public void Update(GameTime gameTime)
        {
            // Safety check to ensure we aren't trying to advance timers if no cutscene is actually loaded.
            if (IsFinished || _currentCutscene == null) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _timer += dt;

            // Listen for the player wanting to skip the entire sequence.
            // We specifically use InputEngine here instead of standard Keyboard.GetState() so that if the player 
            // is holding spacebar to attack when the boss dies, they don't instantly skip the outro cinematic.
            if (InputEngine.IsKeyPressed(Keys.Space) || InputEngine.IsKeyPressed(Keys.Enter))
            {
                IsFinished = true;
            }

            // Check if the current slide has been on screen for its fully allotted duration.
            if (_timer >= _currentCutscene.Slides[_currentIndex].Duration)
            {
                // Reset the timer and move to the next slide in the sequence.
                _timer = 0;
                _currentIndex++;

                // If we just finished the final slide, flag the sequence as over.
                if (_currentIndex >= _currentCutscene.Slides.Count)
                {
                    IsFinished = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (IsFinished || _currentCutscene == null) return;

            // Grab the specific data for the image we need to draw right now.
            CutsceneSlide slide = _currentCutscene.Slides[_currentIndex];

            // Calculate exactly how far along we are in this specific slide's lifespan, returning a percentage between 0.0 and 1.0.
            float progress = _timer / slide.Duration;

            // Figure out how much we need to scale the raw image so it completely fills the player's current window size.
            float scaleX = (float)screenWidth / slide.Texture.Width;
            float scaleY = (float)screenHeight / slide.Texture.Height;

            // We take the larger of the two scales and bump it by 12% to ensure there are no black borders 
            // when we apply the zooming effect later.
            float baseScale = Math.Max(scaleX, scaleY) * 1.12f;

            // Apply a "Ken Burns" effect: smoothly interpolate the image scale between the defined start and end points based on time passed.
            float currentZoom = MathHelper.Lerp(slide.ZoomStart, slide.ZoomEnd, progress);
            float finalScale = baseScale * currentZoom;

            //Offset so the ugly gemini watermark is not visible during gameplay
            Vector2 Offset = new Vector2(-20, -20);
            Vector2 currentPan = Vector2.Lerp(slide.PanStart, slide.PanEnd, progress) + Offset;

            // fade
            // Calculate a brief window at the start and end of the slide to smoothly transition the opacity.
            float fadeDuration = slide.Duration * 0.15f;
            float alpha = 1.0f;

            // If we are in the first 15% of the slide, fade it in.
            if (_timer < fadeDuration) alpha = _timer / fadeDuration;
            // If we are in the last 15% of the slide, fade it out to black.
            else if (_timer > slide.Duration - fadeDuration)
            {
                float timeLeft = slide.Duration - _timer;
                alpha = timeLeft / fadeDuration;
            }

            // Draw the image, applying all our calculated panning, zooming, and fading math.
            Vector2 origin = new Vector2(slide.Texture.Width / 2, slide.Texture.Height / 2);
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

            spriteBatch.Draw(slide.Texture, screenCenter + currentPan, null, Color.White * alpha, 0f, origin, finalScale, SpriteEffects.None, 0f);

            // text
            if (_font != null)
            {
                // Measure the subtitle text and center it near the bottom of the screen.
                Vector2 textSize = _font.MeasureString(slide.Text);
                Vector2 textPos = new Vector2((screenWidth / 2) - (textSize.X / 2), screenHeight - 200);

                // Draw a semi-transparent black box slightly larger than the text so the white font is always readable.
                Rectangle bgRect = new Rectangle((int)textPos.X - 20, (int)textPos.Y - 10, (int)textSize.X + 40, (int)textSize.Y + 20);
                spriteBatch.Draw(_pixel, bgRect, Color.Black * 0.6f * alpha);

                // Draw the actual subtitle text.
                spriteBatch.DrawString(_font, slide.Text, textPos, Color.White * alpha);

                // Draw a small "Skip" prompt in the bottom right corner so the player knows they aren't trapped.
                string skipMsg = "Press SPACE to Skip";
                Vector2 skipSize = _font.MeasureString(skipMsg);
                Vector2 skipPos = new Vector2(screenWidth - skipSize.X - 40, screenHeight - 60);
                spriteBatch.DrawString(_font, skipMsg, skipPos, Color.Gray * alpha * 0.8f);
            }
        }
    }
}