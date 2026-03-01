using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Manages a set of animations and draws the currently playing animation frame.
    public class AnimationManager
    {
        private Dictionary<string, Animation> _anims = new Dictionary<string, Animation>();
        private Animation _currentAnimation;
        private string _currentKey;

        private float _timer;
        public int CurrentFrame { get; private set; }

        public float LayerDepth { get; set; }

        // Register an animation under a string key for later playback.
        public void AddAnimation(string key, Animation animation)
        {
            _anims[key] = animation;
        }

        // Reset the current playback state so no animation is selected.
        public void Reset()
        {
            _currentKey = null;
            CurrentFrame = 0;
            _timer = 0;
        }

        // Start playing the named animation if it is not already active.
        public void Play(string key)
        {
            if (_currentKey == key) return;

            if (_anims.ContainsKey(key))
            {
                _currentKey = key;
                _currentAnimation = _anims[key];
                CurrentFrame = 0;
                _timer = 0;
            }
        }

        // Advance the frame timer and update the current frame based on elapsed time.
        // Uses the animation's FrameSpeed and looping flag to determine behavior.
        public void Update(GameTime gameTime)
        {
            if (_currentAnimation == null) return;

            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timer > _currentAnimation.FrameSpeed)
            {
                _timer = 0f;
                CurrentFrame++;

                if (CurrentFrame >= _currentAnimation.FrameCount)
                {
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

        // Draw the current animation frame at the given position and scale.
        // If the animation is a grid, use the direction row instead of sprite flipping.
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale, SpriteEffects effect, int direction = 0)
        {
            if (_currentAnimation == null) return;

            int frameWidth = _currentAnimation.FrameWidth;
            int frameHeight = _currentAnimation.FrameHeight;
            int currentRow = _currentAnimation.SheetRow;

            // If the animation uses a grid layout we select the row by direction
            // and avoid applying sprite effects so the sheet controls facing.
            if (_currentAnimation.IsGrid)
            {
                currentRow = direction;
                effect = SpriteEffects.None;
            }

            Rectangle source = new Rectangle(
                CurrentFrame * frameWidth,
                currentRow * frameHeight,
                frameWidth,
                frameHeight
            );

            // Use the sprite feet as the origin so the sprite is positioned by its base.
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);

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
