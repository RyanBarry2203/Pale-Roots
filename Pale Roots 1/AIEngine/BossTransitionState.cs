using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    public class BossTransitionState : IGameState
    {
        private Game1 _game;
        private ChaseAndFireEngine _frozenEngine; // The engine we want to render behind the fade
        private Vector2 _blackHolePos;

        private bool _isEntering;
        private bool _playerWon;
        private Action<bool> _onComplete;
        private float _timer = 0f;
        private float _duration = 6f; // Extended slightly for cinematic pacing
        private string _text;

        public BossTransitionState(Game1 game, ChaseAndFireEngine engineToFreeze, bool entering, bool won, Action<bool> onComplete)
        {
            _game = game;
            _frozenEngine = engineToFreeze;
            _isEntering = entering;
            _playerWon = won;
            _onComplete = onComplete;

            // Spawn the "cinematic black hole" 300 pixels above where the player was standing
            _blackHolePos = _frozenEngine.GetPlayer().Position + new Vector2(0, -300);

            if (_isEntering) _text = "What is that...\nGreat power pulling me in.";
            else if (_playerWon) _text = "You draw on the power of the mighty creature.\nThe void empowers you.";
            else _text = "The void consumes what you knew.\nOnly a fragment remains.";

            _game.AudioManager.HandleMusicState(GameState.GameOver);
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += dt;

            // --- THE CINEMATIC EFFECT ---
            // Notice we DO NOT call _frozenEngine.Update(). This freezes the enemies!
            Player p = _frozenEngine.GetPlayer();

            // Apply massive physical suction to the player using your physics library
            Vector2 pull = PhysicsGlobals.CalculateGravitationalForce(_blackHolePos, p.Center, 8000000f, 2000f);
            p.ApplyExternalForce(pull * dt);

            // Give them a cool shrinking effect as they get sucked into the void
            p.Scale = MathHelper.Clamp((float)p.Scale - (dt * 0.5f), 0f, 3f);

            // --- STATE SWITCHING ---
            if (_timer >= _duration)
            {
                // Reset player scale for when they load into the next area
                p.Scale = 3f;

                if (_isEntering)
                {
                    _game.StateManager.ChangeState(new BossBattleState(_game, _onComplete));
                }
                else
                {
                    _onComplete?.Invoke(_playerWon);
                    _game.StateManager.ChangeState(new GameplayState(_game));
                    _game.AudioManager.HandleMusicState(GameState.Gameplay);
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // 1. Draw the completely frozen game world
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _frozenEngine._camera.CurrentCameraTranslation);
            _frozenEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // 2. Draw the cinematic fade to black and text over the top
            spriteBatch.Begin();
            float alpha = 1.0f;
            if (_timer < 1.0f) alpha = _timer;
            if (_timer > _duration - 1.0f) alpha = (_duration - _timer);

            // Draw pure black fade overlay
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Black });
            spriteBatch.Draw(pixel, graphicsDevice.Viewport.Bounds, Color.Black * MathHelper.Clamp(_timer / 3f, 0f, 1f));

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