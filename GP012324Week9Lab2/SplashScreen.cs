using Engines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Screens
{
    class SplashScreen : DrawableGameComponent
    {
        Texture2D _tx;
        private SpriteBatch _spriteBatch;
        private SpriteFont _nameID;
        private bool _wasActive = false;

        public bool Active { get; set; }

        public Texture2D Tx
        {
            get
            {
                return _tx;
            }

            set
            {
                _tx = value;
            }
        }
        public Song BackingTrack { get; set; }
        public SoundEffectInstance SoundPlayer { get; set; }
        public Vector2 Position { get; set; }

        public Keys ActivationKey;


        public SplashScreen(Game game, Vector2 pos, Texture2D tx, Song sound, Keys key) : base(game)
        {
            game.Components.Add(this);
            _tx = tx;
            BackingTrack = sound;
            Position = pos;
            ActivationKey = key;
        }
        public void LoadContent()
        {
            _spriteBatch = Game.Services.GetService<SpriteBatch>();
            _nameID = Game.Content.Load<SpriteFont>("nameID");

            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            // This handles the toggling internally. 
            // We removed the conflicting check from Game1.cs
            //if (InputEngine.IsKeyPressed(ActivationKey))
            //    Active = !Active;

            //if (Active)
            //{
            //    if (BackingTrack != null && MediaPlayer.State == MediaState.Stopped)
            //    {
            //        MediaPlayer.Play(BackingTrack);
            //    }
            //}
            //if (!Active)
            //{
            //    if (BackingTrack != null && MediaPlayer.State == MediaState.Playing && MediaPlayer.Queue.ActiveSong.Name == BackingTrack.Name)
            //    {
            //        MediaPlayer.Stop();
            //        // Could do resume and Pause if you want Media player state
            //    }
            //}

            // Toggle active state on key press
            if (InputEngine.IsKeyPressed(ActivationKey))
                Active = !Active;

            // --- LOGIC FIX START ---

            // CASE 1: Just turned ON
            if (Active && !_wasActive)
            {
                if (BackingTrack != null)
                {
                    MediaPlayer.Play(BackingTrack);
                }
            }
            // CASE 2: Just turned OFF
            else if (!Active && _wasActive)
            {
                // Only stop if the song playing is actually ours
                if (BackingTrack != null && MediaPlayer.Queue.ActiveSong == BackingTrack)
                {
                    MediaPlayer.Stop();
                }
            }

            // Update the tracker so we know what the state was last frame
            _wasActive = Active;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = _spriteBatch;

            if (spriteBatch == null) return;

            // COMBINED DRAW LOGIC
            // Previously you had two "if (Active)" blocks which caused double drawing.
            if (Active)
            {
                spriteBatch.Begin();

                // Draw splash background
                // Using the specific Position and Viewport size to fill screen
                spriteBatch.Draw(_tx,
                    new Rectangle(0, 0,
                        Game.GraphicsDevice.Viewport.Width,
                        Game.GraphicsDevice.Viewport.Height),
                    Color.White);

                // Draw your name ON TOP
                // Only draw text if the font loaded correctly
                if (_nameID != null)
                {
                    string nameID = "S00250496 - Ryan Barry";
                    Vector2 textSize = _nameID.MeasureString(nameID);
                    Vector2 textPosition = new Vector2(
                        (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                        10);

                    spriteBatch.DrawString(_nameID, nameID, textPosition, Color.Red);
                }

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}