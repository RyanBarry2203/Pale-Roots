using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    // This state handles both the initial Main Menu and the in-game Pause Menu.
    // It manages the hitboxes for the buttons and talks to the StateManager to push or pop screens based on what the player clicks.
    public class MenuState : IGameState
    {
        private Game1 _game;

        // These invisible rectangles act as the physical hitboxes for our mouse clicks.
        private Rectangle _playBtnRect;
        private Rectangle _tutorialBtnRect;
        private Rectangle _quitBtnRect;

        public MenuState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Dynamically calculate the center of the screen so our buttons stay perfectly aligned even if the window changes size.
            int centerW = _game.GraphicsDevice.Viewport.Width / 2;
            int centerH = _game.GraphicsDevice.Viewport.Height / 2;

            // Build out the specific hitbox dimensions and positions relative to the center of the screen.
            _playBtnRect = new Rectangle(centerW - 120, centerH - 60, 240, 50);
            _tutorialBtnRect = new Rectangle(centerW - 120, centerH + 10, 240, 50);
            _quitBtnRect = new Rectangle(centerW - 120, centerH + 80, 240, 50);

            // Ping the AudioManager so it knows to fade into the relaxing menu theme.
            _game.AudioManager.HandleMusicState(GameState.Menu);
        }

        public void Update(GameTime gameTime)
        {
            // Ensure the hardware mouse cursor is visible so the player isn't clicking blindly.
            _game.IsMouseVisible = true;

            // Grab the current coordinates of the mouse on the screen.
            MouseState ms = Mouse.GetState();
            Point mousePoint = new Point(ms.X, ms.Y);

            // Wait for a fresh left-click from our custom input wrapper.
            if (InputEngine.IsMouseLeftClick())
            {
                // Check if the mouse's X/Y point is currently inside the bounds of the Play button hitbox.
                if (_playBtnRect.Contains(mousePoint))
                {
                    // If the game hasn't started yet, this is the Main Menu. Push to the Intro cinematic to start a new run.
                    if (!_game.HasStarted)
                    {
                        _game.StateManager.ChangeState(new IntroState(_game));
                    }
                    // If the game has already started, this is the Pause Menu. Pop this state off the stack to resume the action.
                    else
                    {
                        _game.StateManager.PopState();
                    }
                }
                // Check if they clicked the Tutorial button.
                else if (_tutorialBtnRect.Contains(mousePoint))
                {
                    // Stack the tutorial screen on top of the menu so the player can return here when they finish reading.
                    _game.StateManager.PushState(new TutorialState(_game));
                }
                // Check if they clicked the Quit button.
                else if (_quitBtnRect.Contains(mousePoint))
                {
                    // Hard kill the entire MonoGame application.
                    _game.Exit();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // fill screen with menu background
            if (_game.MenuBackground != null)
            {
                spriteBatch.Draw(_game.MenuBackground, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
            }
            else
            {
                // If there is no specific background art, it means we are acting as the pause menu.
                // We briefly end the UI sprite batch, draw the frozen gameplay level using the camera matrix, 
                // and then restart the UI batch to draw the buttons on top.
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _game.GameEngine._camera.CurrentCameraTranslation);
                _game.GameEngine.Draw(gameTime, spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin();
            }

            // pass the invisible hitboxes down to the UIManager so it knows exactly where to render the visual buttons.
            // We pass _game.HasStarted so it knows whether to draw "Play" or "Resume".
            _game.UIManager.DrawMenu(spriteBatch, graphicsDevice, _playBtnRect, _tutorialBtnRect, _quitBtnRect, _game.HasStarted);

            spriteBatch.End();
        }
    }
}