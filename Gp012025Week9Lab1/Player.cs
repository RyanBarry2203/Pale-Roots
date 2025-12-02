using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprites;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Sprites
{

    public class Player : Sprite
    {
        public enum DIRECTION { LEFT, RIGHT, UP, DOWN,STANDING };
        DIRECTION _direction = DIRECTION.STANDING;

        public DIRECTION Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
        int _score;
        float _speed;
        Texture2D[] _textures;
        SoundEffect[] _directionSounds;
        SoundEffectInstance _soundPlayer;
        private Vector2 _startPosition;

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }
        //public Healthbar Healthbar
        //{
        //    get { return healthbar; }
        //}

        public Player(Game game,Texture2D[] tx, SoundEffect[] sounds,
            Vector2 pos, int frameCount, 
            int startScore, float speed) 
            : base(game,tx[0],pos,frameCount)
        {
            _score = startScore;
            _speed = speed;
            _textures = tx;
            _directionSounds = sounds;
            _startPosition = pos;
            //healthbar = new Healthbar(pos + new Vector2(-SpriteWidth/ 2, -10), 100,game);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // TODO: Add your update logic here
            _direction = DIRECTION.STANDING;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                _direction = DIRECTION.LEFT;
                base.Move(new Vector2(-1, 0) * _speed);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                _direction = DIRECTION.UP;
                base.Move(new Vector2(0, -1) * _speed); 
            }
            if
            (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                _direction = DIRECTION.DOWN;
                base.Move(new Vector2(0, 1) * _speed);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                _direction = DIRECTION.RIGHT;
                base.Move(new Vector2(1, 0) * _speed);
            }
            SpriteImage = _textures[(int)_direction];

            if (_soundPlayer == null || _soundPlayer.State == SoundState.Stopped)
            {
                if (_direction != DIRECTION.STANDING)
                {
                    _soundPlayer = _directionSounds[(int)_direction].CreateInstance();
                    _soundPlayer.Play();
                }
            }
            Viewport vp = game.GraphicsDevice.Viewport;
            position.X = MathHelper.Clamp(position.X, 0, vp.Width - SpriteWidth);
            position.Y = MathHelper.Clamp(position.Y, 0, vp.Height - SpriteHeight);
        }
        public void ResetPosition()
        {
            position = _startPosition;
        }
    }
}
