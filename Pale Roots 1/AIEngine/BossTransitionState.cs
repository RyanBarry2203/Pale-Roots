using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    // This state creates a cinematic transition into or out of the boss fight.
    // It takes a snapshot of the active game engine and freezes it in the background 
    // while animating the player getting sucked into a void.
    public class BossTransitionState : IGameState
    {
        private Game1 _game;

        // We hold onto the engine instance that triggered this transition so we can render it, 
        // but we won't call its Update method, effectively freezing time.
        private ChaseAndFireEngine _frozenEngine;
        private Vector2 _blackHolePos;

        // Flags to figure out which cinematic text and next-state logic we should use.
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

            // Calculate a spot high above the player's current position to act as the invisible 
            // gravity well that will pull them upwards during the animation.
            _blackHolePos = _frozenEngine.GetPlayer().Position + new Vector2(0, -300);

            // Set the cinematic text based on whether we are heading into the fight or coming out of it, and if we won or lost.
            if (_isEntering) _text = "What is that...\nGreat power pulling me in.";
            else if (_playerWon) _text = "You draw on the power of the mighty creature.\nThe void empowers you.";
            else _text = "The void consumes what you knew.\nOnly a fragment remains.";

            // Switch the audio to a dramatic track while the transition plays out.
            _game.AudioManager.HandleMusicState(GameState.GameOver);
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += dt;

            // Grab the player reference directly from the frozen engine so we can manipulate them.
            // By intentionally skipping the _frozenEngine.Update() call, enemies and projectiles stop moving.
            Player p = _frozenEngine.GetPlayer();

            // Ask the PhysicsGlobals to calculate a massive gravitational force toward our invisible black hole point,
            // then apply that force to the player so they get physically dragged across the screen.
            Vector2 pull = PhysicsGlobals.CalculateGravitationalForce(_blackHolePos, p.Center, 8000000f, 2000f);
            p.ApplyExternalForce(pull * dt);

            // Steadily shrink the player's scale to create the illusion that they are falling away into the background.
            p.Scale = MathHelper.Clamp((float)p.Scale - (dt * 0.5f), 0f, 3f);

            // Once the cinematic timer finishes, clean up and move to the next actual gameplay state.
            if (_timer >= _duration)
            {
                // Reset the player's scale back to normal so they aren't tiny when they load into the next area.
                p.Scale = 3f;

                if (_isEntering)
                {
                    // If we are heading into the boss fight, push to the lore state first.
                    _game.StateManager.ChangeState(new BossLoreState(_game, _onComplete));
                }
                else
                {
                    // If the fight is over, fire the completion callback so the main game knows if we won or lost,
                    // then return the player to the standard level exploration state.
                    _onComplete?.Invoke(_playerWon);
                    _game.StateManager.ChangeState(new GameplayState(_game));
                    _game.AudioManager.HandleMusicState(GameState.Gameplay);
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // First, we draw the completely frozen game world using the camera from the engine.
            // This ensures the background and frozen enemies stay exactly where they were.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _frozenEngine._camera.CurrentCameraTranslation);
            _frozenEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Next, we draw our cinematic UI layer directly to the screen without the camera matrix.
            spriteBatch.Begin();

            // Calculate a simple alpha curve that fades in over the first second, 
            // stays solid, and fades out during the last second.
            float alpha = 1.0f;
            if (_timer < 1.0f) alpha = _timer;
            if (_timer > _duration - 1.0f) alpha = (_duration - _timer);

            // Generate a 1x1 black pixel and stretch it over the entire screen, 
            // slowly increasing its opacity to create a fade-to-black effect over the first 3 seconds.
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Black });
            spriteBatch.Draw(pixel, graphicsDevice.Viewport.Bounds, Color.Black * MathHelper.Clamp(_timer / 3f, 0f, 1f));

            // Measure our lore text and draw it dead center on the screen, applying our fade-in/out alpha calculation.
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