using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sprites;
using Tracker.WebAPIClient;


namespace Gp012025Week9Lab1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        //private Sprite Player;
        //private Sprite Enemy;

        private Player player;
        private Enemy enemy;

        Texture2D BackgroundTx;
        SpriteFont nameID;



        //new Sprite Player;
        //new Sprite Enemy;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "Gp01 2025 Week9 Lab 1", Task: "Creating Circular chasing Enemy");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            BackgroundTx = Content.Load<Texture2D>("background");

            nameID = Content.Load<SpriteFont>("NameID");

            // Player using the Player class
            Texture2D[] playerTextures = new Texture2D[]
            {
        Content.Load<Texture2D>("left"),
        Content.Load<Texture2D>("right"),
        Content.Load<Texture2D>("up"),
        Content.Load<Texture2D>("down"),
        Content.Load<Texture2D>("stand")
            };

            SoundEffect[] playerSounds = new SoundEffect[]
            {
        Content.Load<SoundEffect>("Audio/PlayerDirection/0"),
        Content.Load<SoundEffect>("Audio/PlayerDirection/1"),
        Content.Load<SoundEffect>("Audio/PlayerDirection/2"),
        Content.Load<SoundEffect>("Audio/PlayerDirection/3"),
        Content.Load<SoundEffect>("Audio/PlayerDirection/4")
            };

            player = new Player(
                this,
                playerTextures,
                playerSounds,
                new Vector2(100, 100),
                8,
                0,
                3.5f
            );

            //Enemey using the Enemy class
            enemy = new Enemy(
                this,
                Content.Load<Texture2D>("sonicsheet"),
                new Vector2(400, 100),
                8
            );
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            player.Update(gameTime);
            enemy.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string nameAndID = "Ryan Barry S00250496";
            Vector2 textSize = nameID.MeasureString(nameAndID);
            Vector2 position = new Vector2(20,20);  

            _spriteBatch.Begin();
            _spriteBatch.Draw(BackgroundTx, new Rectangle(0, 0, 800, 480), Color.White);
            _spriteBatch.DrawString(nameID, nameAndID, position, Color.Yellow);
            player.Draw(_spriteBatch);
            enemy.Draw(_spriteBatch);
            _spriteBatch.End();

            //Player.Draw(_spriteBatch);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
