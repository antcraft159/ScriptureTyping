using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// VeryHard 단계에서 인접 조각 순서 혼동용 후보를 계획한다.
    ///
    /// 역할:
    /// - 현재 정답 조각 목록에서 서로 바꿔도 어색함이 상대적으로 적은
    ///   인접 조각쌍을 찾는다.
    /// - 가상 Verse 생성에 사용할 순서 혼동 결과를 만든다.
    /// </summary>
    public sealed class VeryHardOrderConfusionPlanner
    {
        /// <summary>
        /// 목적:
        /// 인접 조각을 교체한 순서 혼동 후보 문장 목록을 반환한다.
        /// </summary>
        public IReadOnlyList<string> CreateConfusionVerseTexts(IReadOnlyList<string> correctSequence)
        {
            if (correctSequence is null)
            {
                throw new ArgumentNullException(nameof(correctSequence));
            }

            List<string> results = new();

            if (correctSequence.Count < 2)
            {
                return results;
            }

            for (int index = 0; index < correctSequence.Count - 1; index++)
            {
                string left = correctSequence[index];
                string right = correctSequence[index + 1];

                if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
                {
                    continue;
                }

                if (!CanSwapForConfusion(left, right))
                {
                    continue;
                }

                List<string> cloned = correctSequence.ToList();
                cloned[index] = right;
                cloned[index + 1] = left;

                results.Add(JoinPieces(cloned));
            }

            return results
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// 목적:
        /// 인접 조각 순서 혼동 후보로 써도 되는지 판별한다.
        /// </summary>
        public bool CanSwapForConfusion(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            string normalizedLeft = left.Trim();
            string normalizedRight = right.Trim();

            if (normalizedLeft.Length <= 1 || normalizedRight.Length <= 1)
            {
                return false;
            }

            if (normalizedLeft.Length >= 10 || normalizedRight.Length >= 10)
            {
                return false;
            }

            if (IsOnlyPunctuation(normalizedLeft) || IsOnlyPunctuation(normalizedRight))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 목적:
        /// 조각 목록을 공백 기준 문장 형태로 합친다.
        /// </summary>
        private static string JoinPieces(IReadOnlyList<string> pieces)
        {
            return string.Join(
                " ",
                pieces.Where(piece => !string.IsNullOrWhiteSpace(piece))
                      .Select(piece => piece.Trim()));
        }

        /// <summary>
        /// 목적:
        /// 문자열이 문장부호만으로 이루어졌는지 검사한다.
        /// </summary>
        private static bool IsOnlyPunctuation(string text)
        {
            return text.All(char.IsPunctuation);
        }
    }
}