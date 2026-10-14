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
    }
}
