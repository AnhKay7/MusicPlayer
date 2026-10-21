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

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += UpdateProgress;

            DataContext = musicLibrary;
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
        private void UpdateProgress(object? sender, EventArgs e)
        {
            TimeSpan position = audioPlayer.GetCurrentPosition;
            if (!isDragging)
                sldProgress.Value = position.TotalSeconds;
        }

        private MusicLibrary musicLibrary = new MusicLibrary();

        private AudioPlayer audioPlayer = new AudioPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
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
        private void lvSongs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvSongs.SelectedItem is Song song)
            {
                if (!audioPlayer.Play(song.FilePath))
                {
                    MessageBox.Show("Ban nhac nay khong con ton tai!!!");
                    return;
                }
                bdPlayingBar.DataContext = song;
            }
        }

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
        private void sldProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
        }

        private void sldProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void sldProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtCurrentTime.Text = TimeSpan.FromSeconds(e.NewValue).ToString(@"m\:ss");
        }

        private void sldProgress_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            audioPlayer.SeekCurrentPosition(TimeSpan.FromSeconds(sldProgress.Value));
        }
    }
}