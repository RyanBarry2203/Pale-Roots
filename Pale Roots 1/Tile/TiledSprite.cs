using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Pale_Roots_1
{
    public class TiledSprite
    {
        //sprite texture and position

        private bool visible;
        protected Vector2 origin;
        private float angleOfRotation;
        protected float spriteDepth = 1f;
        protected Vector2 pixelPosition;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

       

        //the number of frames in the sprite sheet
        //the current fram in the animation
        //the time between frames
        int mililsecondsBetweenFrames = 100;
        float timer = 0f;

        //the width and height of our texture
        public int FrameWidth = 0;
        public int FrameHeight = 0;

        //the source of our image within the sprite sheet to draw
        Rectangle sourceRectangle;

        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }


        public Vector2 PixelPosition
        {
            get
            {
                return pixelPosition;
            }

            set { pixelPosition = value; }
        }

        public int CurrentFrame
        {
            get
            {
                return _currentFrame;
            }

            set
            {
                _currentFrame = value;
            }
        }

        public float AngleOfRotation { get => angleOfRotation; set => angleOfRotation = value; }

        protected List<TileRef> Frames = new List<TileRef>();
        private int _currentFrame;

        public TiledSprite(Vector2 tilePosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
        {
            spriteDepth = layerDepth;
            pixelPosition = tilePosition;
            visible = true;
            FrameHeight = frameHeight;
            FrameWidth = frameWidth;
            Frames = sheetRefs;
            // added to allow sprites to rotate
            origin = new Vector2(FrameWidth / 2, FrameHeight / 2);
            AngleOfRotation = 0;
            CurrentFrame = 0;
        }


        public virtual void Update(GameTime gametime)
        {
            timer += (float)gametime.ElapsedGameTime.Milliseconds;

            //if the timer is greater then the time between frames, then animate
            if (timer > mililsecondsBetweenFrames)
            {
                _currentFrame++;
                //if we have exceed the number of frames
                if (_currentFrame > Frames.Count - 1)
                {
                    _currentFrame = 0;
                }
                //reset our timer
                timer = 0f;
            }
            //set the source to be the current frame in our animation
            sourceRectangle = new Rectangle(Frames[CurrentFrame]._sheetPosX * FrameWidth,
                    Frames[CurrentFrame]._sheetPosY * FrameHeight,
                    FrameWidth, FrameHeight);

        }
        public bool collisionDetect(TiledSprite other)
        {
            Rectangle myBound = new Rectangle((int)this.PixelPosition.X, (int)this.PixelPosition.Y, this.FrameWidth, this.FrameHeight);
            Rectangle otherBound = new Rectangle((int)other.PixelPosition.X, (int)other.PixelPosition.Y, other.FrameWidth, other.FrameHeight);

            return myBound.Intersects(otherBound);
        }

        



        public virtual void Draw(SpriteBatch spriteBatch, Texture2D SpriteSheet)
        {
            if (visible)
            {
                spriteBatch.Draw(SpriteSheet,
                    PixelPosition + origin, sourceRectangle,
                    Color.White, AngleOfRotation, origin,
                    1.0f, SpriteEffects.None, spriteDepth);
            }
        }

    }
}

