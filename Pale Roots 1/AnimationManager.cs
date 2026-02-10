using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class AnimationManager
    {
        private Dictionary<string, Animation> _anims = new Dictionary<string, Animation>();
        private Animation _currentAnimation;
        private string _currentKey;

        private float _timer;
        public int CurrentFrame { get; private set; }

        public float LayerDepth { get; set; }

        public void AddAnimation(string key, Animation animation)
        {
            _anims[key] = animation;
        }
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
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale, SpriteEffects effect, int direction = 0)
        {
            if (_currentAnimation == null) return;

            int frameWidth = _currentAnimation.FrameWidth;
            int frameHeight = _currentAnimation.FrameHeight;
            int currentRow = _currentAnimation.SheetRow;

            // POLYMORPHIC LOGIC:
            // If it's a Grid (Enemy/Ally), we ignore the 'effect' flip and change the Y row instead.
            // Assuming standard sheets: 0:Down, 1:Left, 2:Right, 3:Up (Adjust based on your asset)
            if (_currentAnimation.IsGrid)
            {
                currentRow = direction;
                effect = SpriteEffects.None; // Grid handles direction, so don't flip
            }

            Rectangle source = new Rectangle(
                CurrentFrame * frameWidth,
                currentRow * frameHeight,
                frameWidth,
                frameHeight
            );

            // Origin at Bottom Center (Feet)
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
