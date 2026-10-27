using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer.Models
{
    public class PlayQueue 
    {
        private List<Song> songs = new List<Song>();
        private int currentIdx;
        public Song? Current => songs.Count != 0 ? songs[currentIdx] : null;
        public void Load(IEnumerable<Song> songs, Song startSong)
        {
            if (songs == null)
                return;

            this.songs = songs.ToList();
            currentIdx = this.songs.IndexOf(startSong);
            if (currentIdx == -1)
                currentIdx = 0;
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
    }
}
