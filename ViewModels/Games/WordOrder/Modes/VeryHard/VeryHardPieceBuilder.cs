using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 단계의 조각 목록을 생성한다.
    ///
    /// 규칙:
    /// - 기본은 공백 단위 어절 분리
    /// - 너무 짧은 조각은 가능한 한 앞뒤와 묶어서 난이도를 높인다
    /// - 정답 조각 외에 방해 조각을 추가한다
    /// </summary>
    public sealed class VeryHardPieceBuilder : IWordOrderPieceBuilder
    {
        private const int MIN_DISTRACTOR_COUNT = 2;
        private const int MAX_DISTRACTOR_COUNT = 4;

        public string Difficulty => WordOrderDifficulty.VeryHard;

        public IReadOnlyList<string> BuildCorrectSequence(Verse verse)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            List<string> tokens = SplitWords(verse.Text);

            if (tokens.Count <= 1)
            {
                return tokens;
            }

            return MergeShortTokens(tokens);
        }

        public IReadOnlyList<WordOrderPieceItem> BuildPieces(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IReadOnlyList<string> correctSequence)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            if (correctSequence is null)
            {
                throw new ArgumentNullException(nameof(correctSequence));
            }

            List<WordOrderPieceItem> pieces = correctSequence
                .Select(text => new WordOrderPieceItem(text, isDistractor: false))
                .ToList();

            IReadOnlyList<string> distractors = BuildDistractorTexts(verse, sourceVerses);

            foreach (string distractor in distractors)
            {
                if (correctSequence.Contains(distractor, StringComparer.Ordinal))
                {
                    continue;
                }

                pieces.Add(new WordOrderPieceItem(distractor, isDistractor: true));
            }

            return Shuffle(pieces);
        }

        public IReadOnlyList<string> BuildDistractorTexts(
            Verse currentVerse,
            IReadOnlyList<Verse> distractorSourceVerses)
        {
            if (currentVerse is null)
            {
                throw new ArgumentNullException(nameof(currentVerse));
            }

            if (distractorSourceVerses is null)
            {
                throw new ArgumentNullException(nameof(distractorSourceVerses));
            }

            HashSet<string> correctSet = new(
                BuildCorrectSequence(currentVerse),
                StringComparer.Ordinal);

            List<string> pool = new();

            foreach (Verse verse in distractorSourceVerses)
            {
                if (verse is null)
                {
                    continue;
                }

                if (string.Equals(verse.Ref, currentVerse.Ref, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (string token in SplitWords(verse.Text))
                {
                    string trimmed = token.Trim();

                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        continue;
                    }

                    if (correctSet.Contains(trimmed))
                    {
                        continue;
                    }

                    if (trimmed.Length <= 1)
                    {
                        continue;
                    }

                    pool.Add(trimmed);
                }
            }

            List<string> distinctPool = pool
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (distinctPool.Count == 0)
            {
                return new List<string>();
            }

            Random random = new();
            int takeCount = Math.Min(
                MAX_DISTRACTOR_COUNT,
                Math.Max(MIN_DISTRACTOR_COUNT, correctSet.Count / 3));

            takeCount = Math.Min(takeCount, distinctPool.Count);

            return distinctPool
                .OrderBy(_ => random.Next())
                .Take(takeCount)
                .ToList();
        }

        private static List<string> SplitWords(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            return text
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private static List<string> MergeShortTokens(IReadOnlyList<string> tokens)
        {
            List<string> result = new();
            int index = 0;

            while (index < tokens.Count)
            {
                string current = tokens[index];

                bool shouldMerge =
                    current.Length <= 1 ||
                    IsOnlyPunctuation(current);

                if (shouldMerge && index + 1 < tokens.Count)
                {
                    result.Add($"{current} {tokens[index + 1]}");
                    index += 2;
                    continue;
                }

                result.Add(current);
                index++;
            }

            return result;
        }

        private static bool IsOnlyPunctuation(string text)
        {
            return text.All(char.IsPunctuation);
        }

        private static List<WordOrderPieceItem> Shuffle(List<WordOrderPieceItem> items)
        {
            Random random = new();

            return items
                .OrderBy(_ => random.Next())
                .ToList();
        }
    }
}