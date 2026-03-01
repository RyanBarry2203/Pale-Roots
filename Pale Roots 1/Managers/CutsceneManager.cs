using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Manages playback of image based cutscenes.
    public class CutsceneManager
    {
        // Holds named cutscene sequences.
        private Dictionary<string, Cutscene> _cutscenes = new Dictionary<string, Cutscene>();
        private Cutscene _currentCutscene;

        // Index of the current slide and time spent on it.
        private int _currentIndex = 0;
        private float _timer = 0f;

        // Simple assets used for text background and font.
        private Texture2D _pixel;
        private SpriteFont _font;

        // Becomes true when the active cutscene has finished.
        public bool IsFinished { get; private set; } = false;

        public CutsceneManager(Game game)
        {
            // Create a single white pixel for drawing backgrounds behind text.
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
            // Start the requested cutscene from the beginning if it exists.
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
            // Do nothing if there is no cutscene or it is already finished.
            if (IsFinished || _currentCutscene == null) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _timer += dt;

            // Allow the player to skip the cutscene with space or enter.
            if (InputEngine.IsKeyPressed(Keys.Space) || InputEngine.IsKeyPressed(Keys.Enter))
            {
                IsFinished = true;
            }

            // Advance to the next slide when the current slide's duration expires.
            if (_timer >= _currentCutscene.Slides[_currentIndex].Duration)
            {
                _timer = 0;
                _currentIndex++;

                // Mark the sequence finished when we pass the last slide.
                if (_currentIndex >= _currentCutscene.Slides.Count)
                {
                    IsFinished = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (IsFinished || _currentCutscene == null) return;

            // Retrieve the current slide to draw.
            CutsceneSlide slide = _currentCutscene.Slides[_currentIndex];

            // Compute progress through the slide as a value from 0 to 1.
            float progress = _timer / slide.Duration;

            // Compute scaling to cover the screen.
            float scaleX = (float)screenWidth / slide.Texture.Width;
            float scaleY = (float)screenHeight / slide.Texture.Height;

            // Use the larger scale and add a small margin to avoid black borders.
            float baseScale = Math.Max(scaleX, scaleY) * 1.12f;

            // Interpolate the zoom value for a gentle pan and zoom effect.
            float currentZoom = MathHelper.Lerp(slide.ZoomStart, slide.ZoomEnd, progress);
            float finalScale = baseScale * currentZoom;

            // Apply a small offset and interpolate the pan position.
            Vector2 Offset = new Vector2(-20, -20);
            Vector2 currentPan = Vector2.Lerp(slide.PanStart, slide.PanEnd, progress) + Offset;

            // Calculate a fade in at the start and a fade out at the end of the slide.
            float fadeDuration = slide.Duration * 0.15f;
            float alpha = 1.0f;

            if (_timer < fadeDuration) alpha = _timer / fadeDuration;
            else if (_timer > slide.Duration - fadeDuration)
            {
                float timeLeft = slide.Duration - _timer;
                alpha = timeLeft / fadeDuration;
            }

            // Draw the slide texture with the computed pan, zoom, and alpha.
            Vector2 origin = new Vector2(slide.Texture.Width / 2, slide.Texture.Height / 2);
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

            spriteBatch.Draw(slide.Texture, screenCenter + currentPan, null, Color.White * alpha, 0f, origin, finalScale, SpriteEffects.None, 0f);

            // Draw subtitle text and a skip hint if a font is available.
            if (_font != null)
            {
                Vector2 textSize = _font.MeasureString(slide.Text);
                Vector2 textPos = new Vector2((screenWidth / 2) - (textSize.X / 2), screenHeight - 200);

                // Draw a semi transparent box behind the text for readability.
                Rectangle bgRect = new Rectangle((int)textPos.X - 20, (int)textPos.Y - 10, (int)textSize.X + 40, (int)textSize.Y + 20);
                spriteBatch.Draw(_pixel, bgRect, Color.Black * 0.6f * alpha);

                // Draw the slide subtitle.
                spriteBatch.DrawString(_font, slide.Text, textPos, Color.White * alpha);

                // Draw a small skip instruction in the bottom right corner.
                string skipMsg = "Press SPACE to Skip";
                Vector2 skipSize = _font.MeasureString(skipMsg);
                Vector2 skipPos = new Vector2(screenWidth - skipSize.X - 40, screenHeight - 60);
                spriteBatch.DrawString(_font, skipMsg, skipPos, Color.Gray * alpha * 0.8f);
            }
        }
    }
}