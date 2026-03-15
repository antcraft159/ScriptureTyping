using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임의 난이도별 규칙 데이터를 보관한다.
    ///
    /// 역할:
    /// - 난이도 이름 상수 제공
    /// - 난이도 목록 제공
    /// - 난이도별 안내/정답/오답 문구 생성
    /// - 난이도별 공통 규칙 객체 생성
    /// </summary>
    public sealed class WordOrderDifficultyRules
    {
        public const string Easy = "쉬움";
        public const string Normal = "보통";
        public const string Hard = "어려움";
        public const string VeryHard = "매우 어려움";
        public const string SamuelRank1 = "사무엘 1등";

        private static readonly IReadOnlyList<string> _allDifficulties = new[]
        {
            Easy,
            Normal,
            Hard,
            VeryHard,
            SamuelRank1
        };

        public string Difficulty { get; init; } = string.Empty;

        public int MinPieceCount { get; init; }

        public int MaxPieceCount { get; init; }

        public bool IsFirstPieceFixed { get; init; }

        public bool IsLastPieceFixed { get; init; }

        public bool ShowSlotNumbers { get; init; }

        public bool ShowCorrectPositionFeedback { get; init; }

        public bool IncludeDistractors { get; init; }

        public int DistractorCount { get; init; }

        public int HintCount { get; init; }

        public bool UseTimer { get; init; }

        public int TimeLimitSeconds { get; init; }

        public int MaxSubmitCount { get; init; }

        /// <summary>
        /// 화면에서 사용할 전체 난이도 목록
        /// </summary>
        public static IReadOnlyList<string> AllDifficulties => _allDifficulties;

        /// <summary>
        /// 난이도별 시작 안내 문구를 반환한다.
        /// </summary>
        public static string GetInitialGuideText(string difficulty)
        {
            if (string.Equals(difficulty, Easy, StringComparison.Ordinal))
            {
                return "보기 조각을 눌러 순서대로 배열하세요.";
            }

            if (string.Equals(difficulty, Normal, StringComparison.Ordinal))
            {
                return "어절 순서를 기억해서 배열하세요.";
            }

            if (string.Equals(difficulty, Hard, StringComparison.Ordinal))
            {
                return "정확한 순서를 기억해서 맞춰보세요.";
            }

            if (string.Equals(difficulty, VeryHard, StringComparison.Ordinal))
            {
                return "가짜 조각에 주의하면서 순서를 맞추세요.";
            }

            if (string.Equals(difficulty, SamuelRank1, StringComparison.Ordinal))
            {
                return "시간 안에 정확하게 완성하세요.";
            }

            return "순서를 맞춰보세요.";
        }

        /// <summary>
        /// 난이도별 정답 문구를 반환한다.
        /// </summary>
        public static string GetCorrectFeedbackText(string difficulty)
        {
            if (string.Equals(difficulty, SamuelRank1, StringComparison.Ordinal))
            {
                return "정답입니다. 사무엘 1등 단계 통과!";
            }

            return "정답입니다.";
        }

        /// <summary>
        /// 난이도별 오답 문구를 반환한다.
        /// </summary>
        public static string GetWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (answerPieces is null)
            {
                throw new ArgumentNullException(nameof(answerPieces));
            }

            if (string.Equals(question.Difficulty, Easy, StringComparison.Ordinal))
            {
                int wrongCount = CountWrongPositions(question, answerPieces);
                return $"오답입니다. 틀린 위치 {wrongCount}개";
            }

            if (string.Equals(question.Difficulty, Normal, StringComparison.Ordinal))
            {
                return "오답입니다. 순서를 다시 확인하세요.";
            }

            if (string.Equals(question.Difficulty, Hard, StringComparison.Ordinal))
            {
                int wrongCount = CountWrongPositions(question, answerPieces);
                return $"오답입니다. 틀린 조각 {wrongCount}개";
            }

            if (string.Equals(question.Difficulty, VeryHard, StringComparison.Ordinal))
            {
                return containsDistractor
                    ? "오답입니다. 방해 조각이 포함되어 있습니다."
                    : "오답입니다. 선택 또는 순서가 틀렸습니다.";
            }

            if (string.Equals(question.Difficulty, SamuelRank1, StringComparison.Ordinal))
            {
                return "실패했습니다.";
            }

            return "오답입니다.";
        }

        /// <summary>
        /// 기본 난이도 규칙 객체를 반환한다.
        /// 필요하면 QuestionFactory에서 이 메서드를 사용하도록 확장할 수 있다.
        /// </summary>
        public static WordOrderDifficultyRules Create(string difficulty)
        {
            if (string.Equals(difficulty, Easy, StringComparison.Ordinal))
            {
                return new WordOrderDifficultyRules
                {
                    Difficulty = Easy,
                    MinPieceCount = 2,
                    MaxPieceCount = 4,
                    IsFirstPieceFixed = true,
                    IsLastPieceFixed = false,
                    ShowSlotNumbers = true,
                    ShowCorrectPositionFeedback = true,
                    IncludeDistractors = false,
                    DistractorCount = 0,
                    HintCount = 2,
                    UseTimer = false,
                    TimeLimitSeconds = 0,
                    MaxSubmitCount = 3
                };
            }

            if (string.Equals(difficulty, Normal, StringComparison.Ordinal))
            {
                return new WordOrderDifficultyRules
                {
                    Difficulty = Normal,
                    MinPieceCount = 3,
                    MaxPieceCount = 6,
                    IsFirstPieceFixed = false,
                    IsLastPieceFixed = false,
                    ShowSlotNumbers = true,
                    ShowCorrectPositionFeedback = false,
                    IncludeDistractors = false,
                    DistractorCount = 0,
                    HintCount = 2,
                    UseTimer = false,
                    TimeLimitSeconds = 0,
                    MaxSubmitCount = 3
                };
            }

            if (string.Equals(difficulty, Hard, StringComparison.Ordinal))
            {
                return new WordOrderDifficultyRules
                {
                    Difficulty = Hard,
                    MinPieceCount = 4,
                    MaxPieceCount = 8,
                    IsFirstPieceFixed = false,
                    IsLastPieceFixed = false,
                    ShowSlotNumbers = false,
                    ShowCorrectPositionFeedback = true,
                    IncludeDistractors = false,
                    DistractorCount = 0,
                    HintCount = 1,
                    UseTimer = false,
                    TimeLimitSeconds = 0,
                    MaxSubmitCount = 2
                };
            }

            if (string.Equals(difficulty, VeryHard, StringComparison.Ordinal))
            {
                return new WordOrderDifficultyRules
                {
                    Difficulty = VeryHard,
                    MinPieceCount = 4,
                    MaxPieceCount = 8,
                    IsFirstPieceFixed = false,
                    IsLastPieceFixed = false,
                    ShowSlotNumbers = false,
                    ShowCorrectPositionFeedback = false,
                    IncludeDistractors = true,
                    DistractorCount = 2,
                    HintCount = 1,
                    UseTimer = false,
                    TimeLimitSeconds = 0,
                    MaxSubmitCount = 2
                };
            }

            if (string.Equals(difficulty, SamuelRank1, StringComparison.Ordinal))
            {
                return new WordOrderDifficultyRules
                {
                    Difficulty = SamuelRank1,
                    MinPieceCount = 5,
                    MaxPieceCount = 10,
                    IsFirstPieceFixed = false,
                    IsLastPieceFixed = false,
                    ShowSlotNumbers = false,
                    ShowCorrectPositionFeedback = false,
                    IncludeDistractors = true,
                    DistractorCount = 3,
                    HintCount = 0,
                    UseTimer = true,
                    TimeLimitSeconds = 20,
                    MaxSubmitCount = 1
                };
            }

            return Create(Easy);
        }

        private static int CountWrongPositions(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            int count = 0;
            int compareCount = Math.Min(answerPieces.Count, question.CorrectSequence.Count);

            for (int i = 0; i < compareCount; i++)
            {
                if (!string.Equals(answerPieces[i].Text, question.CorrectSequence[i], StringComparison.Ordinal))
                {
                    count++;
                }
            }

            count += Math.Abs(question.CorrectSequence.Count - answerPieces.Count);

            return count;
        }
    }
}