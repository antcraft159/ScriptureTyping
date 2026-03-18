using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;
using ScriptureTyping.ViewModels.Games.VerseMatch.Modes.Easy;
using ScriptureTyping.ViewModels.Games.VerseMatch.Modes.Hard;
using ScriptureTyping.ViewModels.Games.VerseMatch.Modes.Normal;
using ScriptureTyping.ViewModels.Games.VerseMatch.Modes.SamuelRank1;
using ScriptureTyping.ViewModels.Games.VerseMatch.Modes.VeryHard;
using System;

namespace ScriptureTyping.ViewModels.Games.VerseMatch
{
    /// <summary>
    /// 목적:
    /// 선택된 난이도 문자열에 맞는 VerseMatch 모드를 반환한다.
    /// </summary>
    public sealed class VerseMatchModeFactory
    {
        /// <summary>
        /// 목적:
        /// 난이도 문자열에 따라 적절한 모드를 생성한다.
        /// </summary>
        /// <param name="difficulty">선택된 난이도 문자열</param>
        /// <returns>난이도 정책 객체</returns>
        public IVerseMatchMode Create(string? difficulty)
        {
            if (string.Equals(difficulty, VerseMatchDifficulty.Easy, StringComparison.Ordinal))
            {
                return new EasyVerseMatchMode();
            }

            if (string.Equals(difficulty, VerseMatchDifficulty.Hard, StringComparison.Ordinal))
            {
                return new HardVerseMatchMode();
            }

            if (string.Equals(difficulty, VerseMatchDifficulty.VeryHard, StringComparison.Ordinal))
            {
                return new VeryHardVerseMatchMode();
            }

            if (string.Equals(difficulty, VerseMatchDifficulty.SamuelRank1, StringComparison.Ordinal))
            {
                return new SamuelRank1VerseMatchMode();
            }

            return new NormalVerseMatchMode();
        }
    }
}