using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    // Creates a cinematic transition into or out of the boss fight.
    // Renders a frozen snapshot of the engine while animating the player being pulled into a void.
    public class BossTransitionState : IGameState
    {
        private Game1 _game;

        // Engine instance to render but not update so the world appears frozen.
        private ChaseAndFireEngine _frozenEngine;
        private Vector2 _blackHolePos;

        // Flags that control which cinematic text and next-state logic to use.
        private bool _isEntering;
        private bool _playerWon;
        private Action<bool> _onComplete;

        private float _timer = 0f;
        private float _duration = 6f;
        private string _text;

        public BossTransitionState(Game1 game, ChaseAndFireEngine engineToFreeze, bool entering, bool won, Action<bool> onComplete)
        {
            _game = game;
            _frozenEngine = engineToFreeze;
            _isEntering = entering;
            _playerWon = won;
            _onComplete = onComplete;

            // Place the invisible gravity well above the player's position for the pull animation.
            _blackHolePos = _frozenEngine.GetPlayer().Position + new Vector2(0, -300);

            // Choose the cinematic text based on whether the game is entering the fight or exiting it.
            if (_isEntering) _text = "What is that...\nGreat power pulling me in.";
            else if (_playerWon) _text = "You draw on the power of the mighty creature.\nThe void empowers you.";
            else _text = "The void consumes what you knew.\nOnly a fragment remains.";

            // Switch the music to the dramatic track for the transition.
            _game.AudioManager.HandleMusicState(GameState.GameOver);
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += dt;

            // Get the player from the frozen engine so the transition can move the player directly.
            Player p = _frozenEngine.GetPlayer();

            // Compute and apply a gravitational pull toward the black hole point.
            Vector2 pull = PhysicsGlobals.CalculateGravitationalForce(_blackHolePos, p.Center, 8000000f, 2000f);
            p.ApplyExternalForce(pull * dt);

            // Reduce the player's scale over time to create a depth effect.
            p.Scale = MathHelper.Clamp((float)p.Scale - (dt * 0.5f), 0f, 3f);

            // After the cinematic duration completes, reset the player and move to the next state.
            if (_timer >= _duration)
            {
                // Restore the player's scale for the next gameplay state.
                p.Scale = 3f;

                if (_isEntering)
                {
                    // Enter the lore state before starting the boss fight.
                    _game.StateManager.ChangeState(new BossLoreState(_game, _onComplete));
                }
                else
                {
                    // Invoke the completion callback and return to the main gameplay state.
                    _onComplete?.Invoke(_playerWon);
                    _game.StateManager.ChangeState(new GameplayState(_game));
                    _game.AudioManager.HandleMusicState(GameState.Gameplay);
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw the frozen game world using the engine camera so the background stays static.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _frozenEngine._camera.CurrentCameraTranslation);
            _frozenEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw the cinematic UI layer without the camera transform.
            spriteBatch.Begin();

            // Compute an alpha value that fades in at the start and fades out at the end.
            float alpha = 1.0f;
            if (_timer < 1.0f) alpha = _timer;
            if (_timer > _duration - 1.0f) alpha = (_duration - _timer);

            // Create a single black pixel texture and draw it fullscreen to produce the fade effect.
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Black });
            spriteBatch.Draw(pixel, graphicsDevice.Viewport.Bounds, Color.Black * MathHelper.Clamp(_timer / 3f, 0f, 1f));

            // Draw the cinematic text centered on the screen using the UI font and the computed alpha.
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