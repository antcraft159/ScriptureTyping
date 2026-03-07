// 파일명: VeryHard/InputQuestionGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 모드용 문제 생성기.
    /// 
    /// 특징:
    /// - 빈칸 2개 생성
    /// - 주관식 입력 중심
    /// - 상대적으로 긴 단어 우선 선택
    /// </summary>
    public sealed class InputQuestionGenerator : IClozeQuestionGenerator
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
                    ModeName = "VeryHard"
                };
            }

            List<string> tokens = Tokenize(sourceText);
            List<int> candidates = GetCandidates(tokens);

            if (candidates.Count < 2)
            {
                return new ClozeQuestion
                {
                    OriginalText = sourceText,
                    MaskedText = sourceText,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "VeryHard"
                };
            }

            List<int> selected = candidates
                .OrderByDescending(i => tokens[i].Length)
                .ThenBy(_ => _random.Next())
                .Take(2)
                .OrderBy(i => i)
                .ToList();

            List<ClozeAnswer> answers = new List<ClozeAnswer>();

            for (int i = 0; i < selected.Count; i++)
            {
                int tokenIndex = selected[i];

                answers.Add(new ClozeAnswer
                {
                    BlankIndex = i,
                    Text = tokens[tokenIndex],
                    TokenIndex = tokenIndex
                });
            }

            List<string> maskedTokens = new List<string>(tokens);

            foreach (int tokenIndex in selected)
            {
                maskedTokens[tokenIndex] = "____";
            }

            return new ClozeQuestion
            {
                OriginalText = sourceText,
                MaskedText = string.Join(" ", maskedTokens),
                Answers = answers,
                OptionSets = Array.Empty<ClozeOptionSet>(),
                ModeName = "VeryHard"
            };
        }

        private List<string> Tokenize(string text)
        {
            return text
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private List<int> GetCandidates(IReadOnlyList<string> tokens)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Length >= 3)
                {
                    result.Add(i);
                }
            }

            return result;
        }
    }
}