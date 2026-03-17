using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// Verse와 난이도에 따라 순서 맞추기 문제를 생성한다.
    ///
    /// 역할:
    /// - 난이도별 규칙 반환
    /// - 정답 조각 분리
    /// - 보기 조각 목록 생성
    /// - 방해 조각 추가
    /// </summary>
    public sealed class WordOrderQuestionFactory
    {
        /// <summary>
        /// 목적:
        /// 난이도별 문제 규칙을 반환한다.
        /// </summary>
        public WordOrderRuleSet GetRules(string difficulty)
        {
            if (string.Equals(difficulty, WordOrderDifficulty.VeryHard, StringComparison.Ordinal))
            {
                return new WordOrderRuleSet(
                    maxSubmitCount: 2,
                    hintCount: 1,
                    useTimer: true,
                    timeLimitSeconds: 30,
                    isFirstPieceFixed: false,
                    distractorCount: 2);
            }

            if (string.Equals(difficulty, WordOrderDifficulty.Hard, StringComparison.Ordinal))
            {
                return new WordOrderRuleSet(
                    maxSubmitCount: 2,
                    hintCount: 1,
                    useTimer: false,
                    timeLimitSeconds: 0,
                    isFirstPieceFixed: false,
                    distractorCount: 0);
            }

            if (string.Equals(difficulty, WordOrderDifficulty.Normal, StringComparison.Ordinal))
            {
                return new WordOrderRuleSet(
                    maxSubmitCount: 2,
                    hintCount: 2,
                    useTimer: false,
                    timeLimitSeconds: 0,
                    isFirstPieceFixed: false,
                    distractorCount: 0);
            }

            return new WordOrderRuleSet(
                maxSubmitCount: 3,
                hintCount: 3,
                useTimer: false,
                timeLimitSeconds: 0,
                isFirstPieceFixed: true,
                distractorCount: 0);
        }

        /// <summary>
        /// 목적:
        /// 실제 게임 문제를 생성한다.
        /// </summary>
        public WordOrderQuestion CreateQuestion(
            Verse verse,
            string difficulty,
            IReadOnlyList<Verse> sourceVerses)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            WordOrderRuleSet rules = GetRules(difficulty);

            List<string> correctSequence = SplitText(verse.Text, difficulty);

            List<WordOrderPieceItem> pieces = correctSequence
                .Select(text => new WordOrderPieceItem(text, isDistractor: false))
                .ToList();

            if (rules.DistractorCount > 0 && sourceVerses is not null)
            {
                IEnumerable<string> distractors = BuildDistractors(
                    currentVerse: verse,
                    sourceVerses: sourceVerses,
                    correctSequence: correctSequence,
                    distractorCount: rules.DistractorCount,
                    difficulty: difficulty);

                foreach (string distractor in distractors)
                {
                    pieces.Add(new WordOrderPieceItem(distractor, isDistractor: true));
                }
            }

            Shuffle(pieces);

            return new WordOrderQuestion
            {
                Difficulty = difficulty,
                ReferenceText = verse.Ref ?? string.Empty,
                VerseText = verse.Text ?? string.Empty,
                CorrectSequence = correctSequence,
                Pieces = pieces,
                HintCount = rules.HintCount,
                UseTimer = rules.UseTimer,
                TimeLimitSeconds = rules.TimeLimitSeconds,
                IsFirstPieceFixed = rules.IsFirstPieceFixed
            };
        }

        /// <summary>
        /// 목적:
        /// 난이도에 맞게 말씀을 조각으로 분리한다.
        /// </summary>
        private static List<string> SplitText(string text, string difficulty)
        {
            List<string> words = (text ?? string.Empty)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (words.Count == 0)
            {
                return new List<string>();
            }

            if (string.Equals(difficulty, WordOrderDifficulty.Easy, StringComparison.Ordinal))
            {
                return BuildEasyChunks(words);
            }

            return words;
        }

        /// <summary>
        /// 목적:
        /// 쉬움 난이도에서 큰 단위 조각을 만든다.
        /// </summary>
        private static List<string> BuildEasyChunks(List<string> words)
        {
            List<string> chunks = new();
            int index = 0;

            while (index < words.Count)
            {
                int remain = words.Count - index;
                int take = remain >= 3 ? 2 : 1;

                chunks.Add(string.Join(" ", words.Skip(index).Take(take)));
                index += take;
            }

            return chunks;
        }

        /// <summary>
        /// 목적:
        /// 다른 말씀에서 방해 조각을 추출한다.
        /// </summary>
        private static IEnumerable<string> BuildDistractors(
            Verse currentVerse,
            IReadOnlyList<Verse> sourceVerses,
            IReadOnlyList<string> correctSequence,
            int distractorCount,
            string difficulty)
        {
            List<string> pool = new();

            foreach (Verse verse in sourceVerses)
            {
                if (verse is null)
                {
                    continue;
                }

                if (ReferenceEquals(verse, currentVerse))
                {
                    continue;
                }

                List<string> pieces = SplitText(verse.Text, difficulty);

                foreach (string piece in pieces)
                {
                    if (!correctSequence.Contains(piece))
                    {
                        pool.Add(piece);
                    }
                }
            }

            return pool
                .Distinct()
                .Take(distractorCount)
                .ToList();
        }

        /// <summary>
        /// 목적:
        /// 보기 조각 순서를 섞는다.
        /// </summary>
        private static void Shuffle(List<WordOrderPieceItem> pieces)
        {
            Random random = new();

            for (int i = pieces.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }
        }
    }

    /// <summary>
    /// 목적:
    /// 난이도별 규칙 묶음
    /// </summary>
    public sealed class WordOrderRuleSet
    {
        public WordOrderRuleSet(
            int maxSubmitCount,
            int hintCount,
            bool useTimer,
            int timeLimitSeconds,
            bool isFirstPieceFixed,
            int distractorCount)
        {
            MaxSubmitCount = maxSubmitCount;
            HintCount = hintCount;
            UseTimer = useTimer;
            TimeLimitSeconds = timeLimitSeconds;
            IsFirstPieceFixed = isFirstPieceFixed;
            DistractorCount = distractorCount;
        }

        public int MaxSubmitCount { get; }
        public int HintCount { get; }
        public bool UseTimer { get; }
        public int TimeLimitSeconds { get; }
        public bool IsFirstPieceFixed { get; }
        public int DistractorCount { get; }
    }
}