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
    /// 매우 어려움 단계용 WordOrderQuestion을 생성한다.
    ///
    /// 규칙:
    /// - 정답 순서는 원문 기준으로 유지한다.
    /// - 보기 조각 생성 시 sourceVerses에 "함정용 가상 Verse"를 추가 전달한다.
    /// - 함정 유형은 아래 3가지를 함께 사용한다.
    ///   1. 원문 기반 미세 변형 함정
    ///   2. 인접 조각 순서 혼동 함정
    ///   3. 맞는 말 같지만 원문은 아닌 유사 표현 함정
    ///
    /// 주의사항:
    /// - 실제 정답 판정은 반드시 원문 CorrectSequence 기준이다.
    /// - PieceBuilder가 sourceVerses를 기반으로 방해 조각을 만드는 구조를 그대로 활용한다.
    /// </summary>
    public sealed class VeryHardQuestionGenerator : IWordOrderQuestionGenerator
    {
        private const int DEFAULT_HINT_COUNT = 1;
        private const int DEFAULT_TIME_LIMIT_SECONDS = 20;
        private const int MAX_MORPH_VARIANT_VERSE_COUNT = 2;
        private const int MAX_SIMILAR_PHRASE_VERSE_COUNT = 2;
        private const int MAX_ORDER_CONFUSION_VERSE_COUNT = 1;

        private static readonly Dictionary<string, string[]> SimilarPhraseMap = new(StringComparer.Ordinal)
        {
            { "마음을", new[] { "생각을", "중심을" } },
            { "생각을", new[] { "마음을", "뜻을" } },
            { "우리를", new[] { "너희를", "그들을" } },
            { "너희를", new[] { "우리를", "그들을" } },
            { "은혜를", new[] { "구원을", "복을" } },
            { "구원을", new[] { "은혜를", "복을" } },
            { "영광을", new[] { "기쁨을", "복을" } },
            { "세상을", new[] { "만물을", "너희를" } },
            { "더욱", new[] { "항상", "진실로" } },
            { "지키라", new[] { "보존하라", "지켜라" } },
            { "사랑하사", new[] { "사랑하여", "아끼사" } },
            { "믿음으로", new[] { "은혜로", "소망으로" } },
            { "안에", new[] { "가운데", "위에" } },
            { "가운데", new[] { "안에", "중에" } },
            { "기억하라", new[] { "생각하라", "잊지말라" } },
            { "구하라", new[] { "찾으라", "바라라" } },
            { "믿으라", new[] { "의지하라", "바라보라" } },
            { "행하라", new[] { "지키라", "따르라" } }
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

            { "그러므로", "그런즉" },
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

        public string Difficulty => WordOrderDifficulty.VeryHard;

        public WordOrderQuestion Generate(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IWordOrderPieceBuilder pieceBuilder,
            int hintCount,
            bool useTimer,
            int timeLimitSeconds,
            bool isFirstPieceFixed)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            if (pieceBuilder is null)
            {
                throw new ArgumentNullException(nameof(pieceBuilder));
            }

            IReadOnlyList<string> correctSequence = BuildCorrectSequence(pieceBuilder, verse);
            List<Verse> augmentedSourceVerses = BuildAugmentedSourceVerses(verse, sourceVerses, correctSequence);

            IReadOnlyList<WordOrderPieceItem> pieces = pieceBuilder.BuildPieces(
                verse,
                augmentedSourceVerses,
                correctSequence);

            return new WordOrderQuestion
            {
                Difficulty = Difficulty,
                ReferenceText = verse.Ref ?? string.Empty,
                VerseText = verse.Text ?? string.Empty,
                CorrectSequence = correctSequence.ToList(),
                Pieces = pieces.ToList(),
                HintCount = ResolveHintCount(hintCount),
                UseTimer = true,
                TimeLimitSeconds = ResolveTimeLimitSeconds(timeLimitSeconds),
                IsFirstPieceFixed = false
            };
        }

        /// <summary>
        /// 목적:
        /// Verse에서 정답 순서 목록을 안전하게 생성한다.
        /// </summary>
        private static IReadOnlyList<string> BuildCorrectSequence(
            IWordOrderPieceBuilder pieceBuilder,
            Verse verse)
        {
            IReadOnlyList<string> correctSequence = pieceBuilder.BuildCorrectSequence(verse);

            if (correctSequence.Count > 0)
            {
                return correctSequence;
            }

            string fallbackText = (verse.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fallbackText))
            {
                return Array.Empty<string>();
            }

            return new List<string> { fallbackText };
        }

        /// <summary>
        /// 목적:
        /// 원본 sourceVerses에 VeryHard 함정용 가상 Verse를 추가한 목록을 만든다.
        /// </summary>
        private static List<Verse> BuildAugmentedSourceVerses(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IReadOnlyList<string> correctSequence)
        {
            List<Verse> result = sourceVerses
                .Where(item => item is not null)
                .ToList();

            HashSet<string> existingTexts = new(
                result
                    .Select(item => Normalize(item.Text))
                    .Where(text => !string.IsNullOrWhiteSpace(text)),
                StringComparer.Ordinal);

            string originalVerseText = JoinPieces(correctSequence);

            AddSyntheticVerses(
                result,
                existingTexts,
                CreateMorphVariantVerseTexts(correctSequence, originalVerseText),
                MAX_MORPH_VARIANT_VERSE_COUNT,
                "VH-MORPH",
                verse.Ref);

            AddSyntheticVerses(
                result,
                existingTexts,
                CreateSimilarPhraseVerseTexts(correctSequence, originalVerseText),
                MAX_SIMILAR_PHRASE_VERSE_COUNT,
                "VH-SIMILAR",
                verse.Ref);

            AddSyntheticVerses(
                result,
                existingTexts,
                CreateOrderConfusionVerseTexts(correctSequence, originalVerseText),
                MAX_ORDER_CONFUSION_VERSE_COUNT,
                "VH-ORDER",
                verse.Ref);

            return result;
        }

        /// <summary>
        /// 목적:
        /// 원문 조각을 조사/어미/호응 단위로 미세 변형한 가상 Verse 텍스트 목록을 만든다.
        /// </summary>
        private static IEnumerable<string> CreateMorphVariantVerseTexts(
            IReadOnlyList<string> correctSequence,
            string originalVerseText)
        {
            List<string> candidates = new();

            for (int index = 0; index < correctSequence.Count; index++)
            {
                string originalPiece = correctSequence[index];
                string? variantPiece = CreateMorphVariantPiece(originalPiece);

                if (string.IsNullOrWhiteSpace(variantPiece))
                {
                    continue;
                }

                if (string.Equals(originalPiece, variantPiece, StringComparison.Ordinal))
                {
                    continue;
                }

                List<string> clonedPieces = correctSequence.ToList();
                clonedPieces[index] = variantPiece;

                string candidateText = JoinPieces(clonedPieces);

                if (!string.Equals(
                    Normalize(candidateText),
                    Normalize(originalVerseText),
                    StringComparison.Ordinal))
                {
                    candidates.Add(candidateText);
                }
            }

            return candidates;
        }

        /// <summary>
        /// 목적:
        /// 원문과 비슷하지만 정답은 아닌 표현을 넣은 가상 Verse 텍스트 목록을 만든다.
        /// </summary>
        private static IEnumerable<string> CreateSimilarPhraseVerseTexts(
            IReadOnlyList<string> correctSequence,
            string originalVerseText)
        {
            List<string> candidates = new();

            for (int index = 0; index < correctSequence.Count; index++)
            {
                string currentPiece = correctSequence[index];

                if (!SimilarPhraseMap.TryGetValue(currentPiece, out string[] replacements))
                {
                    continue;
                }

                foreach (string replacement in replacements)
                {
                    if (string.IsNullOrWhiteSpace(replacement))
                    {
                        continue;
                    }

                    if (string.Equals(currentPiece, replacement, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    List<string> clonedPieces = correctSequence.ToList();
                    clonedPieces[index] = replacement;

                    string candidateText = JoinPieces(clonedPieces);

                    if (!string.Equals(
                        Normalize(candidateText),
                        Normalize(originalVerseText),
                        StringComparison.Ordinal))
                    {
                        candidates.Add(candidateText);
                    }
                }
            }

            return candidates;
        }

        /// <summary>
        /// 목적:
        /// 인접 조각 두 개의 순서를 바꾼 가상 Verse 텍스트 목록을 만든다.
        /// </summary>
        private static IEnumerable<string> CreateOrderConfusionVerseTexts(
            IReadOnlyList<string> correctSequence,
            string originalVerseText)
        {
            List<string> candidates = new();

            if (correctSequence.Count < 2)
            {
                return candidates;
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

                List<string> clonedPieces = correctSequence.ToList();
                clonedPieces[index] = right;
                clonedPieces[index + 1] = left;

                string candidateText = JoinPieces(clonedPieces);

                if (!string.Equals(
                    Normalize(candidateText),
                    Normalize(originalVerseText),
                    StringComparison.Ordinal))
                {
                    candidates.Add(candidateText);
                }
            }

            return candidates;
        }

        /// <summary>
        /// 목적:
        /// 미세 변형용 단일 조각을 생성한다.
        /// </summary>
        private static string? CreateMorphVariantPiece(string piece)
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
        /// 인접 조각 순서 혼동 후보로 써도 되는지 판별한다.
        /// </summary>
        private static bool CanSwapForConfusion(string left, string right)
        {
            if (left.Length <= 1 || right.Length <= 1)
            {
                return false;
            }

            if (left.Length >= 10 || right.Length >= 10)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 목적:
        /// 가상 Verse 텍스트들을 중복 없이 제한 개수만큼 추가한다.
        /// </summary>
        private static void AddSyntheticVerses(
            List<Verse> target,
            HashSet<string> existingTexts,
            IEnumerable<string> candidateTexts,
            int maxCount,
            string tag,
            string? referenceText)
        {
            int addedCount = 0;

            foreach (string candidateText in candidateTexts)
            {
                if (addedCount >= maxCount)
                {
                    break;
                }

                string normalized = Normalize(candidateText);

                if (string.IsNullOrWhiteSpace(normalized))
                {
                    continue;
                }

                if (!existingTexts.Add(normalized))
                {
                    continue;
                }

                target.Add(new Verse(
                    $"{referenceText ?? string.Empty} [{tag}]",
                    candidateText));

                addedCount++;
            }
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
        /// 텍스트 비교용 정규화 문자열을 만든다.
        /// </summary>
        private static string Normalize(string? text)
        {
            return (text ?? string.Empty).Trim();
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

        /// <summary>
        /// 목적:
        /// VeryHard 기본 힌트 개수를 보장한다.
        /// </summary>
        private static int ResolveHintCount(int hintCount)
        {
            return hintCount > 0 ? hintCount : DEFAULT_HINT_COUNT;
        }

        /// <summary>
        /// 목적:
        /// VeryHard 기본 제한 시간을 보장한다.
        /// </summary>
        private static int ResolveTimeLimitSeconds(int timeLimitSeconds)
        {
            return timeLimitSeconds > 0 ? timeLimitSeconds : DEFAULT_TIME_LIMIT_SECONDS;
        }
    }
}