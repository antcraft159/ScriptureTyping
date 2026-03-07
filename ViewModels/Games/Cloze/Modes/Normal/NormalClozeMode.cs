// 파일명: NormalClozeMode.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 난이도 모드 전략 객체.
    /// 
    /// 규칙:
    /// - 빈칸 2개
    /// - 각 빈칸당 보기 6개
    /// </summary>
    public sealed class NormalClozeMode : IClozeMode
    {
        private readonly IClozeQuestionGenerator _questionGenerator;
        private readonly IClozeScoringPolicy _scoringPolicy;

        public NormalClozeMode()
            : this(
                  new NormalQuestionGenerator(new NormalChoiceGenerator()),
                  new NormalScoringPolicy())
        {
        }

        public NormalClozeMode(
            IClozeQuestionGenerator questionGenerator,
            IClozeScoringPolicy scoringPolicy)
        {
            _questionGenerator = questionGenerator ?? throw new ArgumentNullException(nameof(questionGenerator));
            _scoringPolicy = scoringPolicy ?? throw new ArgumentNullException(nameof(scoringPolicy));
        }

        public string Name => "Normal";

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