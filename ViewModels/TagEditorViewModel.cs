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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MusicPlayer.ViewModels
{
    public class TagEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? SaveCompleted;
        public event EventHandler<string>? SaveFailed;
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
            catch (IOException)
            {
                SaveFailed?.Invoke(this, "Unable to save changes. The file is currently in use.");
            }
            catch (UnauthorizedAccessException)
            {
                SaveFailed?.Invoke(this, "Unable to save changes. The file maybe be read-only or you lack write permissions.");
            }
            catch (Exception ex)
            {
                SaveFailed?.Invoke(this, $"An unexpected error occurred: { ex.Message}");
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
