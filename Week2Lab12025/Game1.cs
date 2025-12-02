 using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using SharpDX.Direct2D1;
//using System.Threading.Tasks;
using Tracker.WebAPIClient;


namespace Week2Lab12025
{
    
    public class Game1 : Game
    {

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _txBackground;
        private Texture2D _txCircle;
        private Texture2D _txBox;
        private Texture2D _txDownArrow;
        private Texture2D _txRightArrow;

        private Vector2 _centreOrigin;
        SpriteEffects _rightArrowEffect = SpriteEffects.None;
        float _rotation = 0f;
        KeyboardState previousKeyState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 2 Lab 1", Task: "working with depth");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _txBackground = Content.Load<Texture2D>("Yellow Box");
            _txCircle = Content.Load<Texture2D>("see through circle");
            _txBox = Content.Load<Texture2D>("Magenta Box");
            _txDownArrow = Content.Load<Texture2D>("Down Arrow");
            _txRightArrow = Content.Load<Texture2D>("Right Arrow");

            _centreOrigin = _txRightArrow.Bounds.Center.ToVector2();



            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
              //  Exit();

            // TODO: Add your update logic here

            KeyboardState CurrentKeyState = Keyboard.GetState();

            if (previousKeyState.IsKeyDown(Keys.F) && CurrentKeyState.IsKeyUp(Keys.F)
                && _rightArrowEffect == SpriteEffects.None)
                _rightArrowEffect = SpriteEffects.FlipHorizontally;
            else if (previousKeyState.IsKeyDown(Keys.F) && CurrentKeyState.IsKeyUp(Keys.F)
                && _rightArrowEffect == SpriteEffects.FlipHorizontally)
                _rightArrowEffect = SpriteEffects.None;

            if (previousKeyState.IsKeyDown(Keys.G) && CurrentKeyState.IsKeyUp(Keys.G)
                && _rightArrowEffect == SpriteEffects.None)
                _rightArrowEffect = SpriteEffects.FlipVertically;
            else if (previousKeyState.IsKeyDown(Keys.G) && CurrentKeyState.IsKeyUp(Keys.G)
                && _rightArrowEffect == SpriteEffects.FlipVertically)
                _rightArrowEffect = SpriteEffects.None;

            if (previousKeyState.IsKeyDown(Keys.Q))
                _rotation -= .01f;
            if (previousKeyState.IsKeyDown(Keys.E))
                _rotation += .01f;

            previousKeyState = CurrentKeyState;
                base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Color AlphaColor = new Color(Color.White, 127);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            _spriteBatch.Draw(_txBackground, GraphicsDevice.Viewport.Bounds, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_txBox, new Vector2(150, 150), null, AlphaColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1f);
            _spriteBatch.Draw(_txCircle, new Vector2(100, 100), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            //_spriteBatch.End();

            _spriteBatch.Draw(_txRightArrow, GraphicsDevice.Viewport.Bounds.Center.ToVector2(),
                null, Color.White,
                _rotation, _centreOrigin, 1f, _rightArrowEffect, 0.5f);


            // TODO: Add your drawing code here
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
