using GP01Week11_Lab2_2025;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Tracker.WebAPIClient;

namespace GP01Week11_Lab2_2025
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 
    public enum TileTypes {STEEL_WALL_TILE,STEEL_FLOOR_TILE,BLUE_STEEL_WALL_TILE,EAGLE_TILE }
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        TiledPlayer player;
        private TileLayer t_layer;
        SoundEffect  engineSound;
        SoundEffect background;
        bool backghroundPlaying = true;

        Camera _camera;

        List<TileRef> layer_tileRefs = new List<TileRef>()
        {
            // column, row, value in TileMap
            new TileRef(0,1,(int)TileTypes.STEEL_WALL_TILE),
            new TileRef(3,3,(int)TileTypes.STEEL_FLOOR_TILE),
            new TileRef(4,2,(int)TileTypes.BLUE_STEEL_WALL_TILE),
            new TileRef(0,2,(int)TileTypes.EAGLE_TILE),
        };
        // Just for future reference Not used here
        TileTypes[] ImpassableTileTypes = new TileTypes[] { TileTypes.BLUE_STEEL_WALL_TILE, TileTypes.STEEL_WALL_TILE};

        int[,] tileMap = new int[,]
            {
               {0,2,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
               {0,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,}, 
               {1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,2,1,1,1,1,1,2,1,1,1,1,2,1,1,1,2,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,},
               {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,}

    };
        public Game1()
        {
            IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            new InputEngine(this);
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", 
                activityName: " GP01 2025 Week 11 Lab 2", Task: "Adding Sound");
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Helper.SpriteSheet = Content.Load<Texture2D>("tank tiles 64 x 64");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            player = new TiledPlayer(
                new Vector2(0,0),
                new List<TileRef>()
                {
                   new TileRef(15, 1, 0),
                   new TileRef(16, 1, 0),
                   new TileRef(17, 1, 0),
                   new TileRef(18, 1, 0),
                   new TileRef(19, 1, 0),
                   new TileRef(20, 1, 0),
                   new TileRef(21, 1, 0),
                }, 
                64, 64, 1.0f);
            t_layer = new TileLayer(tileMap,layer_tileRefs, 64,64);

            Vector2 worldBounds = new Vector2(tileMap.GetLength(1) * 64, tileMap.GetLength(0) * 64);
            _camera = new Camera(Vector2.Zero, worldBounds);
            engineSound = Content.Load<SoundEffect>("engineSound");
            background = Content.Load<SoundEffect>("background");

            //player.PlayEngineSound(engineSound);

            // TODO: use this.Content to load your game content here
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
            
            player.Update(gameTime);
            // TODO: Add your update logic here
            // player could go off screen so clamp position
            float mapWidth = tileMap.GetLength(1) * 64;
            float mapHeight = tileMap.GetLength(0) * 64;

            float x = MathHelper.Clamp(player.PixelPosition.X, 0, mapWidth - player.FrameWidth);
            float y = MathHelper.Clamp(player.PixelPosition.Y, 0, mapHeight - player.FrameHeight);

            player.PixelPosition = new Vector2(x, y);

            _camera.follow(player.PixelPosition, GraphicsDevice.Viewport);

            player.PlayEngineSound(engineSound);
            if (backghroundPlaying)
                background.Play();
            backghroundPlaying = false;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend,null, null, null, null,_camera.CurrentCameraTranslation);
            t_layer.Draw(spriteBatch);
            player.Draw(spriteBatch, Helper.SpriteSheet);
            // TODO: Add your drawing code here
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
