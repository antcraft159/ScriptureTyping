using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 단계에서 사용할 정답 조각과 보기 조각 목록을 만든다.
    ///
    /// 규칙:
    /// - 기본은 공백 기준 어절 분리
    /// - 조각 수가 너무 적으면 긴 어절 1개를 추가 분리해서 최소 난이도 확보
    /// - 방해 조각은 다른 구절에서 1개 가져온다
    /// - 마지막에 보기 순서를 섞는다
    /// </summary>
    public sealed class NormalPieceBuilder : IWordOrderPieceBuilder
    {
        private const int DISTRACTOR_COUNT = 1;

        private readonly Random _random;

        public NormalPieceBuilder()
            : this(null)
        {
        }

        public NormalPieceBuilder(Random? random)
        {
            _random = random ?? new Random();
        }

        public string Difficulty => WordOrderDifficulty.Normal;

        public IReadOnlyList<string> BuildCorrectSequence(Verse verse)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            List<string> words = SplitWords(verse.Text);

            if (words.Count >= 4)
            {
                return words;
            }

            if (words.Count == 3)
            {
                int splitIndex = FindSplittableWordIndex(words);
                if (splitIndex >= 0)
                {
                    List<string> expanded = new List<string>(words);
                    string target = expanded[splitIndex];

                    (string left, string right) = SplitWord(target);

                    expanded.RemoveAt(splitIndex);

                    if (!string.IsNullOrWhiteSpace(left))
                    {
                        expanded.Insert(splitIndex, left);
                        splitIndex++;
                    }

                    if (!string.IsNullOrWhiteSpace(right))
                    {
                        expanded.Insert(splitIndex, right);
                    }

                    return expanded
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                }
            }

            return words;
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

            List<WordOrderPieceItem> pieces = new List<WordOrderPieceItem>();

            foreach (string text in correctSequence)
            {
                pieces.Add(CreatePiece(text, isDistractor: false));
            }

            IReadOnlyList<string> distractorTexts = BuildDistractorTexts(verse, sourceVerses);

            foreach (string distractorText in distractorTexts)
            {
                pieces.Add(CreatePiece(distractorText, isDistractor: true));
            }

            Shuffle(pieces);

            return pieces;
        }

        public IReadOnlyList<string> BuildDistractorTexts(Verse verse, IReadOnlyList<Verse> sourceVerses)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            HashSet<string> answerSet = new HashSet<string>(
                BuildCorrectSequence(verse),
                StringComparer.Ordinal);

            List<string> candidates = new List<string>();

            foreach (Verse sourceVerse in sourceVerses)
            {
                if (sourceVerse is null)
                {
                    continue;
                }

                if (ReferenceEquals(sourceVerse, verse))
                {
                    continue;
                }

                foreach (string word in SplitWords(sourceVerse.Text))
                {
                    if (string.IsNullOrWhiteSpace(word))
                    {
                        continue;
                    }

                    if (answerSet.Contains(word))
                    {
                        continue;
                    }

                    candidates.Add(word);
                }
            }

            if (candidates.Count == 0)
            {
                return Array.Empty<string>();
            }

            Shuffle(candidates);

            return candidates
                .Distinct(StringComparer.Ordinal)
                .Take(DISTRACTOR_COUNT)
                .ToList();
        }

        private static WordOrderPieceItem CreatePiece(string text, bool isDistractor)
        {
            return new WordOrderPieceItem(text ?? string.Empty, isDistractor);
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

        private static int FindSplittableWordIndex(IReadOnlyList<string> words)
        {
            int bestIndex = -1;
            int bestLength = 0;

            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];

                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                if (word.Length < 4)
                {
                    continue;
                }

                if (word.Length > bestLength)
                {
                    bestLength = word.Length;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static (string left, string right) SplitWord(string word)
        {
            int mid = word.Length / 2;

            if (mid <= 0 || mid >= word.Length)
            {
                return (word, string.Empty);
            }

            string left = word.Substring(0, mid);
            string right = word.Substring(mid);

            return (left, right);
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);

                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}