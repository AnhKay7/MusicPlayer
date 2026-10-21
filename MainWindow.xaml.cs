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
        #region Event Handle
        private void AudioPlayer_StateChanged(object? sender, EventArgs e)
        {
            btnPlayStop.Content = audioPlayer.IsPlaying ? "⏸" : "▶";
            if (audioPlayer.IsPlaying)
                timer.Start();
            else
                timer.Stop();
            UpdateProgress(sender, e);
        }
        private void UpdateProgress(object? sender, EventArgs e)
        {
            TimeSpan position = audioPlayer.GetCurrentPosition;
            if (!isDragging)
                sldProgress.Value = position.TotalSeconds;
        }
        #endregion

        #region Component & Object
        private MusicLibrary musicLibrary = new MusicLibrary();
        private AudioPlayer audioPlayer = new AudioPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        #endregion

        #region Runtime Variable
        private Song? currentSong;
        #endregion

        #region Play Song
        private bool isAutoPlayEnabled = true;
        private void SongEndedHandle(object sender, EventArgs e)
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
        private bool isDragging = false;

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
    }
}