using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// VeryHard 단계에서 정답처럼 보이지만 원문은 아닌
    /// 유사 표현 함정 조각을 생성한다.
    ///
    /// 역할:
    /// - 단일 조각 기준 유사 표현 후보 반환
    /// - 현재 조각과 동일한 텍스트는 제외
    /// </summary>
    public sealed class VeryHardSimilarPhraseGenerator
    {
        private static readonly Dictionary<string, string[]> SimilarPhraseMap = new(StringComparer.Ordinal)
        {
            { "마음을", new[] { "생각을", "중심을" } },
            { "생각을", new[] { "마음을", "뜻을" } },
            { "뜻을", new[] { "생각을", "마음을" } },

            { "우리를", new[] { "너희를", "그들을" } },
            { "너희를", new[] { "우리를", "그들을" } },
            { "그들을", new[] { "우리를", "너희를" } },

            { "은혜를", new[] { "구원을", "복을" } },
            { "구원을", new[] { "은혜를", "복을" } },
            { "영광을", new[] { "기쁨을", "복을" } },
            { "복을", new[] { "은혜를", "영광을" } },

            { "세상을", new[] { "만물을", "너희를" } },
            { "만물을", new[] { "세상을", "피조물을" } },
            { "피조물을", new[] { "만물을", "세상을" } },

            { "더욱", new[] { "항상", "진실로" } },
            { "항상", new[] { "더욱", "날마다" } },
            { "정녕", new[] { "진실로", "참으로" } },
            { "진실로", new[] { "정녕", "참으로" } },
            { "참으로", new[] { "진실로", "정녕" } },

            { "지키라", new[] { "보존하라", "지켜라" } },
            { "기억하라", new[] { "생각하라", "잊지말라" } },
            { "구하라", new[] { "찾으라", "바라라" } },
            { "믿으라", new[] { "의지하라", "바라보라" } },
            { "행하라", new[] { "지키라", "따르라" } },

            { "사랑하사", new[] { "사랑하여", "아끼사" } },
            { "택하사", new[] { "부르사", "세우사" } },
            { "부르사", new[] { "택하사", "세우사" } },
            { "세우사", new[] { "택하사", "부르사" } },

            { "믿음으로", new[] { "은혜로", "소망으로" } },
            { "은혜로", new[] { "믿음으로", "소망으로" } },
            { "소망으로", new[] { "믿음으로", "은혜로" } },

            { "안에", new[] { "가운데", "위에" } },
            { "가운데", new[] { "안에", "중에" } },
            { "중에", new[] { "가운데", "안에" } },

            { "생명을", new[] { "구원을", "은혜를" } },
            { "길을", new[] { "진리를", "생명을" } },
            { "진리를", new[] { "길을", "생명을" } },
            { "평안을", new[] { "기쁨을", "은혜를" } },
            { "기쁨을", new[] { "평안을", "영광을" } }
        };

        /// <summary>
        /// 목적:
        /// 단일 조각의 유사 표현 후보를 반환한다.
        /// </summary>
        public IReadOnlyList<string> CreateCandidates(string piece)
        {
            if (string.IsNullOrWhiteSpace(piece))
            {
                return Array.Empty<string>();
            }

            string normalizedPiece = piece.Trim();

            if (!SimilarPhraseMap.TryGetValue(normalizedPiece, out string[]? candidates))
            {
                return Array.Empty<string>();
            }

            List<string> result = new();

            foreach (string candidate in candidates)
            {
                string normalizedCandidate = candidate?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(normalizedCandidate))
                {
                    continue;
                }

                if (string.Equals(normalizedPiece, normalizedCandidate, StringComparison.Ordinal))
                {
                    continue;
                }

                result.Add(normalizedCandidate);
            }

            return result;
        }
    }
}