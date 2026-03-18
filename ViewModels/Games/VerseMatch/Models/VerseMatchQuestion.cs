using ScriptureTyping.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Models
{
    /// <summary>
    /// 목적:
    /// 구절 짝 맞추기 게임의 문제 1개를 표현한다.
    /// </summary>
    public sealed class VerseMatchQuestion
    {
        /// <summary>
        /// 목적:
        /// 문제 번호
        /// </summary>
        public int QuestionNumber { get; init; }

        /// <summary>
        /// 목적:
        /// 현재 문제에 포함된 진짜 짝 개수
        /// </summary>
        public int PairCount { get; init; }

        /// <summary>
        /// 목적:
        /// 타이머 사용 여부
        /// </summary>
        public bool UseTimer { get; init; }

        /// <summary>
        /// 목적:
        /// 제한 시간(초)
        /// </summary>
        public int TimeLimitSeconds { get; init; }

        /// <summary>
        /// 목적:
        /// 원본 말씀 목록
        /// </summary>
        public IReadOnlyList<Verse> SourceVerses { get; init; } = Array.Empty<Verse>();

        /// <summary>
        /// 목적:
        /// 화면에 표시할 카드 목록
        /// </summary>
        public IReadOnlyList<VerseMatchCardItem> Cards { get; init; } = Array.Empty<VerseMatchCardItem>();

        /// <summary>
        /// 목적:
        /// 아직 매칭되지 않은 진짜 카드 쌍 수를 계산한다.
        /// </summary>
        public int GetRemainingPairCount()
        {
            if (Cards.Count == 0)
            {
                return 0;
            }

            return Cards
                .Where(x => !x.IsFakeCard)
                .Where(x => !x.IsMatched)
                .Select(x => x.PairKey)
                .Distinct(StringComparer.Ordinal)
                .Count();
        }

        /// <summary>
        /// 목적:
        /// 모든 진짜 카드 쌍을 맞췄는지 확인한다.
        /// 가짜 카드는 완료 조건에 포함하지 않는다.
        /// </summary>
        public bool IsCompleted()
        {
            List<VerseMatchCardItem> realCards = Cards
                .Where(x => !x.IsFakeCard)
                .ToList();

            if (realCards.Count == 0)
            {
                return false;
            }

            return realCards.All(x => x.IsMatched);
        }
    }
}