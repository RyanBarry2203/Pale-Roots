using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // This is the universal blueprint for every single screen or mode in the game.
    // By forcing GameplayState, MenuState, BossBattleState, etc., to all follow this exact contract,
    // our StateManager can load, update, and draw whatever the current state is without needing to know what it actually does.
    public interface IGameState
    {
        void LoadContent();

        void Update(GameTime gameTime);

        void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);
    }
}