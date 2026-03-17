using ScriptureTyping.ViewModels.Games.WordOrder.Models;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// SamuelRank1 난이도 전용 안내/피드백 문구를 관리한다.
    /// </summary>
    public sealed partial class WordOrderGameViewModel
    {
        /// <summary>
        /// 목적:
        /// SamuelRank1 난이도 여부를 반환한다.
        /// </summary>
        private bool IsSamuelRank1Difficulty()
        {
            return string.Equals(
                SelectedDifficulty,
                WordOrderDifficulty.SamuelRank1,
                System.StringComparison.Ordinal);
        }

        /// <summary>
        /// 목적:
        /// SamuelRank1 난이도 시작 안내 문구를 반환한다.
        /// </summary>
        private static string GetSamuelRank1GuideText()
        {
            return "사무엘 1등 단계입니다. 매우 어려움 규칙에 더해 후치사만 바뀐 방해 조각이 추가됩니다.";
        }

        /// <summary>
        /// 목적:
        /// SamuelRank1 난이도 정답 피드백 문구를 반환한다.
        /// </summary>
        private static string GetSamuelRank1CorrectText()
        {
            return "정답입니다! 후치사 방해 조각까지 정확히 구분했습니다.";
        }

        /// <summary>
        /// 목적:
        /// SamuelRank1 난이도 오답 피드백 문구를 반환한다.
        /// </summary>
        private static string GetSamuelRank1WrongText()
        {
            return "오답입니다. 비슷한 후치사 조각까지 다시 구분해 보세요.";
        }
    }
}