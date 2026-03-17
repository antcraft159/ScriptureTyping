using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// Hard 난이도용 조각 목록을 만든다.
    ///
    /// 규칙:
    /// - 기본은 공백 기준 어절 분리
    /// - 너무 짧은 기호 조각은 제거
    /// - 다른 구절에서 방해 조각을 일부 섞는다
    /// </summary>
    public sealed class HardPieceBuilder : IWordOrderPieceBuilder
    {
        private const int DISTRACTOR_COUNT = 3;

        public string Difficulty => WordOrderDifficulty.Hard;

        public IReadOnlyList<string> BuildCorrectSequence(Verse verse)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            return SplitWords(verse.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !IsIgnorableToken(x))
                .ToList();
        }

        public IReadOnlyList<string> BuildDistractorTexts(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            IReadOnlyList<string> correctSequence = BuildCorrectSequence(verse);
            HashSet<string> correctSet = new HashSet<string>(correctSequence, StringComparer.Ordinal);

            List<string> candidates = sourceVerses
                .Where(x => x is not null)
                .Where(x => !string.Equals(x.Ref, verse.Ref, StringComparison.Ordinal))
                .SelectMany(x => SplitWords(x.Text))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !IsIgnorableToken(x))
                .Where(x => x.Length >= 2)
                .Where(x => !correctSet.Contains(x))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Random random = Random.Shared;

            return candidates
                .OrderBy(_ => random.Next())
                .Take(DISTRACTOR_COUNT)
                .ToList();
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
                .Select(text => CreatePiece(text, isDistractor: false))
                .ToList();

            IReadOnlyList<string> distractors = BuildDistractorTexts(verse, sourceVerses);

            foreach (string distractor in distractors)
            {
                pieces.Add(CreatePiece(distractor, isDistractor: true));
            }

            return Shuffle(pieces);
        }

        private static WordOrderPieceItem CreatePiece(string text, bool isDistractor)
        {
            return new WordOrderPieceItem(text ?? string.Empty, isDistractor);
        }

        private static IReadOnlyList<WordOrderPieceItem> Shuffle(List<WordOrderPieceItem> pieces)
        {
            Random random = Random.Shared;

            for (int i = pieces.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }

            return pieces;
        }

        public static IReadOnlyList<string> SplitWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<string>();
            }

            return Regex.Split(text.Trim(), @"\s+")
                .Select(NormalizeWord)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private static string NormalizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return string.Empty;
            }

            return word.Trim();
        }

        private static bool IsIgnorableToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return true;
            }

            return token.All(char.IsPunctuation);
        }
    }
}