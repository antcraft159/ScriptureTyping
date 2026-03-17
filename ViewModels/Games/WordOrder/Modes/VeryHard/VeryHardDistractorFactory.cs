using ScriptureTyping.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// VeryHard 단계에서 사용할 방해 조각 후보를 우선순위에 따라 조합하고,
    /// 최종적으로 문제에 넣을 방해 조각 목록을 만든다.
    ///
    /// 역할:
    /// - 가상 Verse(VH-MORPH / VH-SIMILAR / VH-ORDER)에서 나온 조각을 우선 사용
    /// - 일반 Verse에서 나온 조각을 보조 후보로 사용
    /// - 정답 조각과 중복되는 방해 조각 제거
    /// - 중복 방해 조각 제거
    /// - 조각 수에 따라 최종 방해 조각 개수 계산
    ///
    /// 입력:
    /// - currentVerse: 현재 문제 원본 Verse
    /// - correctSequence: 현재 문제의 정답 조각 목록
    /// - sourceVerses: 방해 조각 후보를 만들 Verse 목록
    /// - pieceExtractor: Verse 하나에서 조각 후보 목록을 꺼내는 함수
    ///
    /// 출력:
    /// - 최종 방해 조각 문자열 목록
    ///
    /// 주의사항:
    /// - 이 클래스는 "방해 조각 선택"만 담당한다.
    /// - 실제 WordOrderPieceItem 생성 및 셔플은 다른 클래스에서 처리한다.
    /// </summary>
    public sealed class VeryHardDistractorFactory
    {
        private const int MIN_DISTRACTOR_COUNT = 2;
        private const int MAX_DISTRACTOR_COUNT = 4;

        private const string MORPH_TAG = "[VH-MORPH]";
        private const string SIMILAR_TAG = "[VH-SIMILAR]";
        private const string ORDER_TAG = "[VH-ORDER]";

        /// <summary>
        /// 목적:
        /// sourceVerses 전체를 검사해서 VeryHard용 최종 방해 조각 목록을 만든다.
        /// </summary>
        public IReadOnlyList<string> CreateDistractors(
            Verse currentVerse,
            IReadOnlyList<string> correctSequence,
            IReadOnlyList<Verse> sourceVerses,
            Func<Verse, IReadOnlyList<string>> pieceExtractor)
        {
            if (currentVerse is null)
            {
                throw new ArgumentNullException(nameof(currentVerse));
            }

            if (correctSequence is null)
            {
                throw new ArgumentNullException(nameof(correctSequence));
            }

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            if (pieceExtractor is null)
            {
                throw new ArgumentNullException(nameof(pieceExtractor));
            }

            HashSet<string> correctSet = new(
                correctSequence
                    .Where(piece => !string.IsNullOrWhiteSpace(piece))
                    .Select(Normalize),
                StringComparer.Ordinal);

            List<string> prioritizedPool = new();
            List<string> fallbackPool = new();

            foreach (Verse verse in sourceVerses)
            {
                if (verse is null)
                {
                    continue;
                }

                if (IsSameRealVerse(currentVerse, verse))
                {
                    continue;
                }

                IReadOnlyList<string> candidatePieces = pieceExtractor(verse);

                foreach (string candidatePiece in candidatePieces)
                {
                    string normalizedPiece = Normalize(candidatePiece);

                    if (IsInvalidDistractor(normalizedPiece, correctSet))
                    {
                        continue;
                    }

                    if (IsSyntheticVerse(verse))
                    {
                        prioritizedPool.Add(normalizedPiece);
                    }
                    else
                    {
                        fallbackPool.Add(normalizedPiece);
                    }
                }
            }

            return SelectFinalDistractors(
                prioritizedPool,
                fallbackPool,
                CalculateDistractorCount(correctSet.Count));
        }

        /// <summary>
        /// 목적:
        /// 우선순위 후보와 일반 후보를 합쳐 최종 방해 조각 목록을 만든다.
        /// </summary>
        public IReadOnlyList<string> SelectFinalDistractors(
            IReadOnlyList<string> prioritizedPool,
            IReadOnlyList<string> fallbackPool,
            int takeCount)
        {
            if (prioritizedPool is null)
            {
                throw new ArgumentNullException(nameof(prioritizedPool));
            }

            if (fallbackPool is null)
            {
                throw new ArgumentNullException(nameof(fallbackPool));
            }

            if (takeCount <= 0)
            {
                return Array.Empty<string>();
            }

            List<string> prioritizedDistinct = prioritizedPool
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(Normalize)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            List<string> fallbackDistinct = fallbackPool
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(Normalize)
                .Distinct(StringComparer.Ordinal)
                .Where(text => !prioritizedDistinct.Contains(text, StringComparer.Ordinal))
                .ToList();

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
        /// 정답 조각 개수에 맞는 방해 조각 수를 계산한다.
        /// </summary>
        public int CalculateDistractorCount(int correctPieceCount)
        {
            int takeCount = Math.Max(MIN_DISTRACTOR_COUNT, correctPieceCount / 3);
            takeCount = Math.Min(MAX_DISTRACTOR_COUNT, takeCount);
            return takeCount;
        }

        /// <summary>
        /// 목적:
        /// 현재 후보가 방해 조각으로 부적절한지 검사한다.
        /// </summary>
        private static bool IsInvalidDistractor(string text, HashSet<string> correctSet)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (text.Length <= 1)
            {
                return true;
            }

            if (IsOnlyPunctuation(text))
            {
                return true;
            }

            if (correctSet.Contains(text))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 목적:
        /// 현재 Verse와 후보 Verse가 같은 실제 구절인지 판별한다.
        /// </summary>
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

        /// <summary>
        /// 목적:
        /// 가상 VeryHard Verse인지 판별한다.
        /// </summary>
        private static bool IsSyntheticVerse(Verse verse)
        {
            return HasTag(verse, MORPH_TAG) ||
                   HasTag(verse, SIMILAR_TAG) ||
                   HasTag(verse, ORDER_TAG);
        }

        /// <summary>
        /// 목적:
        /// Verse Ref에 특정 태그가 포함되어 있는지 검사한다.
        /// </summary>
        private static bool HasTag(Verse verse, string tag)
        {
            string reference = verse.Ref ?? string.Empty;
            return reference.Contains(tag, StringComparison.Ordinal);
        }

        /// <summary>
        /// 목적:
        /// 문자열 비교용 정규화 텍스트를 만든다.
        /// </summary>
        private static string Normalize(string? text)
        {
            return (text ?? string.Empty).Trim();
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