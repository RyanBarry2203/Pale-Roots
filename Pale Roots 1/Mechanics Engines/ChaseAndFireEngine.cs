using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pale_Roots_1
{
    class ChaseAndFireEngine
    {
        public LevelManager _levelManager;
        public Camera _camera;
        public Game _gameOwnedBy;

        Player p;
        List<Sprite> _allies = new List<Sprite>();
        List<ChargingBattleEnemy> _enemies = new List<ChargingBattleEnemy>();

        bool _battleStarted = false;
        Vector2 _mapSize = new Vector2(1920, 1920);

        // CHANGE 1: Move Enemy Spawn to the vertical CENTER (900 instead of 300)
        Vector2 _enemySpawnOrigin = new Vector2(1600, 900);

        public ChaseAndFireEngine(Game game)
        {
            _gameOwnedBy = game;
            game.IsMouseVisible = true;

            // 1. Setup Map
            _levelManager = new LevelManager(game);
            _levelManager.LoadLevel(0);

            // 2. Setup Player
            // CHANGE 2: Spawn Player at Y=900 (Center of Map)
            p = new Player(game, game.Content.Load<Texture2D>("wizard_strip3"), new Vector2(300, 900), 3);

            // 3. Setup Camera
            _camera = new Camera(Vector2.Zero, _mapSize);

            // 4. Initialize Armies
            InitializeArmies();

            // 5. Force Camera to Center
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;
            float targetZoom = (float)vp.Width / _mapSize.X;
            _camera.Zoom = targetZoom;
            _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);
        }

        private void InitializeArmies()
        {
            Texture2D allyTx = _gameOwnedBy.Content.Load<Texture2D>("wizard_strip3");
            Texture2D enemyTx = _gameOwnedBy.Content.Load<Texture2D>("Dragon_strip3");

            // --- ALLIES (Line Formation) ---
            for (int i = 0; i < 5; i++)
            {
                // CHANGE 3: Spawn Allies around Y=900
                // Starting at 780 + increments centers them nicely around the player
                Vector2 pos = new Vector2(150, 780 + (i * 60));
                _allies.Add(new Sprite(_gameOwnedBy, allyTx, pos, 3));
            }

            // --- ENEMIES (Triangle Formation) ---
            int countOfEnemies = 10;

            int currentRow = 0;
            int enemiesInCurrentRow = 1;
            int currentSlotInRow = 0;

            float spacingX = 80f;
            float spacingY = 80f;

            for (int i = 0; i < countOfEnemies; i++)
            {
                // Triangle Logic
                float xPos = _enemySpawnOrigin.X + (currentRow * spacingX);

                // This logic automatically centers the row on _enemySpawnOrigin.Y (which is now 900)
                float rowHeight = (enemiesInCurrentRow - 1) * spacingY;
                float rowStart = _enemySpawnOrigin.Y - (rowHeight / 2f);
                float yPos = rowStart + (currentSlotInRow * spacingY);

                _enemies.Add(new ChargingBattleEnemy(_gameOwnedBy, enemyTx, new Vector2(xPos, yPos), 3));

                currentSlotInRow++;
                if (currentSlotInRow >= enemiesInCurrentRow)
                {
                    currentRow++;
                    enemiesInCurrentRow++;
                    currentSlotInRow = 0;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            Viewport vp = _gameOwnedBy.GraphicsDevice.Viewport;

            if (!_battleStarted)
            {
                // STATE 1: CINEMATIC VIEW
                float targetZoom = (float)vp.Width / _mapSize.X;
                _camera.Zoom = targetZoom;
                _camera.LookAt(new Vector2(_mapSize.X / 2, _mapSize.Y / 2), vp);

                p.Update(gameTime, _levelManager.CurrentLevel);

                if (Keyboard.GetState().IsKeyDown(Keys.D))
                    _battleStarted = true;
            }
            else
            {
                // STATE 2: CHARGE
                _camera.Zoom = MathHelper.Lerp(_camera.Zoom, 1.0f, 0.05f);

                p.Update(gameTime, _levelManager.CurrentLevel);

                foreach (var ally in _allies) { ally.position.X += 3.0f; ally.Update(gameTime); }
                foreach (var enemy in _enemies) { enemy.position.X -= 3.0f; enemy.Update(gameTime); }

                _camera.follow(p.CentrePos, vp);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _levelManager.Draw(spriteBatch);
            p.Draw(spriteBatch);
            foreach (var ally in _allies) ally.Draw(spriteBatch);
            foreach (var enemy in _enemies) enemy.Draw(spriteBatch);
        }
    }
}