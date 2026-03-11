// 파일명: HardClozeMode.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// 어려움 모드 전체 전략 객체.
    /// 
    /// 규칙:
    /// - 빈칸 2개
    /// - 유사 오답 포함 보기 생성
    /// - 기본 완전 일치 채점
    /// </summary>
    public sealed class HardClozeMode : IClozeMode
    {
        private readonly IClozeQuestionGenerator _questionGenerator;
        private readonly IClozeScoringPolicy _scoringPolicy;

        public HardClozeMode()
            : this(
                  new HardQuestionGenerator(new HardChoiceGenerator()),
                  new HardScoringPolicy())
        {
        }

        public HardClozeMode(
            IClozeQuestionGenerator questionGenerator,
            IClozeScoringPolicy scoringPolicy)
        {
            _questionGenerator = questionGenerator ?? throw new ArgumentNullException(nameof(questionGenerator));
            _scoringPolicy = scoringPolicy ?? throw new ArgumentNullException(nameof(scoringPolicy));
        }

        public string Name => "Hard";

        public int BlankCount => 2;

        public int ChoiceCountPerBlank => 6;

        public ClozeQuestion CreateQuestion(string verseText, IReadOnlyList<string> wordPool)
        {
            return _questionGenerator.Generate(verseText, BlankCount, wordPool);
        }

        public ClozeRoundResult Score(ClozeQuestion question, IReadOnlyList<string> submittedAnswers)
        {
            return _scoringPolicy.Score(question, submittedAnswers);
        }
    }
}