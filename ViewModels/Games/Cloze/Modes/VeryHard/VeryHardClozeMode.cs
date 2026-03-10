// 파일명: VeryHard/VeryHardClozeMode.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 모드 전략 객체.
    /// 
    /// 특징:
    /// - 빈칸 2개
    /// - 보기 없이 직접 입력하는 주관식 중심
    /// </summary>
    public sealed class VeryHardClozeMode : IClozeMode
    {
        private readonly IClozeQuestionGenerator _questionGenerator;
        private readonly IClozeScoringPolicy _scoringPolicy;

        public VeryHardClozeMode()
            : this(
                  new InputQuestionGenerator(),
                  new VeryHardScoringPolicy())
        {
        }

        public VeryHardClozeMode(
            IClozeQuestionGenerator questionGenerator,
            IClozeScoringPolicy scoringPolicy)
        {
            _questionGenerator = questionGenerator ?? throw new ArgumentNullException(nameof(questionGenerator));
            _scoringPolicy = scoringPolicy ?? throw new ArgumentNullException(nameof(scoringPolicy));
        }

        public string Name => "VeryHard";

        public int BlankCount => 2;

        public int ChoiceCountPerBlank => 0;

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