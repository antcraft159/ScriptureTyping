using ScriptureTyping.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// Verse와 난이도 규칙을 기반으로 순서 맞추기 문제를 생성한다.
    /// </summary>
    public sealed class WordOrderQuestionFactory
    {
        /// <summary>
        /// 목적:
        /// 난이도 문자열에 해당하는 규칙 객체를 반환한다.
        /// </summary>
        public WordOrderDifficultyRules GetRules(string difficulty)
        {
            return WordOrderDifficultyRules.Create(difficulty);
        }

        /// <summary>
        /// 목적:
        /// 구절 1개를 순서 맞추기 문제로 변환한다.
        /// </summary>
        public WordOrderQuestion CreateQuestion(
            Verse verse,
            string difficulty,
            IEnumerable<Verse>? distractorSourceVerses = null)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            WordOrderDifficultyRules rules = GetRules(difficulty);
            List<string> correctSequence = SplitVerseText(verse.Text, rules);

            List<WordOrderPieceItem> pieces = correctSequence
                .Select((text, index) => new WordOrderPieceItem
                {
                    Text = text,
                    CorrectOrder = index,
                    IsDistractor = false
                })
                .ToList();

            if (rules.IncludeDistractors && rules.DistractorCount > 0)
            {
                List<string> distractors = BuildDistractorTexts(
                    correctSequence,
                    rules.DistractorCount,
                    distractorSourceVerses);

                foreach (string distractor in distractors)
                {
                    pieces.Add(new WordOrderPieceItem
                    {
                        Text = distractor,
                        CorrectOrder = -1,
                        IsDistractor = true
                    });
                }
            }

            Shuffle(pieces);

            return new WordOrderQuestion
            {
                Difficulty = rules.Difficulty,
                ReferenceText = verse.Ref,
                OriginalText = verse.Text,
                CorrectSequence = correctSequence,
                Pieces = pieces,
                HintCount = rules.HintCount,
                UseTimer = rules.UseTimer,
                TimeLimitSeconds = rules.TimeLimitSeconds,
                IsFirstPieceFixed = rules.IsFirstPieceFixed,
                IsLastPieceFixed = rules.IsLastPieceFixed,
                ShowSlotNumbers = rules.ShowSlotNumbers,
                ShowCorrectPositionFeedback = rules.ShowCorrectPositionFeedback
            };
        }

        /// <summary>
        /// 목적:
        /// 말씀 텍스트를 난이도 규칙에 맞는 조각 시퀀스로 분리한다.
        /// </summary>
        private List<string> SplitVerseText(string text, WordOrderDifficultyRules rules)
        {
            List<string> words = text
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (words.Count == 0)
            {
                return new List<string> { text };
            }

            if (rules.Difficulty == WordOrderDifficultyRules.Easy)
            {
                return BuildGroupedPieces(words, Math.Min(4, Math.Max(2, words.Count / 2)));
            }

            if (rules.Difficulty == WordOrderDifficultyRules.Normal)
            {
                return BuildGroupedPieces(words, Math.Min(6, Math.Max(3, words.Count)));
            }

            return words;
        }

        /// <summary>
        /// 목적:
        /// 너무 긴 말씀을 적절한 조각 수로 묶는다.
        /// </summary>
        private List<string> BuildGroupedPieces(List<string> words, int targetPieceCount)
        {
            if (words.Count <= targetPieceCount)
            {
                return new List<string>(words);
            }

            List<string> result = new();
            int baseSize = words.Count / targetPieceCount;
            int remainder = words.Count % targetPieceCount;
            int index = 0;

            for (int i = 0; i < targetPieceCount; i++)
            {
                int take = baseSize + (i < remainder ? 1 : 0);
                List<string> chunk = words.Skip(index).Take(take).ToList();
                result.Add(string.Join(" ", chunk));
                index += take;
            }

            return result.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }

        /// <summary>
        /// 목적:
        /// 방해 조각 텍스트를 생성한다.
        /// </summary>
        private List<string> BuildDistractorTexts(
            List<string> correctSequence,
            int distractorCount,
            IEnumerable<Verse>? distractorSourceVerses)
        {
            HashSet<string> result = new(StringComparer.Ordinal);

            if (distractorSourceVerses is not null)
            {
                List<string> sourceWords = distractorSourceVerses
                    .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Text))
                    .SelectMany(x => x.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    .Distinct(StringComparer.Ordinal)
                    .Where(x => !correctSequence.Contains(x))
                    .ToList();

                foreach (string word in sourceWords)
                {
                    result.Add(word);

                    if (result.Count >= distractorCount)
                    {
                        break;
                    }
                }
            }

            int fallbackIndex = 1;
            while (result.Count < distractorCount)
            {
                result.Add($"방해조각{fallbackIndex}");
                fallbackIndex++;
            }

            return result.Take(distractorCount).ToList();
        }

        /// <summary>
        /// 목적:
        /// 조각 목록을 랜덤 순서로 섞는다.
        /// </summary>
        private void Shuffle<T>(IList<T> list)
        {
            Random random = new();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}