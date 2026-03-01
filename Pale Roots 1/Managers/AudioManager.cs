using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Controls background music playback and crossfading between tracks...still kindof not 100% consistent but i cannot for the life of me figure out why,
    // figured out the main bug it had but sometimes still has long pauses between tracks, and sometimes the fade out doesn't work, but rarely.. i think.
    public class AudioManager
    {
        // Configuration values for fade speed and maximum volume.
        private float _fadeSpeed = 0.5f;
        private const float MaxVolume = 1.0f;

        // Current and target volumes used for smooth fades.
        private float _currentVolume = 0f;
        private float _targetVolume = 0f;

        // Currently playing song and the one queued to start next.
        private Song _currentSong;
        private Song _pendingSong;
        private bool _isSwitchingTrack = false;

        // Public properties for assigning music assets.
        public Song MenuSong { get; set; }
        public Song IntroSong { get; set; }
        public Song DeathSong { get; set; }
        public Song OutroSong { get; set; } // Used for Victory/Outro/Credits

        private List<Song> _combatSongs = new List<Song>();

        public AudioManager()
        {
            // Start muted so music can fade in.
            MediaPlayer.Volume = 0f;
        }

        // Add a song to the combat playlist.
        public void AddCombatSong(Song song)
        {
            _combatSongs.Add(song);
        }

        // Update per frame to advance fades and handle track switching.
        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Adjust current volume toward the target to implement fading.
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

            MediaPlayer.Volume = _currentVolume;

            // Only clear the switching flag once the media player reports it is playing.
            if (MediaPlayer.State == MediaState.Playing)
            {
                _isSwitchingTrack = false;
            }

            // Determine if the hardware finished the song or if we are effectively silent.
            bool songFinished = (MediaPlayer.State == MediaState.Stopped);
            bool fadeComplete = (_currentVolume <= 0.05f);

            // If not already switching and a song is pending and we are silent or the previous song finished, start it.
            if (!_isSwitchingTrack && _pendingSong != null && (fadeComplete || songFinished))
            {
                PlayImmediate(_pendingSong);
            }
        }

        // Request the appropriate music for the given game state.
        public void HandleMusicState(GameState state)
        {
            // If a track is already pending, do not override it.
            if (_pendingSong != null) return;

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
                case GameState.Victory:
                case GameState.Outro:
                case GameState.Credits:
                    RequestTrack(OutroSong, true);
                    break;

                case GameState.Gameplay:
                case GameState.LevelUp:
                    HandleCombatMusic();
                    break;
            }
        }

        // Choose or maintain combat music based on current player context.
        private void HandleCombatMusic()
        {
            // Do not pick a new combat track while a transition is pending.
            if (_pendingSong != null || _isSwitchingTrack) return;

            // Decide if we need a new track when coming from a non-combat theme or when playback stopped.
            bool isWrongTheme = (_currentSong != null && !_combatSongs.Contains(_currentSong));
            bool isSilence = (MediaPlayer.State == MediaState.Stopped);

            if (isWrongTheme || isSilence)
            {
                Song next = GetRandomCombatTrack();
                if (next != null)
                {
                    // Do not loop so the manager can pick a different track when it ends.
                    RequestTrack(next, false);
                }
            }
        }

        // Queue a song and begin fading out the current song.
        private void RequestTrack(Song song, bool loop)
        {

            if (_currentSong == song && _pendingSong == null)
            {
                MediaPlayer.IsRepeating = loop;
                return;
            }

            if (_pendingSong == song) return;

            MediaPlayer.IsRepeating = loop;
            _pendingSong = song;
            _targetVolume = 0.0f; // fade out current track
        }

        // Immediately start playback of the provided song and reset fade state.
        private void PlayImmediate(Song song)
        {
            try
            {
                // Tell the media player to start the requested song.
                MediaPlayer.Play(song);
                MediaPlayer.IsRepeating = false;

                // Update internal tracking of which song is active.
                _currentSong = song;
                _pendingSong = null; // clear pending request

                // Snap volume to maximum so the new song is audible immediately.
                _targetVolume = MaxVolume;
                _currentVolume = MaxVolume;
                MediaPlayer.Volume = _currentVolume;

                // Mark that we are busy switching tracks until the hardware reports playback.
                _isSwitchingTrack = true;
            }
            catch { }
        }

        // Pick a random combat track, avoiding the current track when possible.
        private Song GetRandomCombatTrack()
        {
            if (_combatSongs.Count == 0) return null;
            Song candidate;
            int attempts = 0;
            do
            {
                int index = CombatSystem.RandomInt(0, _combatSongs.Count);
                candidate = _combatSongs[index];
                attempts++;
            }
            while (candidate == _currentSong && attempts < 5);
            return candidate;
        }

        // Stop playback and reset internal state.
        public void Stop()
        {
            MediaPlayer.Stop();
            _currentSong = null;
            _pendingSong = null;
            _currentVolume = MaxVolume;
            _targetVolume = MaxVolume;
        }
    }
}