using MusicPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using TagLib;
using static MusicPlayer.Models.MusicLibrary;

namespace MusicPlayer.Models
{
    public class MusicLibrary
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
        public AddFolderResult ValidateNewFolderPath(string path, IEnumerable<ScanFolder> targetScanFolderList, out string normalizePath)
        {
            normalizePath = path;
            if (!Directory.Exists(path))
            {
                Debug.Print($"MusicLibrary.AddScanFolder: Thu muc {path} khong ton tai");
                return AddFolderResult.NotFound;
            }

            normalizePath = Path.GetFullPath(path);
            if (normalizePath != Path.GetPathRoot(normalizePath))
            {
                normalizePath = normalizePath.TrimEnd(Path.DirectorySeparatorChar);
            }

            foreach (ScanFolder existsfolder in targetScanFolderList)
            {
                if (string.Equals(existsfolder.Path, normalizePath, StringComparison.OrdinalIgnoreCase))
                    return AddFolderResult.Duplicate;

                if (normalizePath.StartsWith(EnsureTraillingSeparatorCharPath(existsfolder.Path), StringComparison.OrdinalIgnoreCase))
                    return AddFolderResult.SubFolder;
                if (existsfolder.Path.StartsWith(EnsureTraillingSeparatorCharPath(normalizePath), StringComparison.OrdinalIgnoreCase))
                    return AddFolderResult.ParentOfExisting;
            }

            return AddFolderResult.Success;
        }
        public AddFolderResult AddScanFolder(string path)
        {
            AddFolderResult addFolderResult = ValidateNewFolderPath(path, ScanFolders, out string normalizePath);
            if (addFolderResult == AddFolderResult.Success)
            {
                ScanFolder scanFolder = libraryDatabase.AddScanFolder(normalizePath);
                ScanFolders.Add(scanFolder);
                LoadFromFolder(scanFolder);
            }
            return addFolderResult;
        }
        public void RemoveScanFolder(ScanFolder requestFolder)
        {
            if (!ScanFolders.Contains(requestFolder))
                return;

            libraryDatabase.DeleteScanFolder(requestFolder.Id);

            var songToRemove = Songs.Where(s => s.FolderId == requestFolder.Id).ToList();

            foreach (Song song in songToRemove)
            {
                Songs.Remove(song);
            }
            ScanFolders.Remove(requestFolder);
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

                    newSong.FolderId = scanFolder.Id;
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
        public void Refresh()
        {
            foreach (ScanFolder scanFolder in ScanFolders)
            {
                LoadFromFolder(scanFolder);
            }
        }
        public List<string> UpdateScanFolders(IEnumerable<ScanFolder> targetScanFolderList)
        {
            List<ScanFolder> pendingRemoveFolders = ScanFolders.Where(f => !targetScanFolderList.Any(rf => rf.Id == f.Id)).ToList();

            List<string> errorList = new List<string>();

            foreach (ScanFolder requestFolder in pendingRemoveFolders)
            {
                try
                {
                    RemoveScanFolder(requestFolder);
                }
                catch (Exception ex)
                {
                    errorList.Add($"Fail to remove {requestFolder.Path}, Message: {ex.Message}");
                }
            }

            List<ScanFolder> pendingAddFolders = targetScanFolderList.Where(s => s.Id == 0).ToList();

            foreach (ScanFolder requestFolder in pendingAddFolders)
            {
                try
                {
                    switch (AddScanFolder(requestFolder.Path))
                    {
                        case AddFolderResult.Duplicate:
                            errorList.Add($"Fail to add {requestFolder.Path}, this folder already exists in your library");
                            break;
                        case AddFolderResult.SubFolder:
                            errorList.Add($"Fail to add {requestFolder.Path}, the parent folder of this folder is already in your library.");
                            break;
                        case MusicLibrary.AddFolderResult.NotFound:
                            errorList.Add($"Fail to add {requestFolder.Path}, this is not a valid folder.");
                            break;
                        case MusicLibrary.AddFolderResult.ParentOfExisting:
                            errorList.Add($"Fail to add {requestFolder.Path}, this folder contains another folder that is already in your library");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errorList.Add($"Fail to add {requestFolder.Path}, Message: {ex.Message}");
                }
            }

            return errorList;
        }
    }
}