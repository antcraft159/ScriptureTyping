// 파일명: ChainQuestionGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// 연속형(체인형) 문제 생성기.
    /// 
    /// 의도:
    /// - 상위 난이도에서 반복 플레이 시 문제 패턴이 단조로워지지 않게 함
    /// - 주로 2개 빈칸 문제 생성에 사용
    /// </summary>
    public sealed class ChainQuestionGenerator : IClozeQuestionGenerator
    {
        private readonly Random _random = new Random();

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

            List<string> tokens = sourceText
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            List<int> candidates = new List<int>();

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Length >= 2)
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count < 2)
            {
                return new ClozeQuestion
                {
                    OriginalText = sourceText,
                    MaskedText = sourceText,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "SamuelRank1"
                };
            }

            List<int> selected = candidates
                .OrderBy(_ => _random.Next())
                .Take(Math.Max(1, blankCount))
                .OrderBy(x => x)
                .ToList();

            List<ClozeAnswer> answers = new List<ClozeAnswer>();
            List<string> maskedTokens = new List<string>(tokens);

            for (int i = 0; i < selected.Count; i++)
            {
                int tokenIndex = selected[i];

                answers.Add(new ClozeAnswer
                {
                    BlankIndex = i,
                    Text = tokens[tokenIndex],
                    TokenIndex = tokenIndex
                });

                maskedTokens[tokenIndex] = "____";
            }

            return new ClozeQuestion
            {
                OriginalText = sourceText,
                MaskedText = string.Join(" ", maskedTokens),
                Answers = answers,
                OptionSets = Array.Empty<ClozeOptionSet>(),
                ModeName = "SamuelRank1"
            };
        }
    }
}