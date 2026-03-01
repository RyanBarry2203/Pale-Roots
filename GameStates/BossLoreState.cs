using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    // Shows a short lore screen and waits for player confirmation before the boss fight.
    public class BossLoreState : IGameState
    {
        private Game1 _game;

        // Callback invoked after the lore state finishes.
        private Action<bool> _onComplete;
        private string _text;

        // Controls the fade out animation when the player chooses to continue.
        private bool _isFadingOut = false;
        private float _fadeTimer = 0f;
        private const float FADE_DURATION = 1.5f;

        private Texture2D _bgTexture;

        public BossLoreState(Game1 game, Action<bool> onComplete)
        {
            _game = game;
            _onComplete = onComplete;

            // Prepare the lore text shown to the player.
            _text = "Gravity has pulled you into a split realm of spacetime\n" +
                    "where certain beings thrive on manipulating the fabrics of physics.\n\n" +
                    "You have access to all your physics alternating abilities here,\n" +
                    "but they won't work the same on a being as powerful as this.\n\n\n" +
                    "Press SPACE or CLICK to face your destiny...";
        }

        public void LoadContent() 
        {
            try
            {
                // Try to load the background image used by this cutscene.
                _bgTexture = _game.Content.Load<Texture2D>("Boss_Cutscene");
            }
            catch
            {
                // Ignore missing background and continue.
            }
        }

        public void Update(GameTime gameTime)
        {
            // Ensure the mouse cursor is visible for click input.
            _game.IsMouseVisible = true;

            // If not already fading, listen for the confirm action to proceed.
            if (!_isFadingOut)
            {
                if (InputEngine.IsActionPressed("Confirm") || InputEngine.IsMouseLeftClick())
                {
                    _isFadingOut = true;

                    // Clear input state so the next gameplay frame does not receive the same input.
                    InputEngine.ClearState();
                }
            }
            // When the player confirmed, advance the fade timer and transition when complete.
            else
            {
                _fadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // After the fade duration, change into the boss battle state.
                if (_fadeTimer >= FADE_DURATION)
                {
                    _game.StateManager.ChangeState(new BossBattleState(_game, _onComplete));
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Clear the screen and begin drawing UI.
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw the background image stretched to the viewport if it is available.
            if (_bgTexture != null)
            {
                spriteBatch.Draw(_bgTexture, graphicsDevice.Viewport.Bounds, Color.White);
            }

            // Dim the background so the lore text is easier to read.
            spriteBatch.Draw(_game.UiPixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.8f);

            if (_game.UiFont != null)
            {
                // Compute current alpha based on whether we are fading out.
                float alpha = 1.0f;
                if (_isFadingOut)
                {
                    alpha = 1.0f - (_fadeTimer / FADE_DURATION);
                }

                // Measure the text and draw it centered on the screen with the computed alpha.
                Vector2 size = _game.UiFont.MeasureString(_text);
                Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

                spriteBatch.DrawString(_game.UiFont, _text, center - (size / 2), Color.White * alpha);
            }

            spriteBatch.End();
        }
    }
}