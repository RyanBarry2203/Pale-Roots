using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    public class Sprite
    {
        // Internal fields
        protected Texture2D spriteImage;
        protected Game game;
        protected Vector2 origin;
        protected float angleOfRotation;
        protected int spriteDepth = 1;
        private bool visible = true; // Backing field for the property

        public Vector2 position;
        public int AttackerCount = 0;
        public Sprite CurrentCombatPartner;
        public Enemy.AISTATE CurrentAIState = Enemy.AISTATE.Charging;

        // Animation fields
        int numberOfFrames = 0;
        int currentFrame = 0;
        int mililsecondsBetweenFrames = 100;
        float timer = 0f;
        public int spriteWidth = 0;
        public int spriteHeight = 0;
        Rectangle sourceRectangle;

        // Properties
        public double Scale { get; set; }
        static protected Rectangle CameraRect;
        public float Speed { get; set; } = 2.0f; // Default speed

        // Corrected Center Property (Uses frame width for sprite sheets)
        public Vector2 Center
        {
            get { return position + new Vector2(spriteWidth / 2f, spriteHeight / 2f); }
        }

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public Texture2D SpriteImage
        {
            get { return spriteImage; }
            set { spriteImage = value; }
        }

        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }

        protected Vector2 WorldOrigin
        {
            get { return position + origin; }
        }

        // Constructor
        public Sprite(Game g, Texture2D texture, Vector2 userPosition, int framecount, double scale)
        {
            Scale = scale;
            this.game = g;
            spriteImage = texture;
            position = userPosition;
            numberOfFrames = framecount;
            spriteHeight = spriteImage.Height;
            visible = true;
            spriteWidth = spriteImage.Width / framecount;

            origin = new Vector2(spriteWidth / 2, spriteHeight / 2);
            angleOfRotation = 0;

            CameraRect = new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            sourceRectangle = new Rectangle(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);
        }

        public virtual void follow(Sprite target)
        {
            if (target == null) return;

            // 1. Calculate the vector pointing from us to the target
            Vector2 direction = target.Center - this.Center;

            // 2. Only move if we aren't already on top of the target
            if (direction.Length() > 5.0f)
            {
                direction.Normalize(); // Make the vector 1 unit long
                position += direction * Speed; // Move toward target
            }
        }

        public virtual void Update(GameTime gametime)
        {
            timer += (float)gametime.ElapsedGameTime.Milliseconds;

            if (timer > mililsecondsBetweenFrames)
            {
                currentFrame++;
                if (currentFrame > numberOfFrames - 1)
                {
                    currentFrame = 0;
                }
                timer = 0f;
            }
            sourceRectangle = new Rectangle(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);
        }

        public bool collisionDetect(Sprite other)
        {
            Rectangle myBound = new Rectangle((int)this.position.X, (int)this.position.Y, this.spriteWidth, this.spriteHeight);
            Rectangle otherBound = new Rectangle((int)other.position.X, (int)other.position.Y, other.spriteWidth, other.spriteHeight);
            return myBound.Intersects(otherBound);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (visible)
            {
                spriteBatch.Draw(spriteImage,
                    position, sourceRectangle,
                    Color.White, angleOfRotation, origin,
                    (float)Scale, SpriteEffects.None, spriteDepth);
            }
        }
    }
}