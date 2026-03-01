using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    // This state acts as a brief narrative pause before the chaos of the boss fight begins.
    // It gives the player some context, drops a hint about the gameplay mechanics, 
    // and waits for them to explicitly confirm they are ready to proceed.
    public class BossLoreState : IGameState
    {
        private Game1 _game;

        // pass along the completion callback so we can hand it off to the BossBattleState later.
        private Action<bool> _onComplete;
        private string _text;

        // Variables to handle the smooth visual transition out of this screen.
        private bool _isFadingOut = false;
        private float _fadeTimer = 0f;
        private const float FADE_DURATION = 1.5f;

        private Texture2D _bgTexture;

        public BossLoreState(Game1 game, Action<bool> onComplete)
        {
            _game = game;
            _onComplete = onComplete;

            // Set up the dramatic lore and hints that will be displayed in the center of the screen.
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
                _bgTexture = _game.Content.Load<Texture2D>("Boss_Cutscene");
            }
            catch
            {
                // Failsafe
            }
        }

        public void Update(GameTime gameTime)
        {
            // Make sure the player can actually see their mouse if they want to click to continue.
            _game.IsMouseVisible = true;

            // If we are just sitting on the screen reading...
            if (!_isFadingOut)
            {
                // Listen to the InputEngine for the player's go-ahead.
                if (InputEngine.IsActionPressed("Confirm") || InputEngine.IsMouseLeftClick())
                {
                    _isFadingOut = true;

                    // We explicitly clear the input state here so the player doesn't accidentally 
                    // trigger an attack or jump the exact frame the boss battle loads.
                    InputEngine.ClearState();
                }
            }
            // If the player hit continue, start ticking up the fade timer.
            else
            {
                _fadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Once the text has completely faded out, swap this state out for the actual BossBattleState.
                if (_fadeTimer >= FADE_DURATION)
                {
                    _game.StateManager.ChangeState(new BossBattleState(_game, _onComplete));
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Wipe the screen to pure black as a base layer.
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            //Draw background image stretched to fit the screen
            if (_bgTexture != null)
            {
                spriteBatch.Draw(_bgTexture, graphicsDevice.Viewport.Bounds, Color.White);
            }


            spriteBatch.Draw(_game.UiPixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.8f);

            if (_game.UiFont != null)
            {
                // start with full opacity.
                float alpha = 1.0f;

                // If the player clicked continue, we calculate how far along the fade timer is 
                // and reduce the alpha down toward 0.0 (invisible).
                if (_isFadingOut)
                {
                    alpha = 1.0f - (_fadeTimer / FADE_DURATION);
                }

                // Find the exact center of the screen, measure the text block, and draw it perfectly centered 
                // while applying our calculated alpha transparency.
                Vector2 size = _game.UiFont.MeasureString(_text);
                Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

                spriteBatch.DrawString(_game.UiFont, _text, center - (size / 2), Color.White * alpha);
            }

            spriteBatch.End();
        }
    }
}