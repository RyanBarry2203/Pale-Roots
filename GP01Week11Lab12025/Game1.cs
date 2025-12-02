using CameraNS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Tiler;
using Tracker.WebAPIClient;

namespace GP01Week11Lab12025
{
    /* For Step 15 of the labsheet we have two levels so we need to make adjustments to the code
        1. We need to create a second tile map for level 2
        2. To move between the two levels we need create a reference to the current tile map to 
        manage the switch between the two levels.
        3. All tileMap references from the original code need to be changed to currentTileMap references
        4. We need an exit sign tile to move between the first and second level
        5.  we need to adjust the collider class to record the tile type 
            so we can identify when the player hits an exit sign
        6.  we need to add an exit sign tile to our tile textures list
        7.  we need to add exit sign tiles to our tile map
        8.  we need to adjust the update method to check for collision with an exit sign
            and if so switch the tile map to the second level tile map
        9.  we need to reset the player position to the start of the second level.
        10. we need to reset the colliders to match the new tile map
        11. In order to check the collision we need to change the behaviour of the player collision
        to return a bool to see if it has collided with the exit sign collider
        It helps if we can have a method to set up level 1 and level 2 colliders and control the transition 
        from level to level using a state based enum variable to manage the levels.
    */
    public enum TileType { Dirt, Grass, Ground, ExitSign, Road, Rock, Wood };

    public enum Levels { Level1, Level2 };
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        int tileWidth = 64;
        int tileHeight = 64;

        Levels currentLevel = Levels.Level1;

        List<Texture2D> tileTextures = new List<Texture2D>();
        List<Collider> colliders = new List<Collider>();
        TilePlayer player;

        SpriteFont _nameID;
        SoundEffect backgroundMusic;
        SoundEffect nextLevel;
        bool musicPlaying = true;

        Camera cam;

