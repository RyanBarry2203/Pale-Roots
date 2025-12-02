using Engines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Screens;
using Tracker.WebAPIClient;
using System.Collections.Generic;

namespace GP012324Week9Lab2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        //private  SoundEffect _opening;
        //private Texture2D _circle;
        //private SoundEffect _capture;
        private SpriteFont _nameID;
        private List<MovingCircle> _movingCircles;

        private SplashScreen _openingScreen;
        private SplashScreen _backgroundScreen;

        //create a list to hold 10 moving circles


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 9 Lab 2", Task: "Implementing Input Manager and Game Play");

            //    _openingScreen = new SplashScreen(this, Vector2.Zero,
            //Content.Load<Texture2D>("OpeningSplashScreen"),
            //Content.Load<Song>("Opening Music Track"), Keys.Space);

            new InputEngine(this);

            // create 10 moving circle instances and keep them in a list
            _movingCircles = new List<MovingCircle>();
            for (int i = 0; i < 10; i++)
            {
                var mc = new MovingCircle(this);
                _movingCircles.Add(mc);
            }

            //     _openingScreen = new SplashScreen(this, Vector2.Zero,
            //null, null, Keys.Space);

            //     _backgroundScreen = new SplashScreen(this, Vector2.Zero,
            //          null, null, Keys.F);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            //_opening = Content.Load<SoundEffect>("Opening Music Track");
            //_circle = Content.Load<Texture2D>("circle");
            //_capture = Content.Load<SoundEffect>("capture");
            Services.AddService(_spriteBatch);
            _nameID = Content.Load<SpriteFont>("nameID");

            //_openingScreen.Tx = Content.Load<Texture2D>("OpeningSplashScreen");
            //_openingScreen.BackingTrack = Content.Load<Song>("Opening Music Track");

            //_backgroundScreen.Tx = Content.Load<Texture2D>("background");
            //_backgroundScreen.BackingTrack = Content.Load<Song>("Opening Music Track");

            _openingScreen = new SplashScreen(this, Vector2.Zero,
                Content.Load<Texture2D>("OpeningSplashScreen"),
                Content.Load<Song>("Opening Music Track"), Keys.Space);
            _openingScreen.Active = true;

            _openingScreen.LoadContent();

            //while (_openingScreen.Active == false)
            //{

            // CHANGED: Set Key to Keys.Escape so the component handles the Escape key itself
            _backgroundScreen = new SplashScreen(this, Vector2.Zero,
                    Content.Load<Texture2D>("background"),
                    Content.Load<Song>("Opening Music Track"), Keys.Escape);

            // Start it as active (it will be hidden by logic in Update if opening is on)
            _backgroundScreen.Active = false;

            _backgroundScreen.LoadContent();


            //if (InputEngine.IsKeyPressed(Keys.Escape))
            //{
            //    _backgroundScreen.Active = false;
            //}


            //_openingScreen.LoadContent();
            //_backgroundScreen.LoadContent();

            base.LoadContent();

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            // TODO: Add your update logic here

            // LOGIC FIX:
            // We removed the manual Input checks here because the SplashScreen component 
            // checks input internally. Doing it in both places caused the flicker.

            // This ensures mutually exclusive screens:
            if (_openingScreen.Active == false)
            {
                // If opening is active, FORCE background to be inactive
                _backgroundScreen.Active = true;
            }
            if (_backgroundScreen.Active == false)
            {
                // If opening is active, FORCE background to be inactive
                _openingScreen.Active = true;
            }


            // If opening is inactive, allow background to follow its own state.
            // However, we want it to turn ON automatically when Opening closes.
            // But we must allow Escape (handled inside the component) to turn it off.

            // If you want it to automatically appear when Space is pressed:
            // We don't force it to true here, because that would prevent Escape from working.


            // NOTE: If you simply want the background to ALWAYS be there when opening is gone:
            // change the else block to: _backgroundScreen.Active = true; 
            // (But then Escape won't work to close it).

            // MovingCircle instances are DrawableGameComponents and were added to Game.Components
            // in their constructor, so they will be updated automatically by the framework.
            // Do not call Update on them manually here to avoid double updates.

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            string nameID = "S00250496 - Ryan Barry";

            Vector2 textSize = _nameID.MeasureString(nameID);
            Vector2 textPosition = new Vector2((GraphicsDevice.Viewport.Width - textSize.X) / 2, 10);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_nameID, nameID, textPosition, Color.Red);
            _spriteBatch.End();


           // //Replace this incorrect line in Draw():
           //      for each(var mc in _movingCircles)
           //      {
           //     mc.Draw(_spriteBatch)
           //      }

           //// With the correct C# foreach syntax and method call:
                




            //GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}