using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class CutsceneManager
    {

        public class CutsceneSlide
        {
            public Texture2D Texture;
            public string Text;
            public float Duration;

            // Movement Variables
            public float ZoomStart;
            public float ZoomEnd;
            public Vector2 PanStart;
            public Vector2 PanEnd;

            private float _fadeAlpha = 1.0f;

            public CutsceneSlide(Texture2D texture, string text, float duration, float zStart, float zEnd, Vector2 pStart, Vector2 pEnd)
            {
                Texture = texture;
                Text = text;
                Duration = duration;
                ZoomStart = zStart;
                ZoomEnd = zEnd;
                PanStart = pStart;
                PanEnd = pEnd;
            }
        }

        private List<CutsceneSlide> _slides = new List<CutsceneSlide>();
        private int _currentIndex = 0;
        private float _timer = 0f;


        private Texture2D _pixel;
        private SpriteFont _font;

        public bool IsFinished { get; private set; } = false;

        public CutsceneManager(Game game)
        {

            _pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });


            try
            {
                _font = game.Content.Load<SpriteFont>("cutsceneFont");
            }
            catch
            {
                
            }


            //_slides.Add(new CutsceneSlide("In the beginning, the roots were pale...", Color.Black, 3000f));
            //_slides.Add(new CutsceneSlide("The war consumed everything.", Color.DarkRed, 3000f));
            //_slides.Add(new CutsceneSlide("Now, only the Skeleton King remains.", Color.DarkSlateGray, 3000f));
            //_slides.Add(new CutsceneSlide("Press SPACE to skip...", Color.Black, 2000f));
        }
        public void AddSlide(CutsceneSlide slide)
        {
            _slides.Add(slide);
        }

        public void Update(GameTime gameTime)
        {
            if (IsFinished) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _timer += dt;


            if (Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                IsFinished = true;
            }


            if (_timer >= _slides[_currentIndex].Duration)
            {
                _timer = 0;
                _currentIndex++;

                if (_currentIndex >= _slides.Count)
                {
                    IsFinished = true;
                }
            }
        }

        public void ClearSlides()
        {
            _slides.Clear();
            _currentIndex = 0;
            _timer = 0f;
            IsFinished = false;
        }

        public void Reset()
        {
            _currentIndex = 0;
            _timer = 0f;
            IsFinished = false;
        }

        public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (IsFinished) return;

            CutsceneSlide slide = _slides[_currentIndex];

            // 1. CALCULATE PROGRESS
            float progress = _timer / slide.Duration;

            // 2. CALCULATE ZOOM & PAN
            float currentZoom = MathHelper.Lerp(slide.ZoomStart, slide.ZoomEnd, progress);
            Vector2 currentPan = Vector2.Lerp(slide.PanStart, slide.PanEnd, progress);

            // 3. CALCULATE FADE
            float fadeDuration = slide.Duration * 0.15f;
            float alpha = 1.0f;

            if (_timer < fadeDuration) alpha = _timer / fadeDuration;
            else if (_timer > slide.Duration - fadeDuration)
            {
                float timeLeft = slide.Duration - _timer;
                alpha = timeLeft / fadeDuration;
            }

            // 4. DRAW IMAGE
            Vector2 origin = new Vector2(slide.Texture.Width / 2, slide.Texture.Height / 2);
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

            spriteBatch.Draw(slide.Texture, screenCenter + currentPan, null, Color.White * alpha,
                0f, origin, currentZoom, SpriteEffects.None, 0f);

            // 5. DRAW TEXT WITH BACKGROUND
            if (_font != null)
            {
                // Main Story Text
                Vector2 textSize = _font.MeasureString(slide.Text);
                Vector2 textPos = new Vector2((screenWidth / 2) - (textSize.X / 2), screenHeight - 200);

                // Draw Semi-Transparent Box behind text
                Rectangle bgRect = new Rectangle(
                    (int)textPos.X - 20,
                    (int)textPos.Y - 10,
                    (int)textSize.X + 40,
                    (int)textSize.Y + 20
                );
                spriteBatch.Draw(_pixel, bgRect, Color.Black * 0.6f * alpha);
                spriteBatch.DrawString(_font, slide.Text, textPos, Color.White * alpha);

                // Skip Prompt (Bottom Right)
                string skipMsg = "Press SPACE to Skip";
                Vector2 skipSize = _font.MeasureString(skipMsg);
                Vector2 skipPos = new Vector2(screenWidth - skipSize.X - 40, screenHeight - 60);

                spriteBatch.DrawString(_font, skipMsg, skipPos, Color.Gray * alpha * 0.8f);
            }
        }
    }
}