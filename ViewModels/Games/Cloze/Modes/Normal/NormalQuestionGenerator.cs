// 파일명: NormalQuestionGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 난이도 문제 생성기.
    /// 
    /// 규칙:
    /// - 단어 2개를 빈칸 처리
    /// - 각 빈칸에 대한 보기 세트 생성
    /// </summary>
    public sealed class NormalQuestionGenerator : IClozeQuestionGenerator
    {
        private readonly IClozeChoiceGenerator _choiceGenerator;
        private readonly Random _random = new Random();

        public NormalQuestionGenerator(IClozeChoiceGenerator choiceGenerator)
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
                    ModeName = "Normal"
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
                    ModeName = "Normal"
                };
            }

            List<int> selectedIndexes = candidateIndexes
                .OrderBy(_ => _random.Next())
                .Take(2)
                .OrderBy(x => x)
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
                ModeName = "Normal"
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