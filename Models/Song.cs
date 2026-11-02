using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TagLib;

namespace MusicPlayer.Models
{
    public class Song : INotifyPropertyChanged
    {
        public const string TitleDefalt = "";
        public const string ArtistsDefault = "Unknown";
        public const string? AlbumDefault = null;

        public Song() { }
        public Song(Song cpySong)
        {
            this.Title = cpySong.Title;
            this.Album = cpySong.Album;
            this.Artists = cpySong.Artists;
            this.Duration = cpySong.Duration;
            this.FilePath = cpySong.FilePath;
            this.DateAdded = cpySong.DateAdded;
            this.Cover = cpySong.Cover;
            this.Id = cpySong.Id;
            this.FileSize = cpySong.FileSize;
            this.LastModified = cpySong.LastModified;
            this.FolderId = cpySong.FolderId;
        }
        public void CpyFrom(Song cpySong)
        {
            this.Title = cpySong.Title;
            this.Album = cpySong.Album;
            this.Artists = cpySong.Artists;
            this.Duration = cpySong.Duration;
            this.FilePath = cpySong.FilePath;
            this.DateAdded = cpySong.DateAdded;
            this.Cover = cpySong.Cover;
            this.Id = cpySong.Id;
            this.FileSize = cpySong.FileSize;
            this.LastModified = cpySong.LastModified;
            this.FolderId = cpySong.FolderId;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string title = "";
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }
        private string? album = null;
        public string? Album
        {
            get { return album;}
            set
            {
                album = value;
                OnPropertyChanged();
            }
        }
        private string artists = ArtistsDefault;
        public string Artists
        {
            get { return artists; }
            set
            {
                artists = value;
                OnPropertyChanged();
            }
        }
        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged();
            }
        }
        private string filePath = "";
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }
        private DateTime dateAdded = DateTime.Today;
        public DateTime DateAdded
        {
            get { return dateAdded; }
            set
            {
                dateAdded = value;
                OnPropertyChanged();
            }
        }

        private ImageSource? cover;
        public ImageSource? Cover
        {
            get { return cover; }
            set
            {
                cover = value;
                OnPropertyChanged();
            }
        }
        public void LoadNewCoverArt()
        {
            if (String.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                Cover = null;
                return;
            }
            try
            {
                using var tagFile = TagLib.File.Create(filePath);
                IPicture[] pictures = tagFile.Tag.Pictures;
                IPicture? coverArt =
                    pictures.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover) ?? 
                    pictures.FirstOrDefault();
                if (coverArt == null)
                {
                    Cover = null;
                    return;
                }
                byte[] bytes = coverArt.Data.Data;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();
                Cover = bitmap;
            }
            catch
            {
                Cover = null;
            }
        }
        public void UnloadCoverArt() => Cover = null;

        public long Id { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; } = DateTime.Today;
        public long? FolderId { get; set; }
    }
}
