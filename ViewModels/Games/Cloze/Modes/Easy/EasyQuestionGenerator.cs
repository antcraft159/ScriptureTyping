// 파일명: EasyQuestionGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 모드용 문제 생성기.
    /// 
    /// 규칙:
    /// - 단어 1개를 빈칸 처리
    /// - 보기 6개 생성
    /// </summary>
    public sealed class EasyQuestionGenerator : IClozeQuestionGenerator
    {
        private readonly IClozeChoiceGenerator _choiceGenerator;
        private readonly Random _random = new Random();

        public EasyQuestionGenerator(IClozeChoiceGenerator choiceGenerator)
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
                    ModeName = "Easy"
                };
            }

            List<string> tokens = Tokenize(sourceText);
            List<int> candidateIndexes = GetCandidateIndexes(tokens);

            if (candidateIndexes.Count == 0)
            {
                return new ClozeQuestion
                {
                    OriginalText = sourceText,
                    MaskedText = sourceText,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "Easy"
                };
            }

            int selectedIndex = candidateIndexes[_random.Next(candidateIndexes.Count)];
            string answerText = tokens[selectedIndex];

            ClozeAnswer answer = new ClozeAnswer
            {
                BlankIndex = 0,
                Text = answerText,
                TokenIndex = selectedIndex
            };

            List<string> maskedTokens = new List<string>(tokens);
            maskedTokens[selectedIndex] = "____";

            IReadOnlyList<ClozeAnswer> answers = new[] { answer };
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
                ModeName = "Easy"
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
                if (tokens[i].Length >= 2)
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