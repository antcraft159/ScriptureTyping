using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.Hard;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.SamuelRank1;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// Verse와 난이도에 따라 순서 맞추기 문제를 생성한다.
    ///
    /// 역할:
    /// - 난이도별 모드 반환
    /// - 모드가 가진 규칙/조각 생성기/채점 정책을 통해 문제 생성
    /// - ViewModel이 모드 내부 구현을 직접 알지 않아도 되도록 중간 진입점 역할 수행
    /// </summary>
    public sealed class WordOrderQuestionFactory
    {
        /// <summary>
        /// 목적:
        /// 난이도별 문제 규칙을 반환한다.
        /// </summary>
        public WordOrderRuleSet GetRules(string difficulty)
        {
            IWordOrderMode mode = GetMode(difficulty);

            return new WordOrderRuleSet(
                maxSubmitCount: mode.MaxSubmitCount,
                hintCount: mode.HintCount,
                useTimer: mode.UseTimer,
                timeLimitSeconds: mode.TimeLimitSeconds,
                isFirstPieceFixed: mode.IsFirstPieceFixed,
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

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            IWordOrderMode mode = GetMode(difficulty);
            return mode.CreateQuestion(verse, sourceVerses);
        }

        /// <summary>
        /// 목적:
        /// 난이도 문자열에 맞는 모드 객체를 반환한다.
        /// </summary>
        private static IWordOrderMode GetMode(string difficulty)
        {
            if (string.Equals(difficulty, WordOrderDifficulty.SamuelRank1, StringComparison.Ordinal))
            {
                return new SamuelRank1WordOrderMode();
            }

            if (string.Equals(difficulty, WordOrderDifficulty.VeryHard, StringComparison.Ordinal))
            {
                return new VeryHardWordOrderMode();
            }

            if (string.Equals(difficulty, WordOrderDifficulty.Hard, StringComparison.Ordinal))
            {
                return new HardWordOrderMode();
            }

            if (string.Equals(difficulty, WordOrderDifficulty.Normal, StringComparison.Ordinal))
            {
                return new NormalWordOrderMode();
            }

            return new EasyWordOrderMode();
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

        /// <summary>
        /// 기존 구조 호환용 유지 값
        /// 현재 실제 방해 조각 생성은 각 모드의 PieceBuilder가 담당한다.
        /// </summary>
        public int DistractorCount { get; }
    }
}