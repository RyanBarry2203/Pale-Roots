using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pale_Roots_1
{
    public class Sprite
    {
        protected Texture2D spriteImage;
        protected Game game;
        protected Vector2 origin;
        protected float angleOfRotation;
        protected int spriteDepth = 1;

        // BATTLE FIELDS
        public int AttackerCount = 0;
        public Sprite CurrentCombatPartner;
        public Enemy.AISTATE CurrentAIState = Enemy.AISTATE.Charging;
        public bool Visible = true;
        public int Health = 100;
        public float AttackCooldown = 0f;
        public float AttackSpeed = 1000f; // 1 second cooldown

        public Vector2 position;
        public double Scale { get; set; }

        // Animation Fields
        protected int numberOfFrames = 0;
        protected int currentFrame = 0;
        protected int mililsecondsBetweenFrames = 100;
        protected float timer = 0f;
        public int spriteWidth = 0;
        public int spriteHeight = 0;
        public Rectangle sourceRectangle;

        protected int _sheetStartX = 0;
        protected int _sheetStartY = 0;

        public Vector2 Center
        {
            get
            {
                return position + new Vector2((spriteWidth * (float)Scale) / 2f, (spriteHeight * (float)Scale) / 2f);
            }
        }

        public Sprite(Game g, Texture2D texture, Vector2 userPosition, int framecount, double scale)
        {
            this.game = g;
            this.spriteImage = texture;
            this.position = userPosition;
            this.numberOfFrames = framecount;
            this.Scale = scale;
            this.spriteHeight = spriteImage.Height;
            this.spriteWidth = spriteImage.Width / framecount;
            this.origin = new Vector2(spriteWidth / 2f, spriteHeight / 2f);
            this.sourceRectangle = new Rectangle(0, 0, spriteWidth, spriteHeight);
        }

        public virtual void follow(Sprite target) { }

        public virtual void Update(GameTime gametime)
        {
            timer += (float)gametime.ElapsedGameTime.TotalMilliseconds;
            if (timer > mililsecondsBetweenFrames)
            {
                currentFrame++;
                if (currentFrame >= numberOfFrames) currentFrame = 0;
                timer = 0f;
            }
            int frameOffsetX = currentFrame * spriteWidth;
            sourceRectangle = new Rectangle(_sheetStartX + frameOffsetX, _sheetStartY, spriteWidth, spriteHeight);
        }
        public void SetSpriteSheetLocation(Rectangle source)
        {
            _sheetStartX = source.X;
            _sheetStartY = source.Y;

            this.spriteWidth = source.Width;
            this.spriteHeight = source.Height;
            this.sourceRectangle = source;

            this.origin = new Vector2(spriteWidth / 2f, spriteHeight / 2f);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                spriteBatch.Draw(spriteImage, position, sourceRectangle,
                    Color.White, angleOfRotation, origin,
                    (float)Scale, SpriteEffects.None, spriteDepth);
            }
        }
    }
}