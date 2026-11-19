using MusicPlayer.Models;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TagLib;

namespace MusicPlayer.ViewModels
{
    public class TagEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? SaveCompleted;
        public event EventHandler<string>? SaveFailed;
        public ICommand SaveCommand { get; }
        public ICommand RemoveCoverCommand { get; }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Song originalSong;
        private MusicLibrary musicLibrary;

        private struct EditAbleTag
        {
            public string title;
            public string album;
            public string artists;
            public string year;
            public string genre;
            public byte[]? coverData;
        }

        private EditAbleTag currentEditTag;
        private EditAbleTag originalEditTag;

        public TagEditorViewModel(Song song, MusicLibrary musicLibrary)
        {
            using var tagFile = TagLib.File.Create(song.FilePath);

            originalSong = song;

            this.musicLibrary = musicLibrary;

            SaveCommand = new RelayCommand(Save, CanSave);

            RemoveCoverCommand = new RelayCommand(RemoveCoverArt, CanRemoveCoverArt);

            Title = tagFile.Tag.Title ?? "";

            Artists = tagFile.Tag.JoinedPerformers ?? "";

            Album = tagFile.Tag.Album ?? "";

            Year = tagFile.Tag.Year == 0 ? "" : tagFile.Tag.Year.ToString();

            Genre = tagFile.Tag.JoinedGenres ?? "";

            try
            {
                IPicture[] pictures = tagFile.Tag.Pictures;
                IPicture? coverArt =
                    pictures.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover) ??
                    pictures.FirstOrDefault();
                if (coverArt == null)
                    CoverData = null;
                else
                    CoverData = coverArt.Data.Data;
            }
            catch
            {
                CoverData = null;
            }

            originalEditTag = currentEditTag;
        }

        private void Save()
        {
            try
            {
                musicLibrary.WriteSongTags(
                        originalSong,
                        Title, Artists, Album,
                        uint.TryParse(Year, out uint parseYear) ? parseYear : 0,
                        Genre,
                        CoverData,
                        !AreByteArrayEqual(currentEditTag.coverData, originalEditTag.coverData)
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
                SaveFailed?.Invoke(this, $"An unexpected error occurred: {ex.Message}");
            }
        }
        private static bool AreByteArrayEqual(byte[]? a, byte[]? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

            return a.AsSpan().SequenceEqual(b);
        }
        private bool HasChange(in EditAbleTag target, in EditAbleTag org)
        {
            return target.title != org.title
                || target.artists != org.artists
                || target.album != org.album
                || target.year != org.year
                || target.genre != org.genre
                || !AreByteArrayEqual(target.coverData, org.coverData);
        }
        private bool CanSave()
        {
            if (Year != "" && !uint.TryParse(Year, out _))
                return false;

            return HasChange(currentEditTag, originalEditTag);
        }
        private void RemoveCoverArt() => CoverData = null;
        private bool CanRemoveCoverArt()
        {
            return CoverData != null;
        }
        public string Title
        {
            get => currentEditTag.title;
            set
            {
                currentEditTag.title = value;
                OnPropertyChanged();
            }
        }
        public string Album
        {
            get => currentEditTag.album;
            set
            {
                currentEditTag.album = value;
                OnPropertyChanged();
            }
        }
        public string Artists
        {
            get => currentEditTag.artists;
            set
            {
                currentEditTag.artists = value;
                OnPropertyChanged();
            }
        }
        public string Year
        {
            get => currentEditTag.year;
            set
            {
                currentEditTag.year = value;
                OnPropertyChanged();
            }
        }
        public string Genre
        {
            get => currentEditTag.genre;
            set
            {
                currentEditTag.genre = value;
                OnPropertyChanged();
            }
        }
        public byte[]? CoverData
        {
            get => currentEditTag.coverData;
            set
            {
                currentEditTag.coverData = value;
                OnPropertyChanged();
            }
        }
    }
}
