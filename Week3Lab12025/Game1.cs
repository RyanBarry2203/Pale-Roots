using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using SharpDX.Direct2D1;
using Tracker.WebAPIClient;

namespace Week3Lab12025
{
    //static void string timeMessage;
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont font;
        string Message = "Message to Fade";
        byte alpha = 255;

        private string timeMessage;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 3 Lab 1", Task: "Fading text");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("FadeFont");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            int seconds = gameTime.TotalGameTime.Seconds;
            if (alpha > 0)
            {
                alpha -= (byte)seconds;
            }
            timeMessage = "Time Elapsed in seconds" +seconds.ToString();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            
            Vector2 textSize = font.MeasureString(timeMessage);
            Vector2 screenCenter = new Vector2(
                _graphics.PreferredBackBufferWidth / 2f,
                _graphics.PreferredBackBufferHeight / 2f
            );

            Vector2 position = screenCenter - (textSize / 2f);
            _spriteBatch.DrawString(font, timeMessage, position, Color.White);
            
            Color messageColor = new Color((byte)255, (byte)255, (byte)255, alpha);
            _spriteBatch.DrawString(font, Message, new Vector2(100, 100), messageColor);
            
            if (alpha == 0)
                alpha = 255;

            _spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}
