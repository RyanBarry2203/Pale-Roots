using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tracker.WebAPIClient;

namespace GP012526Week4Lab2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _txCharacter;
        private Vector2 _characterPos = Vector2.Zero;
        private Texture2D _txBackGround;
        private Texture2D _txDot;
        private Viewport originalViewPort;
        private Viewport mapViewport;

        SpriteFont font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry",
                activityName: "GP01 2025 Week 4 Lab 2", Task: "S00250496 Week 4 Lab 2 finished");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _txBackGround = Content.Load<Texture2D>("main");
            _txCharacter = Content.Load<Texture2D>("bg");
            _txDot = Content.Load<Texture2D>("blueDot");

            font = Content.Load<SpriteFont>("NameID");

            originalViewPort = GraphicsDevice.Viewport;
            originalViewPort.Bounds = _txBackGround.Bounds;
            _graphics.PreferredBackBufferWidth = _txBackGround.Width;
            _graphics.PreferredBackBufferHeight = _txBackGround.Height;
            _graphics.ApplyChanges();

            mapViewport.Bounds = new Rectangle(0, 0, originalViewPort.Bounds.Width / 10,
       originalViewPort.Bounds.Height / 10);
            mapViewport.X = 0;
            mapViewport.Y = 0;

            //create position variable so that font is at the top middole of screen
            Vector2 fontOrigin = font.MeasureString("S00250496 - Ryan Barry") / 2;
            Vector2 fontPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, 0);
            Vector2 fontScale = new Vector2(1.0f, 1.0f);
            

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Left))
                _characterPos.X -= 2;
            if (keyState.IsKeyDown(Keys.Right))
                _characterPos.X += 2;
            if (keyState.IsKeyDown(Keys.Up))
                _characterPos.Y -= 2;
            if (keyState.IsKeyDown(Keys.Down))
                _characterPos.Y += 2;



            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Viewport = originalViewPort;

            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "S00250496 - Ryan Barry", new Vector2(GraphicsDevice.Viewport.Width / 2, 10), Color.White,
                0, font.MeasureString("S00250496 - Ryan Barry") / 2, 1.0f, SpriteEffects.None, 0.5f);

            _spriteBatch.Draw(_txBackGround, Vector2.Zero, Color.White);
            _spriteBatch.Draw(_txCharacter, _characterPos, Color.White);
            _spriteBatch.End();

            GraphicsDevice.Viewport = mapViewport;
            _spriteBatch.Begin();
            _spriteBatch.Draw(_txBackGround, mapViewport.Bounds, Color.White);
            _spriteBatch.Draw(_txDot, _characterPos / 10, null,
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);

            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
