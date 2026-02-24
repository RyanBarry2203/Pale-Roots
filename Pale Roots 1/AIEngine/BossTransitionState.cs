using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    public class BossTransitionState : IGameState
    {
        private Game1 _game;
        private bool _isEntering;
        private bool _playerWon;
        private Action<bool> _onComplete; // Callback to run after transition

        private float _timer = 0f;
        private float _duration = 5f;
        private string _text;

        // Constructor for ENTERING the boss
        public BossTransitionState(Game1 game, Action<bool> onComplete)
        {
            _game = game;
            _isEntering = true;
            _onComplete = onComplete;
            _text = "I feel something's off...\nThe pull of gravity is getting stronger.";
        }

        // Constructor for LEAVING the boss
        public BossTransitionState(Game1 game, bool entering, bool won, Action<bool> onComplete)
        {
            _game = game;
            _isEntering = false;
            _playerWon = won;
            _onComplete = onComplete;

            if (won) _text = "You draw on the power of the mighty creature.\nThe void empowers you.";
            else _text = "The void consumes what you knew.\nOnly a fragment remains.";
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer >= _duration)
            {
                if (_isEntering)
                {
                    // Done with intro text -> Go to Arena
                    _game.StateManager.ChangeState(new BossBattleState(_game, _onComplete));
                }
                else
                {
                    // Done with outro text -> Go back to Main Game
                    _onComplete?.Invoke(_playerWon);

                    // Resume the main game engine
                    _game.StateManager.ChangeState(new GameplayState(_game));

                    // Resume music
                    _game.AudioManager.HandleMusicState(GameState.Gameplay);
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();
            graphicsDevice.Clear(Color.Black);

            float alpha = 1.0f;
            if (_timer < 1.0f) alpha = _timer;
            if (_timer > _duration - 1.0f) alpha = (_duration - _timer);

            if (_game.UiFont != null)
            {
                Vector2 size = _game.UiFont.MeasureString(_text);
                Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
                spriteBatch.DrawString(_game.UiFont, _text, center - (size / 2), Color.Red * alpha);
            }
            spriteBatch.End();
        }
    }
}