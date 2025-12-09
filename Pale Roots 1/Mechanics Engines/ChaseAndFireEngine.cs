using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// using Pale_Roots_1.Managers; 

namespace Pale_Roots_1
{
    class ChaseAndFireEngine
    {
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        Player p;

        // Use lists if you want multiple enemies later, arrays are fixed size
        private CircularChasingEnemy[] chasers;
        RotatingSprite CrossBow;
        Projectile Arrow;

        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            game.IsMouseVisible = true;

            // 1. Setup Map
            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0); // Load the Big Open Plane

            // 2. Setup Player
            // Spawning at 300,300 to be safe inside the map
            p = new Player(game, game.Content.Load<Texture2D>("wizard_strip3"), new Vector2(300, 300), 3);

            // 3. Setup Camera
            // Map is 30x30 tiles (1920x1920 pixels)
            Vector2 mapSize = new Vector2(1920, 1920);
            _camera = new Camera(Vector2.Zero, mapSize);
        }

        public void Update(GameTime gameTime)
        {
            // --- CLEAN UPDATE LOOP ---

            // 1. Pass the Map Layer to the Player so HE handles collision
            p.Update(gameTime, _levelManager.CurrentLevel);

            // 2. Make Camera follow Player
            _camera.follow(p.CentrePos, _gameOwnedBy.GraphicsDevice.Viewport);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _levelManager.Draw(spriteBatch);
            p.Draw(spriteBatch);

            if (Arrow != null) Arrow.Draw(spriteBatch);
        }
    }
}