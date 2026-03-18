using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;
using ScriptureTyping.ViewModels.Games.VerseMatch.Helpers;
using ScriptureTyping.ViewModels.Games.VerseMatch.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.VerseMatch
{
    /// <summary>
    /// 목적:
    /// 구절 짝 맞추기 게임 문제를 생성한다.
    /// </summary>
    public sealed class VerseMatchQuestionFactory
    {
        private readonly Random _random;

        public VerseMatchQuestionFactory()
            : this(null)
        {
        }

        public VerseMatchQuestionFactory(Random? random)
        {
            _random = random ?? new Random();
        }

        /// <summary>
        /// 목적:
        /// 선택된 모드 정책에 따라 문제 목록을 생성한다.
        /// </summary>
        public IReadOnlyList<VerseMatchQuestion> CreateQuestions(
            IReadOnlyList<Verse> verses,
            IVerseMatchMode mode)
        {
            if (verses is null)
            {
                throw new ArgumentNullException(nameof(verses));
            }

            if (mode is null)
            {
                throw new ArgumentNullException(nameof(mode));
            }

            List<Verse> usableVerses = verses
                .Where(x => x is not null)
                .Where(x => !string.IsNullOrWhiteSpace(x.Ref))
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .ToList();

            if (usableVerses.Count < 2)
            {
                return Array.Empty<VerseMatchQuestion>();
            }

            List<Verse> shuffledVerses = usableVerses.ToList();
            VerseMatchShuffleHelper.Shuffle(shuffledVerses, _random);

            List<VerseMatchQuestion> questions = new List<VerseMatchQuestion>();
            int questionNumber = 1;
            int index = 0;

            while (index < shuffledVerses.Count)
            {
                List<Verse> chunk = shuffledVerses
                    .Skip(index)
                    .Take(mode.PairCount)
                    .ToList();

                if (chunk.Count < 2)
                {
                    break;
                }

                List<VerseMatchCardItem> cards = BuildRealCards(chunk, mode.PreviewLength);
                List<VerseMatchCardItem> fakeCards = BuildFakeCards(
                    sourceVerses: usableVerses,
                    currentChunk: chunk,
                    previewLength: mode.PreviewLength,
                    fakeCardCount: mode.FakeCardCount);

                cards.AddRange(fakeCards);
                AssignCardIds(cards);
                VerseMatchShuffleHelper.Shuffle(cards, _random);

                questions.Add(new VerseMatchQuestion
                {
                    QuestionNumber = questionNumber,
                    PairCount = chunk.Count,
                    UseTimer = mode.UseTimer,
                    TimeLimitSeconds = mode.UseTimer ? mode.TimeLimitSeconds : 0,
                    SourceVerses = chunk,
                    Cards = cards
                });

                questionNumber++;
                index += mode.PairCount;
            }

            return questions;
        }

        /// <summary>
        /// 목적:
        /// 진짜 장절 카드와 본문 카드 목록을 생성한다.
        /// </summary>
        private static List<VerseMatchCardItem> BuildRealCards(
            IReadOnlyList<Verse> verses,
            int previewLength)
        {
            List<VerseMatchCardItem> cards = new List<VerseMatchCardItem>();

            foreach (Verse verse in verses)
            {
                string reference = verse.Ref?.Trim() ?? string.Empty;
                string verseText = verse.Text?.Trim() ?? string.Empty;
                string pairKey = $"{reference}|{verseText}";

                cards.Add(new VerseMatchCardItem(
                    pairKey: pairKey,
                    displayText: reference,
                    fullText: reference,
                    cardType: VerseMatchCardType.Reference,
                    isFakeCard: false));

                cards.Add(new VerseMatchCardItem(
                    pairKey: pairKey,
                    displayText: BuildPreviewText(verseText, previewLength),
                    fullText: verseText,
                    cardType: VerseMatchCardType.VerseText,
                    isFakeCard: false));
            }

            return cards;
        }

        /// <summary>
        /// 목적:
        /// 난이도별 가짜 카드를 생성한다.
        /// </summary>
        private List<VerseMatchCardItem> BuildFakeCards(
            IReadOnlyList<Verse> sourceVerses,
            IReadOnlyList<Verse> currentChunk,
            int previewLength,
            int fakeCardCount)
        {
            List<VerseMatchCardItem> fakeCards = new List<VerseMatchCardItem>();

            if (fakeCardCount <= 0)
            {
                return fakeCards;
            }

            List<Verse> candidatePool = sourceVerses
                .Where(x => x is not null)
                .Where(x => !string.IsNullOrWhiteSpace(x.Ref))
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .Where(x => !currentChunk.Any(y =>
                    string.Equals(y.Ref, x.Ref, StringComparison.Ordinal) &&
                    string.Equals(y.Text, x.Text, StringComparison.Ordinal)))
                .ToList();

            if (candidatePool.Count == 0)
            {
                candidatePool = sourceVerses
                    .Where(x => x is not null)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Ref))
                    .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                    .ToList();
            }

            if (candidatePool.Count == 0)
            {
                return fakeCards;
            }

            for (int i = 0; i < fakeCardCount; i++)
            {
                Verse verse = candidatePool[_random.Next(candidatePool.Count)];
                bool makeReferenceCard = _random.Next(2) == 0;

                string reference = verse.Ref?.Trim() ?? string.Empty;
                string verseText = verse.Text?.Trim() ?? string.Empty;

                string displayText = makeReferenceCard
                    ? reference
                    : BuildPreviewText(verseText, previewLength);

                VerseMatchCardType cardType = makeReferenceCard
                    ? VerseMatchCardType.Reference
                    : VerseMatchCardType.VerseText;

                string fakePairKey = $"__FAKE__|{Guid.NewGuid():N}";

                fakeCards.Add(new VerseMatchCardItem(
                    pairKey: fakePairKey,
                    displayText: displayText,
                    fullText: makeReferenceCard ? reference : verseText,
                    cardType: cardType,
                    isFakeCard: true));
            }

            return fakeCards;
        }

        /// <summary>
        /// 목적:
        /// 카드 목록에 고유 CardId를 다시 부여한다.
        /// </summary>
        private static void AssignCardIds(IReadOnlyList<VerseMatchCardItem> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].CardId = i + 1;
            }
        }

        /// <summary>
        /// 목적:
        /// 카드에 표시할 본문 미리보기 문자열을 생성한다.
        /// </summary>
        private static string BuildPreviewText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string normalized = text.Trim();

            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return normalized.Substring(0, maxLength) + "...";
        }
    }
}