using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.VerseMatch.Helpers;
using ScriptureTyping.ViewModels.Games.VerseMatch.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Services
{
    /// <summary>
    /// 목적:
    /// 구절 짝 맞추기 게임 문제를 생성한다.
    /// </summary>
    public sealed class VerseMatchQuestionFactory
    {
        private const string SAMUEL_RANK_1_DIFFICULTY = "사무엘 1등";

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
        /// 난이도별 문제 목록을 생성한다.
        /// </summary>
        public IReadOnlyList<VerseMatchQuestion> CreateQuestions(
            IReadOnlyList<Verse> verses,
            string difficulty)
        {
            if (verses is null)
            {
                throw new ArgumentNullException(nameof(verses));
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

            int pairCount = GetPairCount(difficulty);
            int previewLength = GetPreviewLength(difficulty);
            int fakeCardCount = GetFakeCardCount(difficulty);
            bool useTimer = UseTimer(difficulty);
            int timeLimitSeconds = GetTimeLimitSeconds(difficulty);

            List<Verse> shuffledVerses = usableVerses.ToList();
            VerseMatchShuffleHelper.Shuffle(shuffledVerses, _random);

            List<VerseMatchQuestion> questions = new List<VerseMatchQuestion>();
            int questionNumber = 1;
            int index = 0;

            while (index < shuffledVerses.Count)
            {
                List<Verse> chunk = shuffledVerses
                    .Skip(index)
                    .Take(pairCount)
                    .ToList();

                if (chunk.Count < 2)
                {
                    break;
                }

                List<VerseMatchCardItem> cards = BuildRealCards(
                    verses: chunk,
                    difficulty: difficulty,
                    previewLength: previewLength);

                List<VerseMatchCardItem> fakeCards = BuildFakeCards(
                    sourceVerses: usableVerses,
                    currentChunk: chunk,
                    difficulty: difficulty,
                    previewLength: previewLength,
                    fakeCardCount: fakeCardCount);

                cards.AddRange(fakeCards);
                AssignCardIds(cards);
                VerseMatchShuffleHelper.Shuffle(cards, _random);

                questions.Add(new VerseMatchQuestion
                {
                    QuestionNumber = questionNumber,
                    PairCount = chunk.Count,
                    UseTimer = useTimer,
                    TimeLimitSeconds = useTimer ? timeLimitSeconds : 0,
                    SourceVerses = chunk,
                    Cards = cards
                });

                questionNumber++;
                index += pairCount;
            }

            return questions;
        }

        /// <summary>
        /// 목적:
        /// 진짜 장절 카드와 본문 카드 목록을 생성한다.
        /// </summary>
        private List<VerseMatchCardItem> BuildRealCards(
            IReadOnlyList<Verse> verses,
            string difficulty,
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
                    displayText: BuildPreviewText(verseText, previewLength, difficulty),
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
            string difficulty,
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
                    : BuildPreviewText(verseText, previewLength, difficulty);

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
        /// 규칙:
        /// - 일반 난이도는 앞부분부터 잘라서 표시
        /// - 사무엘 1등은 본문 중간 어절에서 랜덤 시작하여 표시
        /// </summary>
        private string BuildPreviewText(string text, int maxLength, string difficulty)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string normalized = text.Trim();

            if (string.Equals(difficulty, SAMUEL_RANK_1_DIFFICULTY, StringComparison.Ordinal))
            {
                return BuildSamuelRankPreviewText(normalized, maxLength);
            }

            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return normalized.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// 목적:
        /// 사무엘 1등 난이도에서 본문 중간 어절부터 시작하는 미리보기 문자열을 생성한다.
        /// 주의:
        /// - 첫 어절 고정 시작을 피한다.
        /// - 너무 짧은 본문은 전체 또는 앞부분을 그대로 반환한다.
        /// - 적용 버튼을 다시 누르면 문제를 새로 만들면서 시작 위치도 다시 달라진다.
        /// </summary>
        private string BuildSamuelRankPreviewText(string text, int maxLength)
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

            string[] words = normalized
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= 1)
            {
                return normalized.Length <= maxLength
                    ? normalized
                    : normalized.Substring(0, maxLength) + "...";
            }

            int maxStartIndexExclusive = Math.Max(1, words.Length - 1);

            int startIndex;
            if (words.Length <= 3)
            {
                startIndex = 0;
            }
            else
            {
                startIndex = _random.Next(1, maxStartIndexExclusive);
            }

            string preview = string.Join(" ", words.Skip(startIndex));

            if (preview.Length > maxLength)
            {
                preview = preview.Substring(0, maxLength) + "...";
            }

            if (string.IsNullOrWhiteSpace(preview))
            {
                preview = normalized.Length <= maxLength
                    ? normalized
                    : normalized.Substring(0, maxLength) + "...";
            }

            return preview;
        }

        /// <summary>
        /// 목적:
        /// 난이도별 실제 짝 개수를 반환한다.
        /// </summary>
        public int GetPairCount(string difficulty)
        {
            return difficulty switch
            {
                "쉬움" => 3,
                "보통" => 4,
                "어려움" => 5,
                "매우 어려움" => 5,
                "사무엘 1등" => 6,
                _ => 4
            };
        }

        /// <summary>
        /// 목적:
        /// 난이도별 본문 미리보기 길이를 반환한다.
        /// </summary>
        public int GetPreviewLength(string difficulty)
        {
            return difficulty switch
            {
                "쉬움" => 22,
                "보통" => 18,
                "어려움" => 14,
                "매우 어려움" => 11,
                "사무엘 1등" => 10,
                _ => 18
            };
        }

        /// <summary>
        /// 목적:
        /// 난이도별 가짜 카드 개수를 반환한다.
        /// </summary>
        public int GetFakeCardCount(string difficulty)
        {
            return difficulty switch
            {
                "쉬움" => 0,
                "보통" => 0,
                "어려움" => 3,
                "매우 어려움" => 5,
                "사무엘 1등" => 7,
                _ => 0
            };
        }

        /// <summary>
        /// 목적:
        /// 난이도별 타이머 사용 여부를 반환한다.
        /// </summary>
        public bool UseTimer(string difficulty)
        {
            return difficulty switch
            {
                "매우 어려움" => true,
                "사무엘 1등" => true,
                _ => false
            };
        }

        /// <summary>
        /// 목적:
        /// 난이도별 제한 시간을 반환한다.
        /// </summary>
        public int GetTimeLimitSeconds(string difficulty)
        {
            return difficulty switch
            {
                "매우 어려움" => 60,
                "사무엘 1등" => 100,
                _ => 0
            };
        }
    }
}