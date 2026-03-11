using ScriptureTyping.ViewModels.Games.Cloze.Models;
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 모드 전체 전략 객체.
    ///
    /// 구성:
    /// - 빈칸 1개
    /// - 보기 6개
    /// - 기본 채점 정책 사용
    /// </summary>
    public sealed class EasyClozeMode : IClozeMode
    {
        private readonly IClozeQuestionGenerator _questionGenerator;
        private readonly IClozeScoringPolicy _scoringPolicy;

        public EasyClozeMode()
            : this(
                  new EasyQuestionGenerator(new EasyChoiceGenerator()),
                  new EasyScoringPolicy())
        {
        }

        public EasyClozeMode(
            IClozeQuestionGenerator questionGenerator,
            IClozeScoringPolicy scoringPolicy)
        {
            _questionGenerator = questionGenerator ?? throw new ArgumentNullException(nameof(questionGenerator));
            _scoringPolicy = scoringPolicy ?? throw new ArgumentNullException(nameof(scoringPolicy));
        }

        public string Name => "Easy";

        public int BlankCount => 1;

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