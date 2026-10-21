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
        public event PropertyChangedEventHandler? PropertyChanged;
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
        private string artists = "Unknown";
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
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}
