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

            if (InputEngine.IsActionPressed("Confirm") || InputEngine.IsMouseLeftClick())
            {
                // Transition to the actual fight!
                _game.StateManager.ChangeState(new BossBattleState(_game, _onComplete));
                InputEngine.ClearState();
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if (_game.UiFont != null)
            {
                Vector2 size = _game.UiFont.MeasureString(_text);
                Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
                spriteBatch.DrawString(_game.UiFont, _text, center - (size / 2), Color.White);
            }

            spriteBatch.End();
        }
    }
}