using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Base class for visible game objects that handles position, animation, collision, and drawing.
    public class Sprite
    {
        // --- VISUALS ---
        protected Texture2D spriteImage;
        protected Game game;
        protected Vector2 origin; // center point used for rotation and scaling
        protected float angleOfRotation;
        protected int spriteDepth = 1;

        // --- CIRCULAR ARENA PHYSICS ---
        // Enables circular bounds used by the boss arena logic.
        public bool UseCircularBounds { get; set; } = false;
        public Vector2 CircularBoundsCenter { get; set; }
        public float CircularBoundsRadius { get; set; }

        // --- COMBAT DATA ---
        public int AttackerCount { get; set; } = 0;
        public Sprite CurrentCombatPartner;
        public bool Visible = true;
        public int Health { get; set; } = 10000;
        public float AttackCooldown = 0f;
        public float AttackSpeed = 1000f;

        // --- TRANSFORM ---
        public Vector2 position; // world position used as the object's logical center
        public double Scale { get; set; }

        // --- ANIMATION ---
        protected int numberOfFrames = 0;
        protected int currentFrame = 0;
        protected int mililsecondsBetweenFrames = 100;
        protected float timer = 0f;
        public int spriteWidth = 0;
        public int spriteHeight = 0;
        public Rectangle sourceRectangle; // source rectangle for the current animation frame

        protected int _sheetStartX = 0;
        protected int _sheetStartY = 0;

        public Vector2 Center => position;

        public Sprite(Game g, Texture2D texture, Vector2 userPosition, int framecount, double scale)
        {
            this.game = g;
            this.spriteImage = texture;
            this.position = userPosition;
            this.numberOfFrames = framecount;
            this.Scale = scale;
            this.spriteHeight = spriteImage.Height;
            this.spriteWidth = spriteImage.Width / framecount;

            // origin is set to the center of a single frame for correct rotation and scaling
            this.origin = new Vector2(spriteWidth / 2f, spriteHeight / 2f);
            this.sourceRectangle = new Rectangle(0, 0, spriteWidth, spriteHeight);
        }

        public virtual void follow(Sprite target) { }

        public virtual void Update(GameTime gametime)
        {
            // advance animation frames based on elapsed time
            timer += (float)gametime.ElapsedGameTime.TotalMilliseconds;
            if (timer > mililsecondsBetweenFrames)
            {
                currentFrame++;
                if (currentFrame >= numberOfFrames) currentFrame = 0;
                timer = 0f;
            }

            // update the source rectangle to the current frame on the sprite sheet
            int frameOffsetX = currentFrame * spriteWidth;
            sourceRectangle = new Rectangle(_sheetStartX + frameOffsetX, _sheetStartY, spriteWidth, spriteHeight);
        }

        public void SetSpriteSheetLocation(Rectangle source)
        {
            // set a specific area of the sprite sheet for static or animated sprites
            _sheetStartX = source.X;
            _sheetStartY = source.Y;
            this.spriteWidth = source.Width;
            this.spriteHeight = source.Height;
            this.sourceRectangle = source;
            this.origin = new Vector2(spriteWidth / 2f, spriteHeight / 2f);
        }

        protected void ClampToMap()
        {
            // if circular bounds are enabled, keep the sprite inside the circle
            if (UseCircularBounds)
            {
                float dist = Vector2.Distance(position, CircularBoundsCenter);

                // account for sprite size so edges do not poke outside the circle
                float spriteRadius = (spriteWidth * (float)Scale) / 3f;
                float maxDist = CircularBoundsRadius - spriteRadius;

                if (dist > maxDist)
                {
                    Vector2 dir = position - CircularBoundsCenter;
                    if (dir != Vector2.Zero) dir.Normalize();
                    position = CircularBoundsCenter + (dir * maxDist);
                }
            }
            else
            {
                // clamp position to rectangular world bounds using the default map size
                float mapW = GameConstants.DefaultMapSize.X;
                float mapH = GameConstants.DefaultMapSize.Y;

                float clampPaddingX = 50f;
                float clampPaddingY = 50f;

                if (position.X < clampPaddingX) position.X = clampPaddingX;
                if (position.X > mapW - clampPaddingX) position.X = mapW - clampPaddingX;
                if (position.Y < clampPaddingY) position.Y = clampPaddingY;
                if (position.Y > mapH - clampPaddingY) position.Y = mapH - clampPaddingY;
            }
        }

        protected bool IsColliding(Vector2 newPos, List<WorldObject> objects)
        {
            if (objects == null) return false;

            // build a small feet-area rectangle for collision checks so sprites can pass behind tall visuals
            float scale = (float)Scale;
            int w = (int)(spriteWidth * scale * 0.4f);
            int h = (int)(spriteHeight * scale * 0.2f);

            int x = (int)(newPos.X - (w / 2));
            int y = (int)(newPos.Y + (spriteHeight * scale / 2) - h);

            Rectangle futureFeetBox = new Rectangle(x, y, w, h);

            // test intersection with solid world objects
            foreach (var obj in objects)
            {
                if (obj.IsSolid && futureFeetBox.Intersects(obj.CollisionBox))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                // draw the sprite using the current source rectangle, origin, scale and rotation
                spriteBatch.Draw(spriteImage, position, sourceRectangle,
                    Color.White, angleOfRotation, origin,
                    (float)Scale, SpriteEffects.None, spriteDepth);
            }
        }
    }
}