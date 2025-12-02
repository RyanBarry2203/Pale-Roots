using Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using SharpDX.Direct2D1;
using Tracker.WebAPIClient;

namespace GP012025Week6Lab1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SimpleCam cam;
        private Texture2D background;
        private Texture2D character;
        private float speed = 5f;
        private Vector2 CharacterPosition;
        private SpriteFont font;

        SpriteFont nameID;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 6 Lab 1", Task: "Drawing and testing scene");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>("bigback3000x3000");
            character = Content.Load<Texture2D>("right arrow");
            cam = new SimpleCam(GraphicsDevice.Viewport);
            font = Content.Load<SpriteFont>("debug");
            CharacterPosition = GraphicsDevice.Viewport.Bounds.Center.ToVector2();


            nameID = Content.Load<SpriteFont>("NameID");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var kstate = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            //Trying to figure out how to transfer the code from last lab into this
            //one and understand how all the class methods are interacting with eatchoter

            // Same movement code from last lab for character
            if (kstate.IsKeyDown(Keys.A))
                CharacterPosition.X -= speed;
            if (kstate.IsKeyDown(Keys.D))
                CharacterPosition.X += speed;
            if (kstate.IsKeyDown(Keys.W))
                CharacterPosition.Y -= speed;
            if (kstate.IsKeyDown(Keys.S))
                CharacterPosition.Y += speed;


            // Cam Movement
            if (kstate.IsKeyDown(Keys.A))
                cam.Move(new Vector2(-speed, 0));
            if (kstate.IsKeyDown(Keys.S))
                cam.Move(new Vector2(speed, 0));
            if (kstate.IsKeyDown(Keys.W))
                cam.Move(new Vector2(0, -speed));
            if (kstate.IsKeyDown(Keys.D))
                cam.Move(new Vector2(0, speed));

            // Cam Rotation
            if (kstate.IsKeyDown(Keys.X))
                cam.Rotate(0.05f);
            if (kstate.IsKeyDown(Keys.Z))
                cam.Rotate(-0.05f); 

            // Cam Zoom
            if (kstate.IsKeyDown(Keys.Space))
                cam.Zoom(0.01f); 
            if (kstate.IsKeyDown(Keys.Enter))
                cam.Zoom(-0.01f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            string nameAndID = "Ryan Barry S00250496";

            GraphicsDevice.Clear(Color.CornflowerBlue);

            Vector2 textSize = nameID.MeasureString(nameAndID);
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2),
                GraphicsDevice.Viewport.Height - textSize.Y
            );

            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend,
        null, null, null, null, cam.get_transformation(GraphicsDevice));
            _spriteBatch.DrawString(font, "Character X: " + CharacterPosition.X.ToString() + " Y: "
                + CharacterPosition.Y.ToString(), cam.pos + new Vector2(0, 30), Color.White);


            //_spriteBatch.Begin(transformMatrix: cam.get_transformation(GraphicsDevice));
            _spriteBatch.DrawString(nameID, nameAndID, position, Color.White);  
            //_spriteBatch.Draw(background, Vector2.Zero, Color.White);
            //_spriteBatch.Draw(character, CharacterPosition, Color.White);
            //_spriteBatch.End();

            _spriteBatch.Draw(background, Vector2.Zero, Color.White);
            _spriteBatch.Draw(character, CharacterPosition, Color.White);
            // TODO: Add your drawing code here
            _spriteBatch.End();

            _spriteBatch.Begin();
            //Draw the camera in a different spritebatch at constant position in viewport
            _spriteBatch.DrawString(font, "Camera Position X: " + cam.pos.X.ToString() + " Y: "
                + cam.pos.Y.ToString(), Vector2.Zero, Color.White);
            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
