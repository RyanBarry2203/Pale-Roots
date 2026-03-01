using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // The foundational class for every physical object in the game.
    // It handles the basic requirements of "existing": having a position, playing an animation,
    // staying inside the map, and not walking through trees.
    public class Sprite
    {
        // --- VISUALS ---
        protected Texture2D spriteImage;
        protected Game game;
        protected Vector2 origin; // The "handle" of the sprite, mathematically centered for rotation.
        protected float angleOfRotation;
        protected int spriteDepth = 1;

        // --- CIRCULAR ARENA PHYSICS ---
        // Used primarily for the Boss Fight to keep the player locked in a circular zone.
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
        public Vector2 position; // The logical center point of the object in the world.
        public double Scale { get; set; }

        // --- ANIMATION ---
        protected int numberOfFrames = 0;
        protected int currentFrame = 0;
        protected int mililsecondsBetweenFrames = 100;
        protected float timer = 0f;
        public int spriteWidth = 0;
        public int spriteHeight = 0;
        public Rectangle sourceRectangle; // The "window" into the sprite sheet showing the current frame.

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

            // Set the origin to the center of a single frame so rotation and scaling happen from the middle.
            this.origin = new Vector2(spriteWidth / 2f, spriteHeight / 2f);
            this.sourceRectangle = new Rectangle(0, 0, spriteWidth, spriteHeight);
        }

        public virtual void follow(Sprite target) { }

        public virtual void Update(GameTime gametime)
        {
            // --- ANIMATION ENGINE ---
            timer += (float)gametime.ElapsedGameTime.TotalMilliseconds;
            if (timer > mililsecondsBetweenFrames)
            {
                currentFrame++;
                if (currentFrame >= numberOfFrames) currentFrame = 0;
                timer = 0f;
            }

            // Calculate the X-offset on the sprite sheet to show the next frame of animation.
            int frameOffsetX = currentFrame * spriteWidth;
            sourceRectangle = new Rectangle(_sheetStartX + frameOffsetX, _sheetStartY, spriteWidth, spriteHeight);
        }

        public void SetSpriteSheetLocation(Rectangle source)
        {
            // Manually slice a specific part of the sheet. Used for static world props like rocks.
            _sheetStartX = source.X;
            _sheetStartY = source.Y;
            this.spriteWidth = source.Width;
            this.spriteHeight = source.Height;
            this.sourceRectangle = source;
            this.origin = new Vector2(spriteWidth / 2f, spriteHeight / 2f);
        }

        protected void ClampToMap()
        {
            // --- RADIAL CLAMPING (BOSS ARENA) ---
            if (UseCircularBounds)
            {
                float dist = Vector2.Distance(position, CircularBoundsCenter);

                // Account for the sprite's width so the edge of the character doesn't poke out of the circle.
                float spriteRadius = (spriteWidth * (float)Scale) / 3f;
                float maxDist = CircularBoundsRadius - spriteRadius;

                if (dist > maxDist)
                {
                    // If the sprite tries to leave the circle, find the angle of exit and force them back onto the rim.
                    Vector2 dir = position - CircularBoundsCenter;
                    if (dir != Vector2.Zero) dir.Normalize();
                    position = CircularBoundsCenter + (dir * maxDist);
                }
            }
            // --- RECTANGULAR CLAMPING (OVERWORLD) ---
            else
            {
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

            // --- "FEET" COLLISION LOGIC ---
            // We don't check for collisions on the whole body. We create a small box at the 
            // bottom 20% of the sprite. This allows the player to walk behind the tops of 
            // trees while still being blocked by the physical trunks.
            float scale = (float)Scale;
            int w = (int)(spriteWidth * scale * 0.4f);
            int h = (int)(spriteHeight * scale * 0.2f);

            int x = (int)(newPos.X - (w / 2));
            int y = (int)(newPos.Y + (spriteHeight * scale / 2) - h);

            Rectangle futureFeetBox = new Rectangle(x, y, w, h);

            // Check if this "feet" box would intersect with any solid objects in the world.
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
                // Draw the current animation frame to the screen.
                spriteBatch.Draw(spriteImage, position, sourceRectangle,
                    Color.White, angleOfRotation, origin,
                    (float)Scale, SpriteEffects.None, spriteDepth);
            }
        }
    }
}