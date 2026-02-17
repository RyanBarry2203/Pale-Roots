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

        public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (IsFinished) return;

            CutsceneSlide slide = _slides[_currentIndex];

            // 1. CALCULATE PROGRESS (0.0 to 1.0)
            float progress = _timer / slide.Duration;

            // 2. CALCULATE ZOOM & PAN (Linear Interpolation)
            // This smoothly moves from Start values to End values based on progress

            float currentZoom = MathHelper.Lerp(slide.ZoomStart, slide.ZoomEnd, progress);
            Vector2 currentPan = Vector2.Lerp(slide.PanStart, slide.PanEnd, progress);

            // 3. CALCULATE FADE (Dip to Black)
            // First 10% of time: Fade In. Last 10% of time: Fade Out.
            float fadeDuration = slide.Duration * 0.15f;
            float alpha = 1.0f;

            if (_timer < fadeDuration)
            {
                // Fading In (0 to 1)
                alpha = _timer / fadeDuration;
            }
            else if (_timer > slide.Duration - fadeDuration)
            {
                // Fading Out (1 to 0)
                float timeLeft = slide.Duration - _timer;
                alpha = timeLeft / fadeDuration;
            }

            // 4. DRAW THE IMAGE
            // We draw the texture centered, scaled, and offset by our Pan
            Vector2 origin = new Vector2(slide.Texture.Width / 2, slide.Texture.Height / 2);
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

            spriteBatch.Draw(
                slide.Texture,
                screenCenter + currentPan, // Move the image slightly
                null,
                Color.White * alpha, // Apply the Fade
                0f,
                origin,
                currentZoom, // Apply the Zoom
                SpriteEffects.None,
                0f
            );

            // 5. DRAW TEXT
            if (_font != null)
            {
                Vector2 textSize = _font.MeasureString(slide.Text);
                Vector2 textPos = new Vector2((screenWidth / 2) - (textSize.X / 2), screenHeight - 100);

                // Text also fades with the image
                spriteBatch.DrawString(_font, slide.Text, textPos, Color.White * alpha);
            }
        }
    }
}