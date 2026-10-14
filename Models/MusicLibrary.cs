using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace MusicPlayer.Models
{
    class MusicLibrary
    {
        public ObservableCollection<Song> Songs { get; } = new ObservableCollection<Song>();

        public bool LoadFromFolder(string musicPath, out int numberOfAddedSong)
        {
            numberOfAddedSong = 0;

            if (!Directory.Exists(musicPath))
            {
                return false;
            }

            string[] file = Directory.GetFiles(musicPath, "*.mp3", SearchOption.AllDirectories);

            foreach (string x in file)
            {
                if (Songs.Any(s => s.FilePath == x))
                {
                    continue;
                }
                Song newSong = new Song();
                newSong.FilePath = x;
                var tagFile = TagLib.File.Create(x);

                newSong.Title = Path.GetFileNameWithoutExtension(x);
                if (tagFile.Tag.Title != null && tagFile.Tag.Title != "")
                {
                    newSong.Title = tagFile.Tag.Title;
                }
                if (tagFile.Tag.JoinedPerformers != null && tagFile.Tag.Title != "")
                {
                    newSong.Artists = tagFile.Tag.JoinedPerformers;
                }
                newSong.Duration = tagFile.Properties.Duration;
                newSong.DateAdded = File.GetCreationTime(x);

                Songs.Add(newSong);
                numberOfAddedSong++;
            }

            return true;
        }
    }
}
