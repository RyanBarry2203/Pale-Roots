using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    public class BossLoreState : IGameState
    {
        private Game1 _game;
        private Action<bool> _onComplete;
        private string _text;

        // Fade out variables
        private bool _isFadingOut = false;
        private float _fadeTimer = 0f;
        private const float FADE_DURATION = 1.5f;

        public BossLoreState(Game1 game, Action<bool> onComplete)
        {
            _game = game;
            _onComplete = onComplete;

            _text = "Gravity has pulled you into a split realm of spacetime\n" +
                    "where certain beings thrive on manipulating the fabrics of physics.\n\n" +
                    "You have access to all your physics alternating abilities here,\n" +
                    "but they won't work the same on a being as powerful as this.\n\n\n" +
                    "Press SPACE or CLICK to face your destiny...";
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            _game.IsMouseVisible = true;

            if (!_isFadingOut)
            {
                if (InputEngine.IsActionPressed("Confirm") || InputEngine.IsMouseLeftClick())
                {
                    _isFadingOut = true;
                    InputEngine.ClearState();
                }
            }
            else
            {
                _fadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_fadeTimer >= FADE_DURATION)
                {
                    _game.StateManager.ChangeState(new BossBattleState(_game, _onComplete));
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if (_game.UiFont != null)
            {
                // Calculate transparency (1.0 is fully visible, 0.0 is invisible)
                float alpha = 1.0f;
                if (_isFadingOut)
                {
                    alpha = 1.0f - (_fadeTimer / FADE_DURATION);
                }

                Vector2 size = _game.UiFont.MeasureString(_text);
                Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

                // Multiply Color.White by the alpha to fade it out smoothly
                spriteBatch.DrawString(_game.UiFont, _text, center - (size / 2), Color.White * alpha);
            }

            spriteBatch.End();
        }
    }
}