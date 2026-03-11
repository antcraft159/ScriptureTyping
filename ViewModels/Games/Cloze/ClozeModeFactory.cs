using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Modes.Easy;
using ScriptureTyping.ViewModels.Games.Cloze.Modes.Hard;
using ScriptureTyping.ViewModels.Games.Cloze.Modes.Normal;
using ScriptureTyping.ViewModels.Games.Cloze.Modes.SamuelRank1;
using ScriptureTyping.ViewModels.Games.Cloze.Modes.VeryHard;
using System;

namespace ScriptureTyping.ViewModels.Games.Cloze
{
    /// <summary>
    /// 목적:
    /// 난이도(enum)에 맞는 빈칸 모드 객체를 생성하는 팩토리.
    /// </summary>
    public static class ClozeModeFactory
    {
        /// <summary>
        /// 목적:
        /// 지정된 난이도에 맞는 모드 객체를 생성한다.
        /// </summary>
        public static IClozeMode Create(ClozeDifficulty difficulty)
        {
            return difficulty switch
            {
                ClozeDifficulty.Easy => new EasyClozeMode(),
                ClozeDifficulty.Normal => new NormalClozeMode(),
                ClozeDifficulty.Hard => new HardClozeMode(),
                ClozeDifficulty.VeryHard => new VeryHardClozeMode(),
                ClozeDifficulty.SamuelRank1 => new SamuelRank1ClozeMode(),
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "지원하지 않는 난이도입니다.")
            };
        }
    }
}