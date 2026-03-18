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

                List<VerseMatchCardItem> cards = BuildCards(chunk, previewLength);
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
        /// 장절 카드와 본문 카드 목록을 생성한다.
        /// </summary>
        private static List<VerseMatchCardItem> BuildCards(
    IReadOnlyList<Verse> verses,
    int previewLength)
        {
            List<VerseMatchCardItem> cards = new List<VerseMatchCardItem>();
            int cardIdSeed = 1;

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
                    cardId: cardIdSeed++));

                cards.Add(new VerseMatchCardItem(
                    pairKey: pairKey,
                    displayText: BuildPreviewText(verseText, previewLength),
                    fullText: verseText,
                    cardType: VerseMatchCardType.VerseText,
                    cardId: cardIdSeed++));
            }

            return cards;
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

        public bool UseTimer(string difficulty)
        {
            return difficulty switch
            {
                "매우 어려움" => true,
                "사무엘 1등" => true,
                _ => false
            };
        }

        public int GetTimeLimitSeconds(string difficulty)
        {
            return difficulty switch
            {
                "매우 어려움" => 45,
                "사무엘 1등" => 40,
                _ => 0
            };
        }
    }
}