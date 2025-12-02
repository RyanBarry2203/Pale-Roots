using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices.Marshalling;
using Tracker.WebAPIClient;

namespace Week2Lab22025
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        // Variables for the Dot
        Texture2D dot;
        Color dotColor;
        Rectangle dotRect;
        int dotSize;

        Texture2D dot2;
        Color dotColor2;
        Rectangle dotRect2;
        int dotSize2;

        // Variables for the Background 
        Texture2D background;
        Rectangle backgroundRect;

        SoundEffect collisionSound;
        bool isColliding = false;

        // Variables to hold the display properties
        int displayWidth;
        int displayHeight;

        // Variables to hold the color change
        byte redComponent = 150;
        byte blueComponent = 0;
        byte greenComponent = 0;
        byte alphaComponent = 150;

        byte redComponent2 = 0;
        byte blueComponent2 = 150;
        byte greenComponent2 = 0;
        byte alphaComponent2 = 150;

       

        // Vars to draw message
        SpriteFont font;
        string message = " ";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// </summary>
        protected override void Initialize()
        {
            displayWidth = GraphicsDevice.Viewport.Width;
            displayHeight = GraphicsDevice.Viewport.Height;

            ActivityAPIClient.Track(StudentID: "S00250496", StudentName: "Ryan Barry", activityName: "GP01 2025 Week 2 Lab 2", Task: "Collision and Sound.");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            dot = Content.Load<Texture2D>("Assets for Lab 2 2022/WhiteDot");
            dotColor = Color.White;
            dotSize = 40;

            dot2 = Content.Load<Texture2D>("Assets for Lab 2 2022/WhiteDot2");
            dotColor2 = Color.Red;
            dotSize2 = 80;

            dotRect = new Rectangle(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2, dotSize, dotSize);
            dotRect2 = new Rectangle(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2, dotSize, dotSize);

            background = Content.Load<Texture2D>("Assets for Lab 2 2022/background");
            backgroundRect = new Rectangle(0, 0, displayWidth, displayHeight);

            font = Content.Load<SpriteFont>("Assets for Lab 2 2022/MessageFont");

            collisionSound = Content.Load<SoundEffect>("Assets for Lab 2 2022/Collision");

        }

        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.B))
                blueComponent++;
            if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.V))
                greenComponent++;
            if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.C))
                redComponent++;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.X))
                alphaComponent++;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.L))
                blueComponent2++;
            if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.K))
                greenComponent2++;
            if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.J))
                redComponent2++;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.H))
                alphaComponent2++;

            GamePadState gpState = GamePad.GetState(PlayerIndex.One);
            if (gpState.IsConnected)
            {
                if (gpState.ThumbSticks.Left.X != 0 || gpState.ThumbSticks.Left.Y != 0)
                {
                    if (dotRect.X + (int)(gpState.ThumbSticks.Left.X * 5) < graphics.GraphicsDevice.Viewport.Width - dotRect.Width &&
                        dotRect.X + (int)(gpState.ThumbSticks.Left.X * 5) > 0)
                        dotRect.X += (int)(gpState.ThumbSticks.Left.X * 5);

                    if (dotRect.Y - (int)(gpState.ThumbSticks.Left.Y * 5) < graphics.GraphicsDevice.Viewport.Height - dotRect.Height &&
                        dotRect.Y - (int)(gpState.ThumbSticks.Left.Y * 5) > 0)
                        dotRect.Y -= (int)(gpState.ThumbSticks.Left.Y * 5);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState kbState = Keyboard.GetState();
            int moveSpeed = 5;

            if (kbState.IsKeyDown(Keys.Left) && dotRect.X > 0)
                dotRect.X -= moveSpeed;
            if (kbState.IsKeyDown(Keys.Right) && dotRect.X < graphics.GraphicsDevice.Viewport.Width - dotRect.Width)
                dotRect.X += moveSpeed;
            if (kbState.IsKeyDown(Keys.Up) && dotRect.Y > 0)
                dotRect.Y -= moveSpeed;
            if (kbState.IsKeyDown(Keys.Down) && dotRect.Y < graphics.GraphicsDevice.Viewport.Height - dotRect.Height)
                dotRect.Y += moveSpeed;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState kbState2 = Keyboard.GetState();
            int moveSpeed2 = 5;

            if (kbState.IsKeyDown(Keys.A) && dotRect2.X > 0)
                dotRect2.X -= moveSpeed2;
            if (kbState.IsKeyDown(Keys.D) && dotRect2.X < graphics.GraphicsDevice.Viewport.Width - dotRect.Width)
                dotRect2.X += moveSpeed2;
            if (kbState.IsKeyDown(Keys.W) && dotRect2.Y > 0)
                dotRect2.Y -= moveSpeed2;
            if (kbState.IsKeyDown(Keys.S) && dotRect2.Y < graphics.GraphicsDevice.Viewport.Height - dotRect.Height)
                dotRect2.Y += moveSpeed2;

            base.Update(gameTime);

            dotColor = new Color(redComponent, greenComponent, blueComponent, alphaComponent);
            message = "Red: " + redComponent.ToString() +
                      " Green: " + greenComponent.ToString() +
                      " Blue: " + blueComponent.ToString() +
                      " Alpha: " + alphaComponent.ToString();

            dotColor2 = new Color(redComponent2, greenComponent2, blueComponent2, alphaComponent2);
            message = "Red: " + redComponent2.ToString() +
                      " Green: " + greenComponent2.ToString() +
                      " Blue: " + blueComponent2.ToString() +
                      " Alpha: " + alphaComponent2.ToString();

            if (dotRect.Intersects(dotRect2))
            {
                if (!isColliding) 
                {
                    collisionSound.Play();
                    isColliding = true;
                }
                message = "Collision!";
            }
            else
            {
                isColliding = false;
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(background, backgroundRect, Color.White);
            spriteBatch.Draw(dot, dotRect, dotColor);
            spriteBatch.Draw(dot2, dotRect2, dotColor2);

            int stringWidth = (int)font.MeasureString(message).X;
            spriteBatch.DrawString(font, message, new Vector2((displayWidth - stringWidth) / 2, 0), Color.White);

            spriteBatch.DrawString(font, message, new Vector2((displayWidth - stringWidth) / 2, 0), Color.White);

            if (dotRect.Intersects(dotRect2))
            {
                if (!isColliding)
                {
                    collisionSound.Play();
                    isColliding = true;
                }
                message = "Collision!";
            }
            else
            {
                isColliding = false;
            }



            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
