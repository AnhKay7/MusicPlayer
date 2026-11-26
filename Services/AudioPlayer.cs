using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace MusicPlayer.Services
{
    public class AudioPlayer
    {
        // Nghe đủ 20 giây thì tính một lượt.
        private static readonly TimeSpan ListenThreshold = TimeSpan.FromSeconds(20);

        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        private bool isPlaying;
        private bool hasSong;
        private bool mediaOpened;
        private TimeSpan listenedDuration;
        private TimeSpan lastListeningPosition;
        private bool listenCounted;

        public AudioPlayer()
        {
            // Bắt đầu theo dõi sau khi file mở thành công.
            mediaPlayer.MediaOpened += (_, _) => mediaOpened = true;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += (_, e) =>
            {
                Debug.WriteLine(e.ErrorException);
                mediaOpened = false;
                hasSong = false;
                SetPlaying(false);
            };
        }

        public event EventHandler? StateChanged;
        public event EventHandler? SongEnded;
        public event EventHandler? ListenThresholdReached;

        public bool IsPlaying => isPlaying;
        public bool HasSong => hasSong;
        public TimeSpan GetCurrentPosition => mediaPlayer.Position;

        public double Volume
        {
            get => mediaPlayer.Volume;
            set => mediaPlayer.Volume = value;
        }

        public void UpdateListeningProgress()
        {
            TrackListeningPosition(mediaPlayer.Position);
        }

        public void SeekCurrentPosition(TimeSpan position)
        {
            if (!hasSong)
                return;

            // Tính nốt đoạn đã nghe rồi lấy vị trí tua làm mốc mới.
            UpdateListeningProgress();
            mediaPlayer.Position = position;
            lastListeningPosition = position;
        }

        public bool Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            Uri musicFile = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            // Chốt lượt cũ trước khi mở bài mới, kể cả phát lại cùng file.
            UpdateListeningProgress();

            mediaOpened = false;
            ResetListen();
            double volume = mediaPlayer.Volume;
            mediaPlayer.Close();
            mediaPlayer.Open(musicFile);
            mediaPlayer.Volume = volume;
            mediaPlayer.Play();
            hasSong = true;
            SetPlaying(true);
            return true;
        }

        public void Pause()
        {
            UpdateListeningProgress();
            mediaPlayer.Pause();
            SetPlaying(false);
        }

        public void Resume()
        {
            if (!hasSong)
                return;

            // Giữ thời gian đã nghe, lấy mốc mới khi phát tiếp.
            lastListeningPosition = mediaPlayer.Position;
            mediaPlayer.Play();
            SetPlaying(true);
        }

        private void SetPlaying(bool value)
        {
            isPlaying = value;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TrackListeningPosition(TimeSpan position)
        {
            if (!mediaOpened || !isPlaying || listenCounted)
                return;

            TimeSpan advancement = position - lastListeningPosition;
            lastListeningPosition = position;

            if (advancement > TimeSpan.Zero)
                listenedDuration += advancement;

            // Mỗi lượt phát chỉ báo đủ 20 giây một lần.
            if (listenedDuration >= ListenThreshold)
            {
                listenCounted = true;
                ListenThresholdReached?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ResetListen()
        {
            listenedDuration = TimeSpan.Zero;
            lastListeningPosition = TimeSpan.Zero;
            listenCounted = false;
        }

        private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            // Tính nốt đoạn cuối để bài dài đúng 20 giây vẫn được ghi lượt.
            var duration = mediaPlayer.NaturalDuration;
            TrackListeningPosition(duration.HasTimeSpan ? duration.TimeSpan : mediaPlayer.Position);

            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Stop();
            ResetListen();
            SetPlaying(false);
            SongEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}
