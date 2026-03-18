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
    /// - VeryHardQuestionGenerator가 추가한 가상 Verse도 방해 조각 후보로 적극 활용한다
    /// </summary>
    public sealed class VeryHardPieceBuilder : IWordOrderPieceBuilder
    {
        private const int MIN_DISTRACTOR_COUNT = 2;
        private const int MAX_DISTRACTOR_COUNT = 4;
        private const string MORPH_TAG = "[VH-MORPH]";
        private const string SIMILAR_TAG = "[VH-SIMILAR]";
        private const string ORDER_TAG = "[VH-ORDER]";

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

            List<string> prioritizedPool = new();
            List<string> fallbackPool = new();

            foreach (Verse verse in distractorSourceVerses)
            {
                if (verse is null)
                {
                    continue;
                }

                if (IsSameRealVerse(currentVerse, verse))
                {
                    continue;
                }

                IReadOnlyList<string> candidatePieces = BuildCandidatePiecesForDistractorVerse(verse);

                foreach (string piece in candidatePieces)
                {
                    string trimmed = Normalize(piece);

                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        continue;
                    }

                    if (trimmed.Length <= 1)
                    {
                        continue;
                    }

                    if (IsOnlyPunctuation(trimmed))
                    {
                        continue;
                    }

                    if (correctSet.Contains(trimmed))
                    {
                        continue;
                    }

                    if (IsSyntheticVerse(verse))
                    {
                        prioritizedPool.Add(trimmed);
                    }
                    else
                    {
                        fallbackPool.Add(trimmed);
                    }
                }
            }

            List<string> prioritizedDistinct = prioritizedPool
                .Distinct(StringComparer.Ordinal)
                .ToList();

            List<string> fallbackDistinct = fallbackPool
                .Distinct(StringComparer.Ordinal)
                .Where(text => !prioritizedDistinct.Contains(text, StringComparer.Ordinal))
                .ToList();

            int takeCount = CalculateDistractorCount(correctSet.Count);

            if (takeCount <= 0)
            {
                return new List<string>();
            }

            List<string> selected = new();
            Random random = Random.Shared;

            foreach (string item in prioritizedDistinct.OrderBy(_ => random.Next()))
            {
                if (selected.Count >= takeCount)
                {
                    break;
                }

                selected.Add(item);
            }

            foreach (string item in fallbackDistinct.OrderBy(_ => random.Next()))
            {
                if (selected.Count >= takeCount)
                {
                    break;
                }

                selected.Add(item);
            }

            return selected;
        }

        /// <summary>
        /// 목적:
        /// 방해 조각 후보 Verse에서 사용할 조각 후보를 만든다.
        ///
        /// 규칙:
        /// - 일반 Verse는 현재 VeryHard 정답 분리 규칙과 동일하게 분리한다.
        /// - VH-ORDER는 바뀐 순서쌍을 더 잘 보존하기 위해 원문 전체 분리도 함께 사용한다.
        /// </summary>
        private IReadOnlyList<string> BuildCandidatePiecesForDistractorVerse(Verse verse)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            List<string> result = new();

            IReadOnlyList<string> mergedPieces = BuildCorrectSequence(verse);

            foreach (string piece in mergedPieces)
            {
                string normalized = Normalize(piece);

                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    result.Add(normalized);
                }
            }

            if (HasTag(verse, ORDER_TAG))
            {
                foreach (string piece in SplitWords(verse.Text))
                {
                    string normalized = Normalize(piece);

                    if (!string.IsNullOrWhiteSpace(normalized))
                    {
                        result.Add(normalized);
                    }
                }
            }

            return result
                .Distinct(StringComparer.Ordinal)
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

        private static bool IsSameRealVerse(Verse currentVerse, Verse candidateVerse)
        {
            string currentRef = Normalize(currentVerse.Ref);
            string candidateRef = Normalize(candidateVerse.Ref);

            if (string.IsNullOrWhiteSpace(currentRef) || string.IsNullOrWhiteSpace(candidateRef))
            {
                return false;
            }

            return string.Equals(currentRef, candidateRef, StringComparison.Ordinal);
        }

        private static bool IsSyntheticVerse(Verse verse)
        {
            return HasTag(verse, MORPH_TAG) ||
                   HasTag(verse, SIMILAR_TAG) ||
                   HasTag(verse, ORDER_TAG);
        }

        private static bool HasTag(Verse verse, string tag)
        {
            string reference = verse.Ref ?? string.Empty;
            return reference.Contains(tag, StringComparison.Ordinal);
        }

        private static int CalculateDistractorCount(int correctCount)
        {
            int takeCount = Math.Max(MIN_DISTRACTOR_COUNT, correctCount / 3);
            takeCount = Math.Min(MAX_DISTRACTOR_COUNT, takeCount);
            return takeCount;
        }

        private static string Normalize(string? text)
        {
            return (text ?? string.Empty).Trim();
        }

        private static bool IsOnlyPunctuation(string text)
        {
            return text.All(char.IsPunctuation);
        }

        private static List<WordOrderPieceItem> Shuffle(List<WordOrderPieceItem> items)
        {
            Random random = Random.Shared;

            return items
                .OrderBy(_ => random.Next())
                .ToList();
        }
    }
}