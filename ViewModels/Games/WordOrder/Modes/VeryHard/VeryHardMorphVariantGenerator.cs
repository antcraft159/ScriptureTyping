using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// VeryHard 단계에서 원문 기반 미세 변형 함정 조각을 생성한다.
    ///
    /// 역할:
    /// - 정확 일치 사전 기반 변형
    /// - 긴 어미/조사 변형
    /// - 짧은 조사/격조사 변형
    /// - 변형 금지 조각 예외 처리
    ///
    /// 출력:
    /// - 변형 가능하면 변형된 문자열
    /// - 변형 불가하면 null
    /// </summary>
    public sealed class VeryHardMorphVariantGenerator
    {
        /// <summary>
        /// 목적:
        /// 의미상/문체상 변형하면 어색해지거나 사용자 경험을 해치는 조각을 관리한다.
        ///
        /// 주의:
        /// - 아래 조각은 어떤 변형도 적용하지 않는다.
        /// - 특히 "이러므로", "그러므로"는 끝 글자가 "로"라서
        ///   단순 조사 변형 로직에 걸리면 부자연스러운 결과가 나올 수 있으므로 차단한다.
        /// </summary>
        private static readonly HashSet<string> NoVariantPieces = new(StringComparer.Ordinal)
        {
            "이러므로",
            "그러므로"
        };

        private static readonly Dictionary<string, string> ExactMorphVariantMap = new(StringComparer.Ordinal)
        {
            { "하나님이", "하나님은" },
            { "하나님은", "하나님이" },
            { "주께서", "주님이" },
            { "주님이", "주께서" },
            { "예수께서", "예수님이" },
            { "예수님이", "예수께서" },
            { "성령이", "성령은" },
            { "성령은", "성령이" },
            { "그가", "그는" },
            { "그는", "그가" },
            { "그들이", "그들은" },
            { "그들은", "그들이" },
            { "너희가", "너희는" },
            { "너희는", "너희가" },
            { "우리가", "우리는" },
            { "우리는", "우리가" },

            { "너희를", "우리를" },
            { "우리를", "너희를" },
            { "주를", "하나님을" },
            { "하나님을", "주를" },
            { "그에게", "그에게서" },
            { "그에게서", "그에게" },

            { "안에", "가운데" },
            { "가운데", "안에" },
            { "중에", "가운데" },
            { "안에서", "가운데서" },
            { "가운데서", "안에서" },
            { "위에", "아래에" },
            { "아래에", "위에" },
            { "앞에", "뒤에" },
            { "뒤에", "앞에" },
            { "곁에", "가까이에" },
            { "가까이에", "곁에" },

            { "그런즉", "그러므로" },
            { "이에", "그러므로" },
            { "또한", "또" },
            { "또", "또한" },
            { "정녕", "진실로" },
            { "진실로", "정녕" },
            { "반드시", "정녕" },
            { "참으로", "진실로" },
            { "더욱", "항상" },
            { "항상", "더욱" },
            { "속히", "빨리" },
            { "빨리", "속히" },
            { "이미", "벌써" },
            { "벌써", "이미" },

            { "사랑하사", "사랑하여" },
            { "사랑하여", "사랑하사" },
            { "택하사", "택하여" },
            { "택하여", "택하사" },
            { "부르사", "부르시고" },
            { "부르시고", "부르사" },
            { "주시니라", "주셨느니라" },
            { "주셨느니라", "주시니라" },
            { "말하되", "이르되" },
            { "이르되", "말하되" },
            { "보이시니", "나타내시니" },
            { "나타내시니", "보이시니" },
            { "거하시며", "함께하시며" },
            { "함께하시며", "거하시며" },
            { "지키시며", "보호하시며" },
            { "보호하시며", "지키시며" },
            { "살리시고", "구원하시고" },
            { "구원하시고", "살리시고" },

            { "지키라", "지켜라" },
            { "지켜라", "지키라" },
            { "기억하라", "생각하라" },
            { "생각하라", "기억하라" },
            { "구하라", "찾으라" },
            { "찾으라", "구하라" },
            { "믿으라", "의지하라" },
            { "의지하라", "믿으라" },
            { "행하라", "지키라" },

            { "니라", "느니라" },
            { "느니라", "니라" },
            { "이니라", "이로다" },
            { "이로다", "이니라" },
            { "하였느니라", "하였도다" },
            { "하였도다", "하였느니라" },
            { "이었느니라", "이었도다" },
            { "이었도다", "이었느니라" },

            { "으로", "로" },
            { "로", "으로" },
            { "에게", "에게서" },
            { "에게서", "에게" },
            { "에서", "에" },
            { "에", "에서" },
            { "부터", "까지" },
            { "까지", "부터" },
            { "와", "과" },
            { "과", "와" },
            { "은", "는" },
            { "는", "은" },
            { "이", "가" },
            { "가", "이" },
            { "을", "를" },
            { "를", "을" },
            { "도", "만" },
            { "만", "도" }
        };

        /// <summary>
        /// 목적:
        /// 단일 조각에서 미세 변형 함정 조각을 생성한다.
        /// </summary>
        public string? CreateVariant(string piece)
        {
            if (string.IsNullOrWhiteSpace(piece))
            {
                return null;
            }

            string normalizedPiece = piece.Trim();

            if (!IsHangulWord(normalizedPiece))
            {
                return null;
            }

            if (NoVariantPieces.Contains(normalizedPiece))
            {
                return null;
            }

            if (ExactMorphVariantMap.TryGetValue(normalizedPiece, out string exactVariant))
            {
                return exactVariant;
            }

            string? longFormVariant = CreateLongFormVariant(normalizedPiece);

            if (!string.IsNullOrWhiteSpace(longFormVariant))
            {
                return longFormVariant;
            }

            string? particleVariant = CreateParticleVariant(normalizedPiece);

            if (!string.IsNullOrWhiteSpace(particleVariant))
            {
                return particleVariant;
            }

            return null;
        }

        /// <summary>
        /// 목적:
        /// 긴 어미/조사/문체 변형을 우선 적용한다.
        /// </summary>
        private static string? CreateLongFormVariant(string piece)
        {
            if (piece.Length < 2)
            {
                return null;
            }

            if (piece.EndsWith("에게서", StringComparison.Ordinal) && piece.Length > 3)
            {
                return piece[..^3] + "에게";
            }

            if (piece.EndsWith("에게", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "에게서";
            }

            if (piece.EndsWith("안에서", StringComparison.Ordinal) && piece.Length > 3)
            {
                return piece[..^3] + "가운데서";
            }

            if (piece.EndsWith("가운데서", StringComparison.Ordinal) && piece.Length > 4)
            {
                return piece[..^4] + "안에서";
            }

            if (piece.EndsWith("가운데", StringComparison.Ordinal) && piece.Length > 3)
            {
                return piece[..^3] + "안에";
            }

            if (piece.EndsWith("안에", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "가운데";
            }

            if (piece.EndsWith("중에", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "가운데";
            }

            if (piece.EndsWith("부터", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "까지";
            }

            if (piece.EndsWith("까지", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "부터";
            }

            if (piece.EndsWith("하여", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "하사";
            }

            if (piece.EndsWith("하사", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "하여";
            }

            if (piece.EndsWith("하니", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "하매";
            }

            if (piece.EndsWith("하매", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "하니";
            }

            if (piece.EndsWith("시니라", StringComparison.Ordinal) && piece.Length > 3)
            {
                return piece[..^3] + "셨느니라";
            }

            if (piece.EndsWith("셨느니라", StringComparison.Ordinal) && piece.Length > 4)
            {
                return piece[..^4] + "시니라";
            }

            if (piece.EndsWith("느니라", StringComparison.Ordinal) && piece.Length > 3)
            {
                return piece[..^3] + "니라";
            }

            if (piece.EndsWith("니라", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "느니라";
            }

            if (piece.EndsWith("도다", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "느니라";
            }

            if (piece.EndsWith("로다", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "니라";
            }

            return null;
        }

        /// <summary>
        /// 목적:
        /// 짧은 조사/격조사/보조사 변형을 적용한다.
        /// </summary>
        private static string? CreateParticleVariant(string piece)
        {
            if (piece.Length < 2)
            {
                return null;
            }

            if (piece.EndsWith("은", StringComparison.Ordinal))
            {
                return piece[..^1] + "는";
            }

            if (piece.EndsWith("는", StringComparison.Ordinal))
            {
                return piece[..^1] + "은";
            }

            if (piece.EndsWith("이", StringComparison.Ordinal))
            {
                return piece[..^1] + "가";
            }

            if (piece.EndsWith("가", StringComparison.Ordinal))
            {
                return piece[..^1] + "이";
            }

            if (piece.EndsWith("을", StringComparison.Ordinal))
            {
                return piece[..^1] + "를";
            }

            if (piece.EndsWith("를", StringComparison.Ordinal))
            {
                return piece[..^1] + "을";
            }

            if (piece.EndsWith("와", StringComparison.Ordinal))
            {
                return piece[..^1] + "과";
            }

            if (piece.EndsWith("과", StringComparison.Ordinal))
            {
                return piece[..^1] + "와";
            }

            if (piece.EndsWith("도", StringComparison.Ordinal))
            {
                return piece[..^1] + "만";
            }

            if (piece.EndsWith("만", StringComparison.Ordinal))
            {
                return piece[..^1] + "도";
            }

            if (piece.EndsWith("에", StringComparison.Ordinal))
            {
                return piece[..^1] + "에서";
            }

            if (piece.EndsWith("에서", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "에";
            }

            if (piece.EndsWith("로", StringComparison.Ordinal))
            {
                return piece[..^1] + "으로";
            }

            if (piece.EndsWith("으로", StringComparison.Ordinal) && piece.Length > 2)
            {
                return piece[..^2] + "로";
            }

            return null;
        }

        /// <summary>
        /// 목적:
        /// 한글 단어 조각인지 검사한다.
        /// </summary>
        private static bool IsHangulWord(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            foreach (char ch in text)
            {
                if (ch < '가' || ch > '힣')
                {
                    return false;
                }
            }

            return true;
        }
    }
}