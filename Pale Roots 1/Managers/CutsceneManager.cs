using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class CutsceneManager
    {
        private Dictionary<string, Cutscene> _cutscenes = new Dictionary<string, Cutscene>();
        private Cutscene _currentCutscene;

        private int _currentIndex = 0;
        private float _timer = 0f;
        private Texture2D _pixel;
        private SpriteFont _font;

        public bool IsFinished { get; private set; } = false;

        public CutsceneManager(Game game)
        {
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
            if (IsFinished || _currentCutscene == null) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _timer += dt;

            // Using InputEngine so a held spacebar doesn't instantly skip everything
            if (InputEngine.IsKeyPressed(Keys.Space) || InputEngine.IsKeyPressed(Keys.Enter))
            {
                IsFinished = true;
            }

            if (_timer >= _currentCutscene.Slides[_currentIndex].Duration)
            {
                _timer = 0;
                _currentIndex++;

                if (_currentIndex >= _currentCutscene.Slides.Count)
                {
                    IsFinished = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (IsFinished || _currentCutscene == null) return;

            CutsceneSlide slide = _currentCutscene.Slides[_currentIndex];

            float progress = _timer / slide.Duration;
            float scaleX = (float)screenWidth / slide.Texture.Width;
            float scaleY = (float)screenHeight / slide.Texture.Height;

            float baseScale = Math.Max(scaleX, scaleY) * 1.12f;

            float currentZoom = MathHelper.Lerp(slide.ZoomStart, slide.ZoomEnd, progress);
            float finalScale = baseScale * currentZoom;


            Vector2 watermarkOffset = new Vector2(-20, -20);
            Vector2 currentPan = Vector2.Lerp(slide.PanStart, slide.PanEnd, progress) + watermarkOffset;

            float fadeDuration = slide.Duration * 0.15f;
            float alpha = 1.0f;

            if (_timer < fadeDuration) alpha = _timer / fadeDuration;
            else if (_timer > slide.Duration - fadeDuration)
            {
                float timeLeft = slide.Duration - _timer;
                alpha = timeLeft / fadeDuration;
            }

            Vector2 origin = new Vector2(slide.Texture.Width / 2, slide.Texture.Height / 2);
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

            spriteBatch.Draw(slide.Texture, screenCenter + currentPan, null, Color.White * alpha, 0f, origin, finalScale, SpriteEffects.None, 0f);

            if (_font != null)
            {
                Vector2 textSize = _font.MeasureString(slide.Text);
                Vector2 textPos = new Vector2((screenWidth / 2) - (textSize.X / 2), screenHeight - 200);

                Rectangle bgRect = new Rectangle((int)textPos.X - 20, (int)textPos.Y - 10, (int)textSize.X + 40, (int)textSize.Y + 20);
                spriteBatch.Draw(_pixel, bgRect, Color.Black * 0.6f * alpha);
                spriteBatch.DrawString(_font, slide.Text, textPos, Color.White * alpha);

                string skipMsg = "Press SPACE to Skip";
                Vector2 skipSize = _font.MeasureString(skipMsg);
                Vector2 skipPos = new Vector2(screenWidth - skipSize.X - 40, screenHeight - 60);
                spriteBatch.DrawString(_font, skipMsg, skipPos, Color.Gray * alpha * 0.8f);
            }
        }
    }
}