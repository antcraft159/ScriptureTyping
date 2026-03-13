using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// 최고 난이도 "사무엘 1등" 모드 전략 객체.
    ///
    /// 특징:
    /// - 권/장/절만 보여주고 말씀 전체를 직접 입력한다.
    /// - 보기는 제공하지 않는다.
    /// - 플래시 미리보기와 압박형 채점을 사용한다.
    /// </summary>
    public sealed class SamuelRank1ClozeMode : IClozeMode
    {
        private readonly IClozeQuestionGenerator _questionGenerator;
        private readonly IClozeScoringPolicy _scoringPolicy;

        /// <summary>
        /// 잠깐 보여주기 상태를 관리하는 컨트롤러
        /// </summary>
        public FlashPreviewController PreviewController { get; }

        public SamuelRank1ClozeMode()
            : this(
                  new ChainQuestionGenerator(),
                  new PressureScoringPolicy(),
                  new FlashPreviewController())
        {
        }

        public SamuelRank1ClozeMode(
            IClozeQuestionGenerator questionGenerator,
            IClozeScoringPolicy scoringPolicy,
            FlashPreviewController previewController)
        {
            _questionGenerator = questionGenerator ?? throw new ArgumentNullException(nameof(questionGenerator));
            _scoringPolicy = scoringPolicy ?? throw new ArgumentNullException(nameof(scoringPolicy));
            PreviewController = previewController ?? throw new ArgumentNullException(nameof(previewController));
        }

        public string Name => "SamuelRank1";

        /// <summary>
        /// 사무엘 1등은 빈칸 채우기가 아니라 전체 입력 방식이다.
        /// </summary>
        public int BlankCount => 0;

        /// <summary>
        /// 사무엘 1등은 보기를 제공하지 않는다.
        /// </summary>
        public int ChoiceCountPerBlank => 0;

        public ClozeQuestion CreateQuestion(string verseText, IReadOnlyList<string> wordPool)
        {
            return _questionGenerator.Generate(verseText, BlankCount, wordPool);
        }

        public ClozeRoundResult Score(ClozeQuestion question, IReadOnlyList<string> submittedAnswers)
        {
            return _scoringPolicy.Score(question, submittedAnswers);
        }

        /// <summary>
        /// 목적:
        /// 짧은 미리보기를 시작한다.
        /// </summary>
        public void StartPreview(TimeSpan? duration = null)
        {
            PreviewController.Start(duration);
        }

        /// <summary>
        /// 목적:
        /// 미리보기 갱신 여부를 확인한다.
        /// true면 방금 종료된 상태다.
        /// </summary>
        public bool UpdatePreview()
        {
            return PreviewController.Update();
        }

        /// <summary>
        /// 목적:
        /// 미리보기를 즉시 종료한다.
        /// </summary>
        public void StopPreview()
        {
            PreviewController.Stop();
        }
    }
}