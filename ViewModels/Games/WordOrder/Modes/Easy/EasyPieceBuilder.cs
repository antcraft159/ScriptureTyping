using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 난이도에서 사용할 순서 맞추기 조각 목록을 만든다.
    ///
    /// 규칙:
    /// - 너무 잘게 쪼개지지 않도록 2~4어절 정도를 한 묶음으로 만든다.
    /// - 문장 길이가 짧으면 1어절씩 분리될 수 있다.
    /// - 쉬움 단계는 방해 조각 없이 정답 조각만 구성한다.
    /// </summary>
    public sealed class EasyPieceBuilder : IWordOrderPieceBuilder
    {
        private const int TARGET_GROUP_SIZE = 3;

        private readonly Random _random;

        public EasyPieceBuilder()
            : this(null)
        {
        }

        public EasyPieceBuilder(Random? random)
        {
            _random = random ?? new Random();
        }

        public string Difficulty => WordOrderDifficulty.Easy;

        /// <summary>
        /// 목적:
        /// Verse 본문을 쉬움 난이도용 정답 조각 문자열 목록으로 변환한다.
        /// </summary>
        public IReadOnlyList<string> BuildCorrectSequence(Verse verse)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (string.IsNullOrWhiteSpace(verse.Text))
            {
                return Array.Empty<string>();
            }

            List<string> words = SplitWords(verse.Text);

            if (words.Count == 0)
            {
                return Array.Empty<string>();
            }

            if (words.Count <= 4)
            {
                return words;
            }

            List<string> result = new List<string>();
            List<string> buffer = new List<string>();

            for (int i = 0; i < words.Count; i++)
            {
                buffer.Add(words[i]);

                bool isLast = i == words.Count - 1;
                bool shouldFlush = buffer.Count >= TARGET_GROUP_SIZE;

                if (!isLast && shouldFlush)
                {
                    int remain = words.Count - (i + 1);

                    if (remain == 1)
                    {
                        continue;
                    }
                }

                if (shouldFlush || isLast)
                {
                    result.Add(string.Join(" ", buffer));
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                result.Add(string.Join(" ", buffer));
            }

            return result;
        }

        /// <summary>
        /// 목적:
        /// 쉬움 단계는 방해 조각을 사용하지 않으므로 빈 목록을 반환한다.
        /// </summary>
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

            return Array.Empty<string>();
        }

        /// <summary>
        /// 목적:
        /// 정답 순서 목록을 실제 표시용 조각 아이템 목록으로 만든다.
        /// </summary>
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

            if (correctSequence.Count == 0)
            {
                return Array.Empty<WordOrderPieceItem>();
            }

            List<WordOrderPieceItem> pieces = correctSequence
                .Select(text => CreatePiece(text, isDistractor: false))
                .ToList();

            Shuffle(pieces);

            return pieces;
        }

        private static List<string> SplitWords(string text)
        {
            return text
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeToken)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private static string NormalizeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            return token.Trim();
        }

        private static WordOrderPieceItem CreatePiece(string text, bool isDistractor)
        {
            return new WordOrderPieceItem(text ?? string.Empty, isDistractor);
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}