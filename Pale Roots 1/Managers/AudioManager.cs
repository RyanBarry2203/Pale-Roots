using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This class acts as the master DJ for the game, handling all background music.
    // It manages smooth volume fading between tracks and ensures the correct music plays based on the active GameState.
    public class AudioManager
    {
        // Core configuration for how fast the volume shifts during a crossfade.
        private float _fadeSpeed = 0.5f;
        private const float MaxVolume = 1.0f;

        // Tracks the math for volume adjustments frame-by-frame.
        private float _currentVolume = 0f;
        private float _targetVolume = 0f;

        // Pointers to figure out what is playing now vs what we are trying to transition into.
        private Song _currentSong;
        private Song _pendingSong;

        // A critical safety flag. XNA's MediaPlayer sometimes takes a few frames to actually start a song,
        // so we use this to prevent the manager from accidentally spam-triggering Play() multiple times.
        private bool _isSwitchingTrack = false;

        // Specific tracks loaded directly by Game1 that correlate to specific menu or cinematic states.
        public Song MenuSong { get; set; }
        public Song IntroSong { get; set; }
        public Song DeathSong { get; set; }
        public Song OutroSong { get; set; }

        // We keep a separate list of intense combat tracks so we can randomly shuffle them during gameplay.
        private List<Song> _combatSongs = new List<Song>();

        public AudioManager()
        {
            // We start the global volume at 0 so the first song smoothly fades in when the game launches.
            MediaPlayer.Volume = 0f;
        }

        public void AddCombatSong(Song song)
        {
            _combatSongs.Add(song);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;


            // If our actual volume doesn't match our desired volume, step it forward or backward 
            // based on the fade speed and delta time.
            if (_currentVolume < _targetVolume)
            {
                _currentVolume += _fadeSpeed * dt;
                if (_currentVolume > _targetVolume) _currentVolume = _targetVolume;
            }
            else if (_currentVolume > _targetVolume)
            {
                _currentVolume -= _fadeSpeed * dt;
                if (_currentVolume < _targetVolume) _currentVolume = _targetVolume;
            }

            // Apply the calculated volume to the actual hardware.
            MediaPlayer.Volume = _currentVolume;


            // We constantly poll the hardware. Once it confirms it is officially playing the audio stream,
            // we drop our safety flag so the manager can start listening for new track requests again.
            if (MediaPlayer.State == MediaState.Playing)
            {
                _isSwitchingTrack = false;
            }

            // Check if the current song naturally ended, or if our fade-out math reached the bottom.
            bool songFinished = (MediaPlayer.State == MediaState.Stopped);
            bool fadeComplete = (_currentVolume <= 0.05f);

            // If we have a new song queued up, aren't locked in a transition, and the old song is fully faded out...
            if (!_isSwitchingTrack && _pendingSong != null && (fadeComplete || songFinished))
            {
                // force the new song to start.
                PlayImmediate(_pendingSong);
            }
        }

        // The primary public method. The various GameStates call this, passing their own enum, 
        // and the manager decides what track should be playing.
        public void HandleMusicState(GameState state)
        {
            // If we are already in the middle of fading into a new track, ignore any new requests.
            if (_pendingSong != null) return;

            // Route the state enum to the correct hardcoded track.
            switch (state)
            {
                case GameState.Menu:
                    RequestTrack(MenuSong, true);
                    break;
                case GameState.Intro:
                    RequestTrack(IntroSong, false);
                    break;
                case GameState.GameOver:
                    RequestTrack(DeathSong, false);
                    break;

                // We combine several victory states to all point to the Outro track.
                case GameState.Victory:
                case GameState.Outro:
                case GameState.Credits:
                    RequestTrack(OutroSong, true);
                    break;

                // For actual active gameplay, we hand it off to a specialized method that shuffles the combat playlist.
                case GameState.Gameplay:
                case GameState.LevelUp:
                    HandleCombatMusic();
                    break;
            }
        }

        private void HandleCombatMusic()
        {
            // Safety check: Don't try to pick a new random combat song if we are already transitioning.
            if (_pendingSong != null || _isSwitchingTrack) return;

            // Figure out if we need to pick a new track. 
            // We need a new track if the current song isn't in our combat list (e.g., coming from a menu),
            // OR if the hardware tells us the current track naturally finished playing.
            bool isWrongTheme = (_currentSong != null && !_combatSongs.Contains(_currentSong));
            bool isSilence = (MediaPlayer.State == MediaState.Stopped);

            if (isWrongTheme || isSilence)
            {
                Song next = GetRandomCombatTrack();
                if (next != null)
                {
                    // Note: We pass 'false' for loop here, meaning when the song ends naturally, 
                    // the isSilence check above will trigger and we will pick a new random song for variety!
                    RequestTrack(next, false);
                }
            }
        }

        private void RequestTrack(Song song, bool loop)
        {
            // Ignore the request if we are already playing this exact song or already fading into it.
            if (_currentSong == song && _pendingSong == null) return;
            if (_pendingSong == song) return;

            // Apply the loop settings for the incoming track.
            MediaPlayer.IsRepeating = loop;

            // Queue the requested track and instantly drop the target volume to 0. 
            // This triggers the fade-out logic in the Update loop.
            _pendingSong = song;
            _targetVolume = 0.0f;
        }

        private void PlayImmediate(Song song)
        {
            try
            {
                // 1. Tell the XNA hardware to actively start streaming the audio data.
                MediaPlayer.Play(song);
                MediaPlayer.IsRepeating = false;

                // 2. Update our internal pointers to finalize the transition.
                _currentSong = song;
                _pendingSong = null;

                // 3. Immediately snap both target and current volume variables to Max.
                // We don't fade *in* for combat tracks; we hit hard on the beat.
                _targetVolume = MaxVolume;
                _currentVolume = MaxVolume;
                MediaPlayer.Volume = _currentVolume;

                // 4. Raise the safety flag so the update loop knows the hardware is busy loading the file.
                _isSwitchingTrack = true;
            }
            catch { }
        }

        private Song GetRandomCombatTrack()
        {
            // If the combat list is empty, safely bail out.
            if (_combatSongs.Count == 0) return null;

            Song candidate;
            int attempts = 0;

            // Try up to 5 times to pick a random track that IS NOT the exact same track we just finished playing.
            // This ensures we get nice variety while playing.
            do
            {
                int index = CombatSystem.RandomInt(0, _combatSongs.Count);
                candidate = _combatSongs[index];
                attempts++;
            }
            while (candidate == _currentSong && attempts < 5);

            return candidate;
        }

        public void Stop()
        {
            // Fully kills all audio and resets the manager. Used when hard-resetting the game session.
            MediaPlayer.Stop();
            _currentSong = null;
            _pendingSong = null;
            _currentVolume = MaxVolume;
            _targetVolume = MaxVolume;
        }
    }
}