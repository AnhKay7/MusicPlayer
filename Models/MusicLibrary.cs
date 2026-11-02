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
        public ObservableCollection<ScanFolder> ScanFolders { get; } = new ObservableCollection<ScanFolder>();

        public enum AddFolderResult
        {
            Success,
            Duplicate,
            SubFolder,
            NotFound,
            ParentOfExisting
        }
        private string EnsureTraillingSeparatorCharPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }
        public AddFolderResult AddScanFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Debug.Print($"MusicLibrary.AddScanFolder: Thu muc {path} khong ton tai");
                return AddFolderResult.NotFound;
            }

            string normalizePath = Path.GetFullPath(path);
            if (normalizePath != Path.GetPathRoot(normalizePath))
            {
                normalizePath = normalizePath.TrimEnd(Path.DirectorySeparatorChar);
            }

            foreach (ScanFolder existsfolder in ScanFolders)
            {
                if (string.Equals(existsfolder.Path, normalizePath, StringComparison.OrdinalIgnoreCase))
                    return AddFolderResult.Duplicate;

                if (normalizePath.StartsWith(EnsureTraillingSeparatorCharPath(existsfolder.Path), StringComparison.OrdinalIgnoreCase))
                    return AddFolderResult.SubFolder;
                if (existsfolder.Path.StartsWith(EnsureTraillingSeparatorCharPath(normalizePath), StringComparison.OrdinalIgnoreCase))
                    return AddFolderResult.ParentOfExisting;
            }

            ScanFolder scanFolder = libraryDatabase.AddScanFolder(normalizePath);
            ScanFolders.Add(scanFolder);
            LoadFromFolder(scanFolder);
            return AddFolderResult.Success;
        }
        public void LoadFromDatabase()
        {
            libraryDatabase.CreateTables();
            List<ScanFolder> databaseScanFolders = libraryDatabase.GetScanFolders();
            foreach (ScanFolder scanFolder in databaseScanFolders)
            {
                ScanFolders.Add(scanFolder);
            }

            List<Song> databaseSong = libraryDatabase.GetAllSongs();
            foreach (Song song in databaseSong)
            {
                Songs.Add(song);
            }

        }
        public void LoadFromFolder(ScanFolder scanFolder)
        {
            if (!Directory.Exists(scanFolder.Path))
            {
                Debug.Print($"MusicLibrary.LoadFromFolder: Thu muc {scanFolder.Path} khong ton tai");
                return;
            }
            string[] file = Directory.GetFiles(scanFolder.Path, "*.mp3", SearchOption.AllDirectories);

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

                    newSong.Title = System.IO.Path.GetFileNameWithoutExtension(x);
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


                    libraryDatabase.InsertSong(newSong, scanFolder.Id);
                    Songs.Add(newSong);
                }
                catch (Exception ex)
                {
                    Debug.Print($"Can't add music with file path: {x} because {ex.Message}");
                    continue;
                }
            }
        }
    }
}
