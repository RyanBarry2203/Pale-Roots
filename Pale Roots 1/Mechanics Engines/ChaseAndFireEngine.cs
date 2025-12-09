using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// using Pale_Roots_1.Managers; // Uncomment if you put LevelManager in a subfolder

namespace Pale_Roots_1
{
    class ChaseAndFireEngine
    {
        // 1. REPLACE TileLayer WITH LevelManager
        public LevelManager _levelManager;

        Player p;
        //SpriteBatch spriteBatch;
        private CircularChasingEnemy[] chasers;
        public Game _gameOwnedBy;
        RotatingSprite CrossBow;
        Projectile Arrow;

        public Camera _camera;

        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            game.IsMouseVisible = true;
            //spriteBatch = new SpriteBatch(game.GraphicsDevice);

            // 2. INITIALIZE THE MANAGER
            _levelManager = new LevelManager(game);

            // 3. TELL IT TO LOAD LEVEL 1
            // This single line replaces all that messy map code we just deleted
            _levelManager.LoadLevel(0);

            // ... (Keep your existing Player/Enemy setup code here) ...

            //CrossBow = new RotatingSprite(game, game.Content.Load<Texture2D>("CrossBow"), new Vector2(100, 100), 1);
            //CrossBow.rotationSpeed = 0.01f;

            // Ensure Player starts in a valid spot (e.g. 100, 100)
            p = new Player(game, game.Content.Load<Texture2D>("wizard_strip3"), new Vector2(300, 300), 3);

            // Initialize Camera centered on player
            // Assuming map size is 10 tiles * 64 pixels = 640 width (Adjust based on your real map size)
            Vector2 mapSize = new Vector2(1920, 1920);
            _camera = new Camera(Vector2.Zero, mapSize);

            // ... (Rest of your entity setup) ...
        }

        // Change the signature to accept TileLayer
        public void Update(GameTime gameTime) // Removed "TileLayer layer" parameter, we get it from manager
        {
            Viewport gameScreen = _gameOwnedBy.GraphicsDevice.Viewport;
            Vector2 proposedPosition = p.position; // Assuming 'p' is your player variable

            // 1. Handle Input
            if (Keyboard.GetState().IsKeyDown(Keys.D)) proposedPosition += new Vector2(1, 0) * 5.0f; // Hardcoded speed for demo
            if (Keyboard.GetState().IsKeyDown(Keys.A)) proposedPosition += new Vector2(-1, 0) * 5.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.W)) proposedPosition += new Vector2(0, -1) * 5.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) proposedPosition += new Vector2(0, 1) * 5.0f;

            // 2. NEW COLLISION LOGIC using LevelManager
            // We access the CurrentLevel property you created in LevelManager
            TileLayer currentLayer = _levelManager.CurrentLevel;

            if (currentLayer != null)
            {
                // Get the specific tile the player is trying to step on
                // We use 64 because that is your tile size defined in LevelManager
                int tileX = (int)(proposedPosition.X + p.spriteWidth / 2) / 64;
                int tileY = (int)(proposedPosition.Y + p.spriteHeight / 2) / 64;

                // Check bounds to prevent crashing if player walks off map
                if (tileX >= 0 && tileX < currentLayer.Tiles.GetLength(1) &&
                    tileY >= 0 && tileY < currentLayer.Tiles.GetLength(0))
                {
                    // Only move if the tile is Passable
                    if (currentLayer.Tiles[tileY, tileX].Passable)
                    {
                        p.position = proposedPosition;
                    }
                }
            }

            // Update player animation

            _camera.follow(p.CentrePos, _gameOwnedBy.GraphicsDevice.Viewport);

            p.Update(gameTime);
        }

        // CHANGE: Add 'SpriteBatch spriteBatch' as a parameter
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Now we use the spriteBatch passed in from Game1, which is already 'Open'
            _levelManager.Draw(spriteBatch);

            p.Draw(spriteBatch);

            // If you have other items like CrossBow or projectiles, pass 'spriteBatch' to them too
            if (Arrow != null) Arrow.Draw(spriteBatch);
        }
    }
}