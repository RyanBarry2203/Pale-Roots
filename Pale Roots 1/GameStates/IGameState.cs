using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Interface for screens and modes used by the StateManager.
    // Implementations load resources, update logic each frame, and draw to the screen.
    public interface IGameState
    {
        // Load any textures, sounds, or data needed by this state.
        void LoadContent();

        // Update the state's logic using the provided game time.
        void Update(GameTime gameTime);

        // Render the state using the provided sprite batch and graphics device.
        void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);
    }
}