using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace MusicPlayer.Services
{
    class AudioPlayer
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public bool Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            Uri musicFile = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            mediaPlayer.Open(musicFile);
            mediaPlayer.Play();
            return true;
        }
    }
}
