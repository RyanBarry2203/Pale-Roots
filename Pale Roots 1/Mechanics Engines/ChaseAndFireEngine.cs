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

        PlayerWithWeapon p;
        SpriteBatch spriteBatch;
        private CircularChasingEnemy[] chasers;
        private Game _gameOwnedBy;
        RotatingSprite CrossBow;
        Projectile Arrow;

        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            game.IsMouseVisible = true;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            // 2. INITIALIZE THE MANAGER
            _levelManager = new LevelManager(game);

            // 3. TELL IT TO LOAD LEVEL 1
            // This single line replaces all that messy map code we just deleted
            _levelManager.LoadLevel(1);

            // ... (Keep your existing Player/Enemy setup code here) ...

            //CrossBow = new RotatingSprite(game, game.Content.Load<Texture2D>("CrossBow"), new Vector2(100, 100), 1);
            //CrossBow.rotationSpeed = 0.01f;

            // Ensure Player starts in a valid spot (e.g. 100, 100)
            p = new PlayerWithWeapon(game, game.Content.Load<Texture2D>("wizard_strip3"), new Vector2(100, 100), 3);

            // ... (Rest of your entity setup) ...
        }

        // Change the signature to accept TileLayer
        public void Update(GameTime gameTime, TileLayer layer)
        {
            Viewport gameScreen = myGame.GraphicsDevice.Viewport;
            Vector2 proposedPosition = position;

            // 1. Handle Input (Calculate where we want to go)
            if (Keyboard.GetState().IsKeyDown(Keys.D)) proposedPosition += new Vector2(1, 0) * playerVelocity;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) proposedPosition += new Vector2(-1, 0) * playerVelocity;
            if (Keyboard.GetState().IsKeyDown(Keys.W)) proposedPosition += new Vector2(0, -1) * playerVelocity;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) proposedPosition += new Vector2(0, 1) * playerVelocity;

            // 2. Collision Detection
            if (layer != null)
            {
                // Get the center of the player
                int tileX = (int)(proposedPosition.X + spriteWidth / 2) / 64;
                int tileY = (int)(proposedPosition.Y + spriteHeight / 2) / 64;

                // Check bounds
                if (tileX >= 0 && tileX < layer.Tiles.GetLength(1) &&
                    tileY >= 0 && tileY < layer.Tiles.GetLength(0))
                {
                    // Only move if the tile is Passable
                    if (layer.Tiles[tileY, tileX].Passable)
                    {
                        this.position = proposedPosition;
                    }
                }
            }
            else
            {
                // Fallback if map isn't loaded yet
                this.position = proposedPosition;
            }

            // ... (Keep your projectile/shooting logic below here) ...
            // Note: Do NOT call base.Update(gameTime) if it overrides position logic, 
            // but usually, base.Update just handles animation, so it's fine.
            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            // 5. DELEGATE DRAWING TO THE MANAGER
            // The Engine doesn't need to know *how* to draw tiles anymore
            _levelManager.Draw(spriteBatch);

            p.Draw(spriteBatch);
            //CrossBow.Draw(spriteBatch);

            // ... (Rest of drawing code) ...
        }
    }
}