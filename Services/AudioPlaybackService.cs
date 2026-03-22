using System;
using System.IO;
using System.Windows.Media;

namespace ScriptureTyping.Services
{
    /// <summary>
    /// 목적:
    /// 로컬 mp3 파일 재생, 정지, 탐색(Seek)을 담당한다.
    /// </summary>
    public sealed class AudioPlaybackService
    {
        private readonly MediaPlayer _mediaPlayer;
        private bool _isPlaying;

        public event EventHandler? PlaybackStateChanged;

        public bool IsPlaying => _isPlaying;

        public TimeSpan Position => _mediaPlayer.Position;

        public TimeSpan Duration
        {
            get
            {
                if (_mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    return _mediaPlayer.NaturalDuration.TimeSpan;
                }

                return TimeSpan.Zero;
            }
        }

        public AudioPlaybackService()
        {
            _mediaPlayer = new MediaPlayer();

            _mediaPlayer.MediaEnded += OnMediaEnded;
            _mediaPlayer.MediaFailed += OnMediaFailed;
        }

        public void Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException("재생할 파일 경로가 비어 있습니다.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("MP3 파일을 찾을 수 없습니다.", filePath);
            }

            _mediaPlayer.Open(new Uri(filePath, UriKind.Absolute));
            _mediaPlayer.Play();

            _isPlaying = true;
            RaisePlaybackStateChanged();
        }

        public void Stop()
        {
            _mediaPlayer.Stop();

            _isPlaying = false;
            RaisePlaybackStateChanged();
        }

        public void Seek(TimeSpan position)
        {
            if (position < TimeSpan.Zero)
            {
                position = TimeSpan.Zero;
            }

            if (_mediaPlayer.NaturalDuration.HasTimeSpan &&
                position > _mediaPlayer.NaturalDuration.TimeSpan)
            {
                position = _mediaPlayer.NaturalDuration.TimeSpan;
            }

            _mediaPlayer.Position = position;
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            _isPlaying = false;
            RaisePlaybackStateChanged();
        }

        private void OnMediaFailed(object? sender, ExceptionEventArgs e)
        {
            _isPlaying = false;
            RaisePlaybackStateChanged();
        }

        private void RaisePlaybackStateChanged()
        {
            PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}