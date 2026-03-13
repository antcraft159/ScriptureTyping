using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// 사무엘 1등 모드용 문제 생성기.
    ///
    /// 의도:
    /// - 빈칸 문제를 만들지 않는다.
    /// - 말씀 원문 전체를 보관한다.
    /// - 화면에서는 권/장/절만 보여주고,
    ///   사용자는 말씀 전체를 직접 입력한다.
    /// </summary>
    public sealed class ChainQuestionGenerator : IClozeQuestionGenerator
    {
        public ClozeQuestion Generate(
            string sourceText,
            int blankCount,
            IReadOnlyList<string> wordPool)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return new ClozeQuestion
                {
                    OriginalText = string.Empty,
                    MaskedText = string.Empty,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "SamuelRank1"
                };
            }

            return new ClozeQuestion
            {
                OriginalText = sourceText,
                MaskedText = string.Empty,
                Answers = Array.Empty<ClozeAnswer>(),
                OptionSets = Array.Empty<ClozeOptionSet>(),
                ModeName = "SamuelRank1"
            };
        }
    }
}