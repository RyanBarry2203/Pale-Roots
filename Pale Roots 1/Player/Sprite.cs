using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Base Sprite: minimal drawable game object with simple animation and basic collision helpers.
    // - Used by Player, Enemy, Ally, WorldObject, Projectile and other actors.
    // - Provides sprite sheet frame logic, a simple "feet" collision box helper and map clamping.
    public class Sprite
    {
        // Texture and rendering helpers
        protected Texture2D spriteImage;
        protected Game game;
        protected Vector2 origin;
        protected float angleOfRotation;
        protected int spriteDepth = 1;

        // Battle / gameplay bookkeeping (kept lightweight so CombatSystem can read/write)
        public int AttackerCount { get; set; } = 0;
        public Sprite CurrentCombatPartner;
        public Enemy.AISTATE CurrentAIState = Enemy.AISTATE.Charging;
        public bool Visible = true;
        public int Health { get; set; } = 10000;
        public float AttackCooldown = 0f;
        public float AttackSpeed = 1000f; // milliseconds per attack

        // World position and scale (position is treated as the logical center point)
        public Vector2 position;
        public double Scale { get; set; }

        // Animation / spritesheet fields
        protected int numberOfFrames = 0;
        protected int currentFrame = 0;
        protected int mililsecondsBetweenFrames = 100;
        protected float timer = 0f;
        public int spriteWidth = 0;
        public int spriteHeight = 0;
        public Rectangle sourceRectangle;

        // Optional source rect offset for spritesheets that pack many sprites
        protected int _sheetStartX = 0;
        protected int _sheetStartY = 0;

        // Convenience: center for systems that use a logical center point for targeting
        public Vector2 Center
        {
            get { return position; }
        }

        // Constructor sets texture, frame count, scale and origin based on the supplied sheet.
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

        // Virtual placeholder; some subclasses (RotatingSprite) use a follow implementation.
        public virtual void follow(Sprite target) { }

        // Advance animation frames and update the source rectangle to the current frame.
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

        // Set a different source rectangle on the sheet (useful for Helper / WorldObject).
        public void SetSpriteSheetLocation(Rectangle source)
        {
            _sheetStartX = source.X;
            _sheetStartY = source.Y;

            this.spriteWidth = source.Width;
            this.spriteHeight = source.Height;
            this.sourceRectangle = source;

            this.origin = new Vector2(spriteWidth / 2f, spriteHeight / 2f);
        }

        // Keep the logical center point inside the map bounds (uses GameConstants.DefaultMapSize).
        // Call this after movement to avoid showing outside the world.
        protected void ClampToMap()
        {
            float mapW = GameConstants.DefaultMapSize.X;
            float mapH = GameConstants.DefaultMapSize.Y;

            float halfWidth = (spriteWidth * (float)Scale) / 2f;
            float halfHeight = (spriteHeight * (float)Scale) / 2f;

            if (position.X < halfWidth) position.X = halfWidth;
            if (position.X > mapW - halfWidth) position.X = mapW - halfWidth;

            if (position.Y < halfHeight) position.Y = halfHeight;
            if (position.Y > mapH - halfHeight) position.Y = mapH - halfHeight;
        }

        // Simple collision check used by movement helpers:
        // - Builds a small "feet" rectangle from newPos and checks against solid WorldObjects' CollisionBox.
        // - Returns true when a collision would occur (caller should avoid moving there).
        protected bool IsColliding(Vector2 newPos, List<WorldObject> objects)
        {
            if (objects == null) return false;

            // Feet box is a small rectangle near the bottom-center of the sprite.
            float scale = (float)Scale;
            int w = (int)(spriteWidth * scale * 0.4f); // 40% of sprite width
            int h = (int)(spriteHeight * scale * 0.2f); // 20% of sprite height

            int x = (int)(newPos.X - (w / 2));
            int y = (int)(newPos.Y + (spriteHeight * scale / 2) - h);

            Rectangle futureFeetBox = new Rectangle(x, y, w, h);

            foreach (var obj in objects)
            {
                if (obj.IsSolid && futureFeetBox.Intersects(obj.CollisionBox))
                {
                    return true;
                }
            }

            return false;
        }

        // Draw the sprite at `position` using the current sourceRectangle and origin.
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