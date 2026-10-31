using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MusicPlayer.Models
{
    public class PlayQueue : INotifyPropertyChanged
    {
        private List<Song> songs = new List<Song>();
        private List<Song> shuffleSongs = new List<Song>();
        public Song? Current => songs.Count != 0 ? (isShuffleEnable ? shuffleSongs[currentIdx] : songs[currentIdx]) : null;
        private int currentIdx;
        public bool IsListFinished => songs.Count != 0 ? currentIdx == songs.Count - 1 : false;
        private bool isShuffleEnable = false;
        public bool IsShuffleEnable => isShuffleEnable;

        public enum RepeatMode
        {
            Off,
            All,
            One
        }
        private RepeatMode repeatMode = RepeatMode.Off;
        public RepeatMode CurrentRepeatMode => repeatMode;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Load(IEnumerable<Song> songs, Song startSong)
        {
            if (songs == null)
                return;

            this.songs = songs.ToList();
            currentIdx = this.songs.IndexOf(startSong);
            if (currentIdx == -1)
                currentIdx = 0;

            if (isShuffleEnable)
                Shuffle();
        }
        public void MoveNext()
        {
            if (songs.Count == 0)
                return;
            currentIdx = (currentIdx + 1) % songs.Count;
        }
        public void MovePrevious()
        {
            if (songs.Count == 0)
                return;
            currentIdx = (currentIdx - 1 + songs.Count) % songs.Count;
        }
        private void Shuffle()
        {
            if (songs.Count == 0)
                return;

            shuffleSongs = new List<Song>(songs);
            Random rnd = new Random();
            int n = shuffleSongs.Count;

            int currentSongIdx = shuffleSongs.IndexOf(songs[currentIdx]);
            (shuffleSongs[0], shuffleSongs[currentSongIdx]) = (shuffleSongs[currentSongIdx], shuffleSongs[0]);

            for (int i = n - 1; i >= 1; i--)
            {
                int ranNum = rnd.Next(1, i + 1);

                (shuffleSongs[i], shuffleSongs[ranNum]) = (shuffleSongs[ranNum], shuffleSongs[i]);
            }
            currentIdx = 0;
        }
        public void TurnOnShuffle()
        {
            if (isShuffleEnable)
                return;
            isShuffleEnable = true;
            Shuffle();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsShuffleEnable)));
        }
        public void TurnOffShuffle()
        {
            if (!isShuffleEnable)
                return;

            isShuffleEnable = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsShuffleEnable)));

            if (songs.Count == 0)
                return;
            if (shuffleSongs.Count == 0)
                currentIdx = 0;
            else
                currentIdx = songs.IndexOf(shuffleSongs[currentIdx]);

            if (currentIdx == -1)
                currentIdx = 0;
        }
        public void ChangeRepeatMode()
        {
            switch (repeatMode)
            {
                case RepeatMode.Off:
                    repeatMode = RepeatMode.All;
                    break;
                case RepeatMode.All:
                    repeatMode = RepeatMode.One;
                    break;
                default:
                    repeatMode = RepeatMode.Off;
                    break;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentRepeatMode)));
        }
    }
}
