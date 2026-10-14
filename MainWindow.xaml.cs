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

            DataContext = musicLibrary;
        }

        private void AudioPlayer_StateChanged(object? sender, EventArgs e)
        {
            btnPlayStop.Content = audioPlayer.IsPlaying ? "⏸" : "▶";
        }

        private MusicLibrary musicLibrary = new MusicLibrary();

        private AudioPlayer audioPlayer = new AudioPlayer();
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
                spNowPlaying.DataContext = song;
            }
        }

        private void butPlayStop_Click(object sender, RoutedEventArgs e)
        {
            if (audioPlayer.IsPlaying)
                audioPlayer.Pause();
            else
                audioPlayer.Resume();
        }
    }
}