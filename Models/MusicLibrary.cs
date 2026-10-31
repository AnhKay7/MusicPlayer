using MusicPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using TagLib;

namespace MusicPlayer.Models
{
    class MusicLibrary
    {
        private LibraryDatabase libraryDatabase = new LibraryDatabase();
        public ObservableCollection<Song> Songs { get; } = new ObservableCollection<Song>();

        public void LoadFromDatabase()
        {
            libraryDatabase.CreateTables();
            List<Song> databaseSong = libraryDatabase.GetAllSongs();
            foreach (Song song in databaseSong)
            {
                Songs.Add(song);
            }
        }
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

                try
                {
                    using var tagFile = TagLib.File.Create(x);

                    Song newSong = new Song();
                    newSong.FilePath = x;

                    newSong.Title = Path.GetFileNameWithoutExtension(x);
                    if (tagFile.Tag.Title != null && tagFile.Tag.Title != "")
                    {
                        newSong.Title = tagFile.Tag.Title;
                    }

                    if (tagFile.Tag.JoinedPerformers != null && tagFile.Tag.JoinedPerformers != "")
                    {
                        newSong.Artists = tagFile.Tag.JoinedPerformers;
                    }

                    if (tagFile.Tag.Album != null && tagFile.Tag.Album != "")
                    {
                        newSong.Album = tagFile.Tag.Album;
                    }

                    FileInfo fileInfo = new System.IO.FileInfo(x);
                    newSong.FileSize = fileInfo.Length;
                    newSong.LastModified = fileInfo.LastWriteTime;

                    newSong.Duration = tagFile.Properties.Duration;
                    newSong.DateAdded = DateTime.Now;


                    libraryDatabase.InsertSong(newSong);
                    Songs.Add(newSong);
                    numberOfAddedSong++;
                }
                catch
                {
                    Debug.Print($"Can't add music with file path: {x}");
                    continue;
                }
            }

            return true;
        }
    }
}
