// 파일명: HardQuestionGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// 어려움 모드 문제 생성기.
    /// 
    /// 특징:
    /// - 길이가 비교적 긴 단어를 우선 후보로 사용
    /// - 빈칸 2개 생성
    /// - 유사 오답 생성기와 연결
    /// </summary>
    public sealed class HardQuestionGenerator : IClozeQuestionGenerator
    {
        private readonly IClozeChoiceGenerator _choiceGenerator;
        private readonly Random _random = new Random();

        public HardQuestionGenerator(IClozeChoiceGenerator choiceGenerator)
        {
            _choiceGenerator = choiceGenerator ?? throw new ArgumentNullException(nameof(choiceGenerator));
        }

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
                    ModeName = "Hard"
                };
            }

            List<string> tokens = Tokenize(sourceText);
            List<int> candidateIndexes = GetCandidateIndexes(tokens);

            if (candidateIndexes.Count < 2)
            {
                return new ClozeQuestion
                {
                    OriginalText = sourceText,
                    MaskedText = sourceText,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "Hard"
                };
            }

            List<int> selectedIndexes = candidateIndexes
                .OrderByDescending(i => tokens[i].Length)
                .ThenBy(_ => _random.Next())
                .Take(2)
                .OrderBy(i => i)
                .ToList();

            List<ClozeAnswer> answers = new List<ClozeAnswer>();

            for (int i = 0; i < selectedIndexes.Count; i++)
            {
                int tokenIndex = selectedIndexes[i];

                answers.Add(new ClozeAnswer
                {
                    BlankIndex = i,
                    Text = tokens[tokenIndex],
                    TokenIndex = tokenIndex
                });
            }

            List<string> maskedTokens = new List<string>(tokens);

            foreach (int tokenIndex in selectedIndexes)
            {
                maskedTokens[tokenIndex] = "____";
            }

            IReadOnlyList<ClozeOptionSet> optionSets = _choiceGenerator.GenerateChoices(
                answers,
                BuildWordPool(tokens, wordPool),
                6);

            return new ClozeQuestion
            {
                OriginalText = sourceText,
                MaskedText = string.Join(" ", maskedTokens),
                Answers = answers,
                OptionSets = optionSets,
                ModeName = "Hard"
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

        private List<int> GetCandidateIndexes(IReadOnlyList<string> tokens)
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

        private IReadOnlyList<string> BuildWordPool(
            IReadOnlyList<string> tokens,
            IReadOnlyList<string> externalPool)
        {
            List<string> result = new List<string>();

            if (externalPool != null)
            {
                result.AddRange(externalPool);
            }

            result.AddRange(tokens);

            return result
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}