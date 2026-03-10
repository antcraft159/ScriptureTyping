// 파일명: FlashPreviewController.cs
using System;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// 플래시 미리보기(잠깐 보여주기) 상태를 관리한다.
    /// 
    /// 사용 예:
    /// - 원문을 1~2초 보여주고 숨김
    /// - 정답을 잠깐 보여주고 입력 단계로 전환
    /// </summary>
    public sealed class FlashPreviewController
    {
        /// <summary>
        /// 현재 미리보기 표시 중인지 여부
        /// </summary>
        public bool IsPreviewVisible { get; private set; }

        /// <summary>
        /// 미리보기 시작 시각
        /// </summary>
        public DateTime? PreviewStartedAt { get; private set; }

        /// <summary>
        /// 미리보기 유지 시간
        /// </summary>
        public TimeSpan PreviewDuration { get; private set; } = TimeSpan.FromSeconds(1.5);

        /// <summary>
        /// 목적:
        /// 미리보기를 시작한다.
        /// </summary>
        public void Start(TimeSpan? duration = null)
        {
            PreviewDuration = duration ?? TimeSpan.FromSeconds(1.5);
            PreviewStartedAt = DateTime.UtcNow;
            IsPreviewVisible = true;
        }

        /// <summary>
        /// 목적:
        /// 미리보기를 강제로 종료한다.
        /// </summary>
        public void Stop()
        {
            IsPreviewVisible = false;
            PreviewStartedAt = null;
        }

        /// <summary>
        /// 목적:
        /// 현재 시점 기준 미리보기가 끝났는지 확인하고 상태를 갱신한다.
        /// </summary>
        public bool Update()
        {
            if (!IsPreviewVisible || PreviewStartedAt == null)
            {
                return false;
            }

            TimeSpan elapsed = DateTime.UtcNow - PreviewStartedAt.Value;

            if (elapsed >= PreviewDuration)
            {
                Stop();
                return true;
            }

            return false;
        }
    }
}