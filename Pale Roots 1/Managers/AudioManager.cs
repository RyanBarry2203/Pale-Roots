using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class AudioManager
    {
        // Configuration
        private float _fadeSpeed = 0.5f;

        // State
        private float _currentVolume = 0f;
        private float _targetVolume = 0f;

        // Tracks
        private Song _currentSong;
        private Song _pendingSong;

        // Playlists
        public Song MenuSong { get; set; }
        public Song IntroSong { get; set; }
        public Song DeathSong { get; set; }
        public Song OutroSong { get; set; } // Used for Victory/Outro/Credits

        private List<Song> _combatSongs = new List<Song>();

        public AudioManager()
        {
            // Initialize volume to silent so we fade in
            MediaPlayer.Volume = 0f;
        }

        public void AddCombatSong(Song song)
        {
            _combatSongs.Add(song);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Fading Logic with Snapping
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

            // SAFETY CHECK: Ensure we don't get stuck at near-zero volume
            if (Math.Abs(_currentVolume - _targetVolume) < 0.01f)
            {
                _currentVolume = _targetVolume;
            }

            MediaPlayer.Volume = _currentVolume;

            // 2. Cross-Fade Execution
            // Increased threshold from 0.01f to 0.05f to prevent floating point misses
            if (_pendingSong != null && _currentVolume <= 0.05f)
            {
                try
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_pendingSong);

                    _currentSong = _pendingSong;
                    _pendingSong = null;
                    _targetVolume = 0.5f; // Fade back in
                }
                catch { }
            }
        }

        // The main interface for Game1 to ask for music
        public void HandleMusicState(GameState state)
        {
            // If we are currently fading out (Pending exists), wait.
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

        private void HandleCombatMusic()
        {
            // Check if we are playing a "Theme" song that shouldn't be here
            bool isWrongTheme = (_currentSong == MenuSong || _currentSong == IntroSong ||
                                 _currentSong == DeathSong || _currentSong == OutroSong);

            // Check if the current song has finished naturally
            bool isSilence = (MediaPlayer.State == MediaState.Stopped);

            if (isWrongTheme)
            {
                // This is a hard switch, so we fade out then in
                RequestTrack(GetRandomCombatTrack(), false);
            }
            else if (isSilence)
            {
                // Song ended naturally. Just play the next one.
                // DO NOT set volume to 0 here, or you get a silence gap.
                Song next = GetRandomCombatTrack();

                try
                {
                    MediaPlayer.Play(next);
                    _currentSong = next;
                    // Ensure volume is up
                    _targetVolume = 0.5f;
                    _currentVolume = 0.5f;
                }
                catch { }
            }
        }

        private void RequestTrack(Song song, bool loop)
        {
            if (_currentSong == song && _pendingSong == null) return;
            if (_pendingSong == song) return;

            MediaPlayer.IsRepeating = loop;
            _pendingSong = song;
            _targetVolume = 0.0f; // Fade out current
        }

        private void PlayImmediate(Song song)
        {
            try
            {
                MediaPlayer.Play(song);
                MediaPlayer.IsRepeating = false;
                _currentSong = song;
                _currentVolume = 0f;
                _targetVolume = 0.5f;
            }
            catch { }
        }

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

        public void Stop()
        {
            MediaPlayer.Stop();
            _currentSong = null;
            _pendingSong = null;
            _currentVolume = 0.5f;
            _targetVolume = 0.5f;
        }
    }
}