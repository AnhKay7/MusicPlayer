using Microsoft.Win32;
using MusicPlayer.Models;
using MusicPlayer.Services;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private MusicLibrary musicLibrary = new MusicLibrary();
        private AudioPlayer audioPlayer = new AudioPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private PlayQueue playQueue = new PlayQueue();
        private Song? currentSong;
        private double lastVolumeValue = 0.5;
        #endregion

        #region Init
        public MainWindow()
        {
            InitializeComponent();

            audioPlayer.StateChanged += AudioPlayer_StateChanged;
            audioPlayer.SongEnded += SongEndedHandle;

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += UpdateProgress;

            sldProgress.UserSeekCompleted += SeekAudioPosition;

            btnShuffle.DataContext = playQueue;
            btnRepeat.DataContext = playQueue;

            DataContext = musicLibrary;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                musicLibrary.LoadFromDatabase();

                if (musicLibrary.ScanFolders.Count == 0)
                {
                    string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    switch (musicLibrary.AddScanFolder(folderPath)) {
                        case MusicLibrary.AddFolderResult.Success:
                            MessageBox.Show($"Quet duoc nhac trong thu muc {folderPath}");
                            break;
                        default:
                            MessageBox.Show($"Khong tim thay thu muc mac dinh.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Progress Bar
        private void UpdateProgress(object? sender, EventArgs e)
        {
            TimeSpan position = audioPlayer.GetCurrentPosition;
            if (!sldProgress.IsUserDragging)
                sldProgress.Value = position.TotalSeconds;
        }
        private void SeekAudioPosition(object? sender, EventArgs e)
        {
            audioPlayer.SeekCurrentPosition(TimeSpan.FromSeconds(sldProgress.Value));
        }
        private void sldProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtCurrentTime.Text = TimeSpan.FromSeconds(e.NewValue).ToString(@"m\:ss");
        }
        #endregion

        #region Play Song
        private bool isAutoPlayEnabled = true;
        private void SongEndedHandle(object? sender, EventArgs e)
        {
            switch (playQueue.CurrentRepeatMode)
            {
                case PlayQueue.RepeatMode.Off:
                    if (!playQueue.IsListFinished)
                        PlayNextSong();
                    break;
                case PlayQueue.RepeatMode.All:
                    PlayNextSong();
                    break;
                default:
                    if (currentSong != null)
                        PlaySong(currentSong);
                    break;
            }

        }
        private void PlaySong(Song song)
        {
            if (!audioPlayer.Play(song.FilePath))
            {
                MessageBox.Show("This song no longer exists!!!");
                return;
            }
            currentSong?.UnloadCoverArt();
            song.LoadNewCoverArt();
            bdPlayingBar.DataContext = song;
            bdNowPlaying.DataContext = song;
            currentSong = song;
        }
        private void PlayNextSong()
        {
            if (currentSong == null)
                return;
            playQueue.MoveNext();
            Song? nextSong = playQueue.Current;
            if (nextSong != null)
                PlaySong(nextSong);
        }
        private void PlayPreviousSong()
        {
            if (currentSong == null)
                return;
            playQueue.MovePrevious();
            Song? previousSong = playQueue.Current;
            if (previousSong != null)
                PlaySong(previousSong);
        }
        private void lvSongs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvSongs.SelectedItem is Song song)
            {
                playQueue.Load(musicLibrary.Songs, song);
                PlaySong(song);
            }
        }
        private void btnNextSong_Click(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }
        private void btnPreviousSong_Click(object sender, RoutedEventArgs e)
        {
            PlayPreviousSong();
        }
        #endregion

        #region PlayingBar

        private void butPlayStop_Click(object sender, RoutedEventArgs e)
        {
            if (!audioPlayer.HasSong)
                return;
            if (audioPlayer.IsPlaying)
                audioPlayer.Pause();
            else
                audioPlayer.Resume();
        }
        private void AudioPlayer_StateChanged(object? sender, EventArgs e)
        {
            btnPlayStop.Content = audioPlayer.IsPlaying ? "⏸" : "▶";
            if (audioPlayer.IsPlaying)
                timer.Start();
            else
                timer.Stop();
            UpdateProgress(sender, e);
        }
        #endregion

        #region Volume
        private void ChangeVolumeIcon()
        {
            double value = sldVolume.Value;
            if (value == 0)
            {
                txtVolumeIcon.Text = "🔇";
            }
            else if (value > 0.7)
            {
                txtVolumeIcon.Text = "🔊";
            }
            else
                txtVolumeIcon.Text = "🔉";
        }
        private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            audioPlayer.Volume = e.NewValue;
            ChangeVolumeIcon();
        }
        private void txtVolumeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sldVolume.Value > 0)
            {
                lastVolumeValue = sldVolume.Value;
                sldVolume.Value = 0;
            }
            else
            {
                sldVolume.Value = lastVolumeValue;
            }
        }
        private void btnShuffle_Click(object sender, RoutedEventArgs e)
        {
            if (playQueue.IsShuffleEnable)
                playQueue.TurnOffShuffle();
            else
                playQueue.TurnOnShuffle();
        }

        private void btnRepeat_Click(object sender, RoutedEventArgs e)
        {
            playQueue.ChangeRepeatMode();
        }
        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            btnAdd.ContextMenu.PlacementTarget = btnAdd;
            btnAdd.ContextMenu.IsOpen = true;
        }

        private void menuAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Select Music Folder";
            bool? result = dialog.ShowDialog();
            if (result != true)
            {
                Debug.Print("User cancel choosing folder");
                return;
            }

            try
            {
                string chosenFolderPath = dialog.FolderName;
                switch (musicLibrary.AddScanFolder(chosenFolderPath))
                {
                    case MusicLibrary.AddFolderResult.Success:
                        MessageBox.Show("Succesfully added new music folder.");
                        break;
                    case MusicLibrary.AddFolderResult.Duplicate:
                        MessageBox.Show("This folder is already in your library.");
                        break;
                    case MusicLibrary.AddFolderResult.NotFound:
                        MessageBox.Show("Fail: This is not a valid folder.");
                        break;
                    case MusicLibrary.AddFolderResult.SubFolder:
                        MessageBox.Show("Fail: The parent folder of this folder is already in your library.");
                        break;
                    case MusicLibrary.AddFolderResult.ParentOfExisting:
                        MessageBox.Show("Fail: This folder contains another folder that is already in your library");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> errorList = musicLibrary.Refresh();
                if (errorList.Count > 0)
                {
                    string allErrorMessage = string.Join("\n", errorList);
                    MessageBox.Show(
                        $"Some error happen when refresh music library:\n{allErrorMessage}",
                        "Partial Refresh Errors",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                        );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error happen when refresh. Message: {ex.Message}", "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void menuManageScanFolder_Click(object sender, RoutedEventArgs e)
        {
            Window manageFolderWindow = new ManageFoldersWindow(musicLibrary);
            manageFolderWindow.Owner = this;
            manageFolderWindow.ShowDialog();
        }
    }
}