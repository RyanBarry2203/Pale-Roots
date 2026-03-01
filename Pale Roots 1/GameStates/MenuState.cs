using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // Handles the main menu and the pause menu UI.
    // Uses hitboxes and the StateManager to change screens when the player clicks.
    public class MenuState : IGameState
    {
        private Game1 _game;

        // Invisible rectangles used as mouse click hitboxes, in full screen they are slightly off...
        private Rectangle _playBtnRect;
        private Rectangle _tutorialBtnRect;
        private Rectangle _quitBtnRect;

        public MenuState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Calculate screen center so buttons remain centered.
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            int centerH = _game.GraphicsDevice.Viewport.Height / 2;

            // Create button hitboxes around the center point.
            _playBtnRect = new Rectangle(centerW - 120, centerH - 60, 240, 50);
            _tutorialBtnRect = new Rectangle(centerW - 120, centerH + 10, 240, 50);
            _quitBtnRect = new Rectangle(centerW - 120, centerH + 80, 240, 50);

            // Tell the audio manager to play the menu music.
            _game.AudioManager.HandleMusicState(GameState.Menu);
        }

        public void Update(GameTime gameTime)
        {
            // Make the mouse cursor visible.
            _game.IsMouseVisible = true;

            // Get the current mouse position.
            MouseState ms = Mouse.GetState();
            Point mousePoint = new Point(ms.X, ms.Y);

            // Check for a fresh left mouse click.
            if (InputEngine.IsMouseLeftClick())
            {
                // If the click is inside the play button hitbox.
                if (_playBtnRect.Contains(mousePoint))
                {
                    // If the game has not started, go to the intro cutscene.
                    if (!_game.HasStarted)
                    {
                        _game.StateManager.ChangeState(new IntroState(_game));
                    }
                    // If the game is running, pop this menu to resume gameplay.
                    else
                    {
                        _game.StateManager.PopState();
                    }
                }
                // If the click is inside the tutorial button hitbox.
                else if (_tutorialBtnRect.Contains(mousePoint))
                {
                    // Push the tutorial state so the player can read it and return.
                    _game.StateManager.PushState(new TutorialState(_game));
                }
                // If the click is inside the quit button hitbox.
                else if (_quitBtnRect.Contains(mousePoint))
                {
                    // Exit the application.
                    _game.Exit();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // Draw the menu background if available.
            if (_game.MenuBackground != null)
            {
                spriteBatch.Draw(_game.MenuBackground, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
            }
            else
            {
                // If no background is set, render the paused game behind the menu using the engine camera.
                // End the UI batch, draw the frozen game world, and then restart the UI batch to draw the buttons on top.
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
                _game.GameEngine.Draw(gameTime, spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin();
            }

            // Pass the button hitboxes to UIManager so it can draw the visual buttons.
            // Provide HasStarted so UIManager knows to label the primary button as Play or Resume.
            _game.UIManager.DrawMenu(spriteBatch, graphicsDevice, _playBtnRect, _tutorialBtnRect, _quitBtnRect, _game.HasStarted);

            spriteBatch.End();
        }
    }
}