using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This class sits inside characters and enemies, managing their different sprite sheets.
    // Instead of manually tracking frames in every single entity class, they just tell this manager "Play Walk" and it handles the math.
    public class AnimationManager
    {
        // A lookup table storing all the different animations this specific character can perform.
        private Dictionary<string, Animation> _anims = new Dictionary<string, Animation>();

        // Tracks what is currently playing.
        private Animation _currentAnimation;
        private string _currentKey;

        // Tracks how much time has passed since we last swapped to a new frame.
        private float _timer;
        public int CurrentFrame { get; private set; }

        public float LayerDepth { get; set; }

        public void AddAnimation(string key, Animation animation)
        {
            _anims[key] = animation;
        }

        public void Reset()
        {
            // Fully clears out the current animation data. Useful for resetting characters when a level restarts.
            _currentKey = null;
            CurrentFrame = 0;
            _timer = 0;
        }

        public void Play(string key)
        {
            // If the requested animation is already playing, do nothing. 
            // We don't want to reset the frame counter back to 0 every single update loop!
            if (_currentKey == key) return;

            // If we actually have the animation loaded, swap to it and reset the timers so it starts from the first frame.
            if (_anims.ContainsKey(key))
            {
                _currentKey = key;
                _currentAnimation = _anims[key];
                CurrentFrame = 0;
                _timer = 0;
            }
        }

        public void Update(GameTime gameTime)
        {
            // Safety check. Don't try to update timers if we haven't told the manager to play anything yet.
            if (_currentAnimation == null) return;

            // Add the elapsed milliseconds since the last frame to our internal timer.
            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check if enough time has passed to step forward to the next frame in the sprite sheet.
            if (_timer > _currentAnimation.FrameSpeed)
            {
                // Reset the timer and bump the frame index.
                _timer = 0f;
                CurrentFrame++;

                // If we've reached the end of the sprite sheet row
                if (CurrentFrame >= _currentAnimation.FrameCount)
                {
                    // and the animation is set to loop (like walking or idling), wrap back to the start.
                    if (_currentAnimation.IsLooping)
                    {
                        CurrentFrame = 0;
                    }
                    else
                    {
                        CurrentFrame = _currentAnimation.FrameCount - 1;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale, SpriteEffects effect, int direction = 0)
        {
            if (_currentAnimation == null) return;

            // Grab the specific dimensions for this sprite from the animation object.
            int frameWidth = _currentAnimation.FrameWidth;
            int frameHeight = _currentAnimation.FrameHeight;
            int currentRow = _currentAnimation.SheetRow;

            // If the sprite sheet is a grid containing 4 different facings (Up, Down, Left, Right)...
            if (_currentAnimation.IsGrid)
            {
                // Override the row we pull the frame from based on the direction the character is facing.
                // Because the grid provides art for all 4 directions, we explicitly disable XNA's built-in image flipping.
                currentRow = direction;
                effect = SpriteEffects.None;
            }

            // Mathematically define the exact box on the sprite sheet we want to cut out and draw this frame.
            Rectangle source = new Rectangle(
                CurrentFrame * frameWidth,
                currentRow * frameHeight,
                frameWidth,
                frameHeight
            );

            // We set the origin point to the bottom center of the sprite. 
            // This ensures characters are drawn "standing" on their X/Y coordinate, not hanging from it by their top-left corner.
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);

            // Send everything to the graphics card to actually draw it on screen.
            spriteBatch.Draw(
                _currentAnimation.Texture,
                position,
                source,
                Color.White,
                0f,
                origin,
                scale,
                effect,
                LayerDepth
            );
        }
    }
}