using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace MusicPlayer.Services
{
    public class AudioPlayer
    {
        public AudioPlayer()
        {
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }
        private MediaPlayer mediaPlayer = new MediaPlayer();
        public event EventHandler? StateChanged;

        private bool isPlaying = false;
        public bool IsPlaying => isPlaying;
        public TimeSpan GetCurrentPosition => mediaPlayer.Position;
        public void SeekCurrentPosition(TimeSpan timeSpan)
        {
            if (!hasSong)
                return;
            mediaPlayer.Position = timeSpan;
        }
        private bool hasSong = false;
        public bool HasSong => hasSong;
        private void SetPlaying(bool value)
        {
            isPlaying = value;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        public bool Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            Uri musicFile = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            mediaPlayer.Open(musicFile);
            mediaPlayer.Play();
            hasSong = true;
            SetPlaying(true);
            return true;
        }
        public void Pause()
        {
            mediaPlayer.Pause();
            SetPlaying(false);
        }
        public void Resume()
        {
            if (!hasSong)
                return;
            mediaPlayer.Play();
            SetPlaying(true);
        }
        private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Stop();
            SetPlaying(false);
        }
    }
}
