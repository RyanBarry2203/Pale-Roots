using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tracker.WebAPIClient;
using Engines;
using Sprites;

namespace GP01Week8Lab2025
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ChaseEngine _chaseEngine;  
        //private Healthbar healthBar;
        SpriteFont nameID;
        Texture2D BackgroudTx;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 8 Lab", Task: "Implementing Collectibles");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            nameID = Content.Load<SpriteFont>("NameID");
            BackgroudTx = Content.Load<Texture2D>("Images/background");
            _chaseEngine = new ChaseEngine(this);

            //healthBar = new Healthbar(
             //   new Vector2(20, 20), // position on screen
             //   100,                 // starting health
             //   this                 // reference to Game
           // );



            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //KeyboardState k = Keyboard.GetState();

            //if (k.IsKeyDown(Keys.Right))
            //{
            //    healthBar.health++;   // increase
            //}

            //if (k.IsKeyDown(Keys.Left))
            //{
             //   if (healthBar.health > 0)
             //       healthBar.health--;  // decrease
            //}
            // TODO: Add your update logic here
            _chaseEngine.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            string nameAndID = "Ryan Barry S00250496";
            Vector2 textSize = nameID.MeasureString(nameAndID);
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2),
                GraphicsDevice.Viewport.Height - textSize.Y
            );

            _spriteBatch.Begin();
            //healthBar.draw(_spriteBatch);
            _spriteBatch.Draw(BackgroudTx, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            _spriteBatch.DrawString(nameID, nameAndID, position, Color.White);
            _spriteBatch.End();

            _chaseEngine.Draw(gameTime);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
