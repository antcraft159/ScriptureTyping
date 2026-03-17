using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// SamuelRank1 난이도에서 사용할 조각 목록을 생성한다.
    ///
    /// 규칙:
    /// - 기본 정답 조각은 공백 기준 어절 단위로 생성한다.
    /// - 방해 조각은 조사/후치사만 바꾼 조각 위주로 추가한다.
    /// - 현재 구절에서 부족하면 다른 구절(sourceVerses)에서도 후보를 보충한다.
    /// - 목표는 정답 조각 외에 최대 15개의 추가 조각을 생성하는 것이다.
    /// </summary>
    public sealed class SamuelRank1PieceBuilder : IWordOrderPieceBuilder
    {
        private const int ADDITIONAL_DISTRACTOR_COUNT = 15;

        private readonly Random _random;

        private static readonly Dictionary<string, string[]> PostpositionMap = new Dictionary<string, string[]>
        {
            ["은"] = new[] { "는", "이", "가", "을", "를" },
            ["는"] = new[] { "은", "이", "가", "을", "를" },
            ["이"] = new[] { "가", "은", "는", "을", "를" },
            ["가"] = new[] { "이", "은", "는", "을", "를" },
            ["을"] = new[] { "를", "은", "는", "이", "가" },
            ["를"] = new[] { "을", "은", "는", "이", "가" },
            ["에"] = new[] { "에서", "에게", "으로", "와", "과" },
            ["에서"] = new[] { "에", "에게", "으로", "와", "과" },
            ["에게"] = new[] { "에", "에서", "으로", "와", "과" },
            ["께"] = new[] { "에게", "에", "에서" },
            ["와"] = new[] { "과", "이", "가", "은", "는" },
            ["과"] = new[] { "와", "이", "가", "은", "는" },
            ["로"] = new[] { "으로", "에", "에서" },
            ["으로"] = new[] { "로", "에", "에서" },
            ["만"] = new[] { "도", "까지", "부터" },
            ["도"] = new[] { "만", "까지", "부터" },
            ["까지"] = new[] { "부터", "만", "도" },
            ["부터"] = new[] { "까지", "만", "도" },
            ["랑"] = new[] { "와", "과" }
        };

        public SamuelRank1PieceBuilder()
            : this(null)
        {
        }

        public SamuelRank1PieceBuilder(Random? random)
        {
            _random = random ?? new Random();
        }

        /// <summary>
        /// 현재 조각 생성기가 담당하는 난이도명
        /// </summary>
        public string Difficulty => WordOrderDifficulty.SamuelRank1;

        /// <summary>
        /// 목적:
        /// 원문 텍스트를 정답 순서 기준 어절 목록으로 분리한다.
        /// </summary>
        /// <param name="verse">원본 말씀</param>
        /// <returns>정답 순서 어절 목록</returns>
        public IReadOnlyList<string> BuildCorrectSequence(Verse verse)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            string sourceText = verse.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return Array.Empty<string>();
            }

            return sourceText
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        /// <summary>
        /// 목적:
        /// SamuelRank1 난이도용 전체 보기 조각을 만든다.
        ///
        /// 출력:
        /// - 정답 조각
        /// - 조사/후치사만 바꾼 가짜 조각 최대 15개
        /// - 최종 셔플된 WordOrderPieceItem 목록
        /// </summary>
        /// <param name="verse">원본 말씀</param>
        /// <param name="sourceVerses">전체 말씀 소스</param>
        /// <param name="correctSequence">정답 순서 목록</param>
        /// <returns>표시용 조각 목록</returns>
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

            List<WordOrderPieceItem> result = new List<WordOrderPieceItem>();

            foreach (string text in correctSequence)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                result.Add(new WordOrderPieceItem(text, isDistractor: false));
            }

            IReadOnlyList<string> distractors = BuildDistractorTexts(verse, sourceVerses);

            foreach (string text in distractors)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                result.Add(new WordOrderPieceItem(text, isDistractor: true));
            }

            Shuffle(result);
            return result;
        }

        /// <summary>
        /// 목적:
        /// 정답 조각들로부터 조사/후치사만 바꾼 방해 조각 문자열 목록을 생성한다.
        /// 부족하면 다른 구절에서도 후보를 보충한다.
        /// </summary>
        /// <param name="verse">원본 말씀</param>
        /// <param name="sourceVerses">전체 말씀 소스</param>
        /// <returns>방해 조각 문자열 목록</returns>
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

            List<string> correctSequence = BuildCorrectSequence(verse).ToList();
            HashSet<string> answerSet = new HashSet<string>(correctSequence, StringComparer.Ordinal);
            List<string> result = new List<string>();

            AddDistractorsFromTokens(correctSequence, answerSet, result);

            if (result.Count < ADDITIONAL_DISTRACTOR_COUNT)
            {
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

                    List<string> sourceTokens = BuildCorrectSequence(sourceVerse).ToList();

                    AddDistractorsFromTokens(sourceTokens, answerSet, result);

                    if (result.Count >= ADDITIONAL_DISTRACTOR_COUNT)
                    {
                        break;
                    }
                }
            }

            return result
                .Take(ADDITIONAL_DISTRACTOR_COUNT)
                .ToList();
        }

        /// <summary>
        /// 목적:
        /// 토큰 목록에서 조사 변형 방해 조각을 result에 추가한다.
        /// </summary>
        private static void AddDistractorsFromTokens(
            IReadOnlyList<string> tokens,
            HashSet<string> answerSet,
            IList<string> result)
        {
            foreach (string token in tokens)
            {
                foreach (string candidate in BuildParticleVariants(token))
                {
                    if (string.IsNullOrWhiteSpace(candidate))
                    {
                        continue;
                    }

                    if (answerSet.Contains(candidate))
                    {
                        continue;
                    }

                    if (result.Contains(candidate, StringComparer.Ordinal))
                    {
                        continue;
                    }

                    result.Add(candidate);

                    if (result.Count >= ADDITIONAL_DISTRACTOR_COUNT)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 목적:
        /// 조사/후치사만 바꾼 가짜 조각 후보를 생성한다.
        /// </summary>
        private static IEnumerable<string> BuildParticleVariants(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            foreach ((string suffix, string[] replacements) in PostpositionMap
                         .OrderByDescending(x => x.Key.Length))
            {
                if (!text.EndsWith(suffix, StringComparison.Ordinal))
                {
                    continue;
                }

                string stem = text.Substring(0, text.Length - suffix.Length);

                if (string.IsNullOrWhiteSpace(stem))
                {
                    yield break;
                }

                foreach (string replacement in replacements)
                {
                    string candidate = stem + replacement;

                    if (!string.Equals(candidate, text, StringComparison.Ordinal))
                    {
                        yield return candidate;
                    }
                }

                yield break;
            }
        }

        /// <summary>
        /// 목적:
        /// 보기 조각 순서를 무작위로 섞는다.
        /// </summary>
        private void Shuffle<T>(IList<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}