        int[,] tileMap = new int[,]
   {
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,2,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},

   };
        int[,] tileMap2 = new int[,]
           {
        {2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,1,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2},
        {2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},

           };
        int[,] currentTileMap;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //graphics.PreferredBackBufferWidth = tileWidth * tileMap.GetLength(1);
            //graphics.PreferredBackBufferHeight = tileHeight * tileMap.GetLength(0);

            //the viewport was too big and while the camera worked it was pointless because the viewport was massive and i wanted the viewport to be normal size and then follow the player so the 
            // camera functionality could be seen
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            graphics.ApplyChanges();

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 11 Lab 1", Task: "Week 11 Lab 1 Adding Sound");
            IsMouseVisible = true;
            //backgroundMusic.Play();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // assumes dirt is 0
            Texture2D dirt = Content.Load<Texture2D>("se_free_dirt_texture");
            tileTextures.Add(dirt);
            //Texture2D dirtTx = tileTextures[(int)TileType.Dirt];

            Texture2D grass = Content.Load<Texture2D>("se_free_grass_texture");
            tileTextures.Add(grass);

            Texture2D ground = Content.Load<Texture2D>("se_free_ground_texture");
            tileTextures.Add(ground);
            // Exit sign is at position 3
            tileTextures.Add(Content.Load<Texture2D>("exit"));

            _nameID = Content.Load<SpriteFont>("nameID");
            backgroundMusic = Content.Load<SoundEffect>("backgroundMusic");
            nextLevel = Content.Load<SoundEffect>("nextLevel");

            Vector2 mapDimensions = new Vector2(
                tileMap.GetLength(1) * tileWidth,
                tileMap.GetLength(0) * tileHeight);

            cam = new Camera(Vector2.Zero, mapDimensions);
            cam.Zoom = 1.5f;

            level1(); // level 1 replaces the previous setup code
            //SetColliders(TileType.Ground);
            //SetColliders(TileType.Dirt);
            //SetColliders(TileType.ExitSign);
            //player = new TilePlayer(Content.Load<Texture2D>(@"Tiles/player"),
            //    new Vector2(tileWidth*6, tileHeight*3));

            //currentTileMap = tileMap;
            // TODO: use this.Content to load your game content here
        }

        public void level1()
        {
            currentTileMap = tileMap;
            SetColliders(TileType.Ground);
            SetColliders(TileType.Dirt);
            SetColliders(TileType.ExitSign);
            player = new TilePlayer(Content.Load<Texture2D>("player"),
                new Vector2(tileWidth * 6, tileHeight * 3));

            // TODO: use this.Content to load your game content here
        }

        public void level2()
        {
            currentTileMap = tileMap2;
            colliders.Clear();
            SetColliders(TileType.Ground);
            SetColliders(TileType.Dirt);
            SetColliders(TileType.ExitSign);
            
            player = new TilePlayer(Content.Load<Texture2D>("player"),
                new Vector2(tileWidth * 6, tileHeight * 3));
        }
        // We have changed the reference to currentTileMap to allow for level switching
        public void SetColliders(TileType t)
        {
            for (int x = 0; x < currentTileMap.GetLength(1); x++)
                for (int y = 0; y < currentTileMap.GetLength(0); y++)
                {
                    if (currentTileMap[y, x] == (int)t)
                    {
                        colliders.Add(new Collider(
                            Content.Load<Texture2D>("collider"),
                            x, y, t // Add TileType parameter for easier collider tile identification
                            ));
                    }

                }
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            // Switching between state levels allows for better management of multiple levels
            // so logically we use a switch statement to manage the levels
            switch (currentLevel)
            {
                case Levels.Level1:
                    player.update(gameTime);
                    foreach (var item in colliders)
                        if (player.Collision(item))
                        {
                            if (item.CollisionType == TileType.ExitSign)
                            {
                                // Switch to level 2 and reset
                                currentLevel = Levels.Level2;
                                nextLevel.Play();
                                level2();
                                break;
                            }
                            // This is for efficiency to stop checking once a collision is found
                            break; 
                        }
                    break;
                case Levels.Level2:
                    player.update(gameTime);
                    foreach (var item in colliders)
                        if (player.Collision(item))
                        {
                            if (item.CollisionType == TileType.ExitSign)
                            {
                                currentLevel = Levels.Level1;
                                //level1();
                                nextLevel.Play();
                                Exit();
                            }
                            break; // This is for efficiency to stop checking once a collision is found
                        }
                    break;

                default:
                    break;
            }
            if (player.Position.X < 0) player.Position = new Vector2(0, player.Position.Y);
            if (player.Position.Y < 0) player.Position = new Vector2(player.Position.X, 0);

            float mapWidth = currentTileMap.GetLength(1) * tileWidth;
            float mapHeight = currentTileMap.GetLength(0) * tileHeight;

            if (player.Position.X > mapWidth - 64) player.Position = new Vector2(mapWidth - 64, player.Position.Y);
            if (player.Position.Y > mapHeight - 64) player.Position = new Vector2(player.Position.X, mapHeight - 64);

            // TODO: Add your update logic here
            while (musicPlaying == true)
            {
                backgroundMusic.Play();
                musicPlaying = false;
            }
            cam.follow(player.Position, GraphicsDevice.Viewport);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,null, null, null, null,cam.CurrentCameraTranslation);

            // Draw the nameID in the middle top middle of the screen
            string nameID = "S00250496 Ryan Barry";
            Vector2 nameIDSize = _nameID.MeasureString(nameID);
            Vector2 nameIDPositiion = new Vector2((graphics.PreferredBackBufferWidth - nameIDSize.X) / 2, 10);

            for (int x = 0; x < currentTileMap.GetLength(1) ; x++)
                for (int y = 0; y < currentTileMap.GetLength(0); y++)
                {
                    int textureIndex = currentTileMap[y, x];
                    Texture2D texture = tileTextures[textureIndex];
                    // Draw surrounding tiles
                        spriteBatch.Draw(texture,
                            new Rectangle(x * tileWidth,
                          y * tileHeight,
                          tileWidth,
                          tileHeight),
                            Color.White);
                }
            foreach (var c in colliders)
                c.Draw(spriteBatch);
            player.Draw(spriteBatch);
            spriteBatch.DrawString(_nameID, nameID, nameIDPositiion, Color.Yellow);
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
