using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Pauses the game and displays upgrade options for the player to choose from.
    public class LevelUpState : IGameState
    {
        private Game1 _game;

        // Small delay to prevent accidental clicks the instant the screen appears.
        private float _inputDelay = 0.5f;

        public LevelUpState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Ask the audio manager to play the level up music.
            _game.AudioManager.HandleMusicState(GameState.LevelUp);
        }

        public void Update(GameTime gameTime)
        {
            // Make the mouse cursor visible so the player can click options.
            _game.IsMouseVisible = true;

            // Count down the safety delay before accepting input.
            if (_inputDelay > 0)
            {
                _inputDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            // On a fresh left click, check which upgrade card was selected.
            if (InputEngine.IsMouseLeftClick())
            {
                MouseState ms = Mouse.GetState();
                Rectangle screen = _game.GraphicsDevice.Viewport.Bounds;

                // Card dimensions and spacing used to build clickable rectangles.
                int cardWidth = 200;
                int cardHeight = 300;
                int spacing = 50;

                // Center the row of cards on the screen.
                int totalWidth = (_game.CurrentUpgradeOptions.Count * cardWidth) + ((_game.CurrentUpgradeOptions.Count - 1) * spacing);
                int startX = (screen.Width / 2) - (totalWidth / 2);
                int startY = (screen.Height / 2) - (cardHeight / 2);

                // Create a hitbox for each option and see if the click landed inside it.
                for (int i = 0; i < _game.CurrentUpgradeOptions.Count; i++)
                {
                    Rectangle cardRect = new Rectangle(startX + (i * (cardWidth + spacing)), startY, cardWidth, cardHeight);

                    if (cardRect.Contains(ms.Position))
                    {
                        // Execute the upgrade action and return to gameplay if still in this state.
                        _game.CurrentUpgradeOptions[i].ApplyAction.Invoke();

                        if (_game.StateManager.CurrentState == this)
                        {
                            _game.StateManager.ChangeState(new GameplayState(_game));
                        }

                        // Clear input so the click does not carry over into gameplay.
                        InputEngine.ClearState();
                        break;
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Draw the frozen game world in the background using the engine camera.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
            _game.GameEngine.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // Draw the level up UI overlay using the UI manager.
            spriteBatch.Begin();
            _game.UIManager.DrawLevelUpScreen(spriteBatch, graphicsDevice, _game.CurrentUpgradeOptions, _game.UpgradeManager);
            spriteBatch.End();
        }
    }
}