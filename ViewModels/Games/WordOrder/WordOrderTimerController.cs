using System;
using System.Windows.Threading;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임에서 사용하는 초 단위 타이머를 관리한다.
    ///
    /// 주요 역할:
    /// - 남은 시간 초기화
    /// - 타이머 시작/정지
    /// - 1초마다 남은 시간 감소
    /// - 시간 종료 이벤트 발생
    ///
    /// 입력:
    /// - 제한 시간(초)
    ///
    /// 출력:
    /// - Tick 이벤트
    /// - TimeExpired 이벤트
    ///
    /// 주의사항:
    /// - UI 스레드에서 동작하는 DispatcherTimer를 사용한다.
    /// - 남은 시간이 0 이하가 되면 자동으로 정지한다.
    /// </summary>
    public sealed class WordOrderTimerController
    {
        private readonly DispatcherTimer _timer;
        private int _remainingSeconds;

        public WordOrderTimerController()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += OnTimerTick;
        }

        /// <summary>
        /// 남은 시간(초)
        /// </summary>
        public int RemainingSeconds => _remainingSeconds;

        /// <summary>
        /// 현재 타이머 동작 여부
        /// </summary>
        public bool IsRunning => _timer.IsEnabled;

        /// <summary>
        /// 1초 감소 후 호출된다.
        /// </summary>
        public event Action<int>? Tick;

        /// <summary>
        /// 시간이 모두 종료되면 호출된다.
        /// </summary>
        public event Action? TimeExpired;

        /// <summary>
        /// 목적:
        /// 제한 시간을 설정하고 남은 시간을 초기화한다.
        /// </summary>
        public void Configure(int totalSeconds)
        {
            Stop();
            _remainingSeconds = Math.Max(0, totalSeconds);
        }

        /// <summary>
        /// 목적:
        /// 현재 설정된 남은 시간으로 타이머를 시작한다.
        /// </summary>
        public void Start()
        {
            if (_remainingSeconds <= 0)
            {
                return;
            }

            if (_timer.IsEnabled)
            {
                return;
            }

            _timer.Start();
        }

        /// <summary>
        /// 목적:
        /// 타이머를 정지한다.
        /// </summary>
        public void Stop()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        /// <summary>
        /// 목적:
        /// 남은 시간을 새 값으로 바꾸고 타이머를 정지한다.
        /// </summary>
        public void Reset(int totalSeconds)
        {
            Stop();
            _remainingSeconds = Math.Max(0, totalSeconds);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (_remainingSeconds <= 0)
            {
                Stop();
                TimeExpired?.Invoke();
                return;
            }

            _remainingSeconds--;

            Tick?.Invoke(_remainingSeconds);

            if (_remainingSeconds <= 0)
            {
                Stop();
                TimeExpired?.Invoke();
            }
        }
    }
}