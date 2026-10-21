using MusicPlayer.Models;
using MusicPlayer.Services;
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

        private bool isDragging = false;
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

            DataContext = musicLibrary;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            if (!musicLibrary.LoadFromFolder(folderPath, out int numberOfAddedSong))
            {
                MessageBox.Show("Khong tim thay thuc muc nhac!!!");
                return;
            }

            //MessageBox.Show($"Tim duoc {numberOfAddedSong} bai");
        }
        #endregion

        #region Progress Bar
        private void UpdateProgress(object? sender, EventArgs e)
        {
            TimeSpan position = audioPlayer.GetCurrentPosition;
            if (!isDragging)
                sldProgress.Value = position.TotalSeconds;
        }
        private void sldProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtCurrentTime.Text = TimeSpan.FromSeconds(e.NewValue).ToString(@"m\:ss");
        }

        private void sldProgress_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            audioPlayer.SeekCurrentPosition(TimeSpan.FromSeconds(sldProgress.Value));
        }

        private void sldProgress_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
        }
        #endregion

        #region Play Song
        private bool isAutoPlayEnabled = true;
        private void SongEndedHandle(object? sender, EventArgs e)
        {
            if (isAutoPlayEnabled)
                PlayNextSong();
        }
        private void PlaySong(Song song)
        {
            if (!audioPlayer.Play(song.FilePath))
            {
                MessageBox.Show("Ban nhac nay khong con ton tai!!!");
                return;
            }
            bdPlayingBar.DataContext = song;
            currentSong = song;
        }
        private void PlayNextSong()
        {
            if (currentSong == null)
                return;
            int nextSongIdx = (musicLibrary.Songs.IndexOf(currentSong) + 1) % musicLibrary.Songs.Count;
            PlaySong(musicLibrary.Songs[nextSongIdx]);
        }
        private void PlayPreviousSong()
        {
            if (currentSong == null)
                return;
            int previousSongIdx = (musicLibrary.Songs.IndexOf(currentSong) - 1 + musicLibrary.Songs.Count) % musicLibrary.Songs.Count;
            PlaySong(musicLibrary.Songs[previousSongIdx]);
        }
        private void lvSongs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvSongs.SelectedItem is Song song)
            {
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
            ChangeVolumeIcon();
        }
        #endregion
    }
}