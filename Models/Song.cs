using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

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
        private string artist = "Unknown";
        public string Artist
        {
            get { return artist; }
            set
            {
                artist = value;
                OnPropertyChanged();
            }
        }
        private string duration = "00:00";
        public string Duration
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
        private string dateAdded = "";
        public string DateAdded
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
    }
}
