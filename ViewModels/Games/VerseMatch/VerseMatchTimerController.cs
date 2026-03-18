using System;
using System.Windows.Threading;

namespace ScriptureTyping.ViewModels.Games.VerseMatch
{
    /// <summary>
    /// 목적:
    /// 구절 짝 맞추기 게임의 초 단위 타이머를 제어한다.
    /// </summary>
    public sealed class VerseMatchTimerController
    {
        private readonly DispatcherTimer _timer;
        private int _remainingSeconds;

        /// <summary>
        /// 목적:
        /// 1초가 감소할 때마다 남은 시간을 전달한다.
        /// </summary>
        public event Action<int>? SecondElapsed;

        /// <summary>
        /// 목적:
        /// 남은 시간이 0초가 되었을 때 알린다.
        /// </summary>
        public event Action? Expired;

        public VerseMatchTimerController()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += OnTimerTick;
        }

        /// <summary>
        /// 목적:
        /// 지정된 초부터 타이머를 시작한다.
        /// </summary>
        /// <param name="seconds">시작 초</param>
        public void Start(int seconds)
        {
            Stop();

            _remainingSeconds = Math.Max(0, seconds);

            if (_remainingSeconds <= 0)
            {
                return;
            }

            _timer.Start();
        }

        /// <summary>
        /// 목적:
        /// 현재 타이머를 중지한다.
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>
        /// 목적:
        /// 1초마다 남은 시간을 감소시키고 만료 여부를 판단한다.
        /// </summary>
        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (_remainingSeconds > 0)
            {
                _remainingSeconds--;
                SecondElapsed?.Invoke(_remainingSeconds);
            }

            if (_remainingSeconds > 0)
            {
                return;
            }

            Stop();
            Expired?.Invoke();
        }
    }
}