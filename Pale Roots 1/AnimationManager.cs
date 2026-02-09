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
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale, SpriteEffects effect)
        {
            if (_currentAnimation == null) return;

            int frameWidth = _currentAnimation.FrameWidth;
            int frameHeight = _currentAnimation.FrameHeight;

            Rectangle source = new Rectangle(
                CurrentFrame * frameWidth,
                _currentAnimation.SheetRow * frameHeight,
                frameWidth,
                frameHeight
            );


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
