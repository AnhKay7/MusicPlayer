using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MusicPlayer.ViewModels
{
    public class TagEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? SaveCompleted;
        public ICommand SaveCommand { get; }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Song originalSong;
        private MusicLibrary musicLibrary;
        public TagEditorViewModel(Song song, MusicLibrary musicLibrary) 
        {
            using var tagFile = TagLib.File.Create(song.FilePath);

            originalSong = song;

            this.musicLibrary = musicLibrary;

            Title = tagFile.Tag.Title ?? "";

            Artists = tagFile.Tag.JoinedPerformers ?? "";

            Album = tagFile.Tag.Album ?? "";

            Year = tagFile.Tag.Year == 0 ? "" : tagFile.Tag.Year.ToString();

            Genre = tagFile.Tag.JoinedGenres ?? "";

            SaveCommand = new RelayCommand(Save, CanSave);
        }

        private void Save()
        {
            Debug.Print($"TagEditorViewModel: {Title}, {Artists}, {Album}, {Year}, {Genre}");

            try
            {
                musicLibrary.WriteSongTags(
                        originalSong,
                        title, artists, album,
                        uint.TryParse(year, out uint parseYear) ? parseYear : 0,
                        genre
                );
                SaveCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error happen when write new tag. Message: {ex.Message}", "File Tag Writing Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanSave()
        {
            if (Year != "" && !uint.TryParse(Year, out _))
                return false;

            return true;
            //string origTitle = originalSong.Title ?? "";
            //string origArtists = originalSong.Artists ?? ""; 
            //string origAlbum = originalSong.Album ?? "";
            ////string origGenre = originalSong.Genre ?? "";
            ////string origYear = originalSong.Year == 0 ? "" : originalSong.Year.ToString();

            //return Title != origTitle
            //    || Artists != origArtists
            //    || Album != origAlbum;
            //    //|| Year != origYear
            //    //|| Genre != origGenre;
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
        private string album = "";
        public string Album
        {
            get { return album; }
            set
            {
                album = value;
                OnPropertyChanged();
            }
        }
        private string artists = "";
        public string Artists
        {
            get { return artists; }
            set
            {
                artists = value;
                OnPropertyChanged();
            }
        }

        private string year = "";
        public string Year
        {
            get { return year; }
            set
            {
                year = value;
                OnPropertyChanged();
            }
        }

        private string genre = "";
        public string Genre
        {
            get { return genre; }
            set
            {
                genre = value;
                OnPropertyChanged();
            }
        }

    }
}
