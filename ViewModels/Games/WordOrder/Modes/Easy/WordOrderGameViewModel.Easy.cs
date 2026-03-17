using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy;
using System;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// WordOrderGameViewModel에서 쉬움 단계 전용 로직을 별도 partial 파일로 분리한다.
    ///
    /// 현재 역할:
    /// - Easy 모드 객체 접근
    /// - Easy 문제 생성 래핑
    /// - Easy 채점 래핑
    /// - Easy 힌트 래핑
    ///
    /// 주의:
    /// - 아직 전체 난이도 팩토리 구조로 완전히 옮기기 전의 중간 단계용 파일이다.
    /// </summary>
    public sealed partial class WordOrderGameViewModel
    {
        private EasyWordOrderMode? _easyMode;

        private EasyWordOrderMode EasyMode => _easyMode ??= new EasyWordOrderMode();

        /// <summary>
        /// 쉬움 난이도 질문 생성 전용 래퍼
        /// </summary>
        private WordOrderQuestion CreateEasyQuestion(Verse verse)
        {
            return EasyMode.CreateQuestion(verse, _sourceVerses);
        }

        /// <summary>
        /// 쉬움 난이도 정답 판정 전용 래퍼
        /// </summary>
        private bool IsEasyAnswerCorrect(WordOrderQuestion question)
        {
            return EasyMode.IsAnswerCorrect(question, AnswerPieces);
        }

        /// <summary>
        /// 쉬움 난이도 힌트 적용 전용 래퍼
        /// </summary>
        private bool TryUseEasyHint(out string message)
        {
            message = string.Empty;

            if (_currentQuestion is null)
            {
                message = "현재 문제가 없습니다.";
                return false;
            }

            bool applied = EasyMode.TryApplyHint(
                _currentQuestion,
                AvailablePieces,
                AnswerPieces,
                out message);

            if (applied)
            {
                RefreshPlacedIndexes();
                UpdateStatusTexts();
                UpdateCommandStates();
            }

            return applied;
        }

        /// <summary>
        /// 쉬움 단계 메타데이터를 현재 ViewModel 상태에 반영한다.
        /// </summary>
        private void ApplyEasyModeMetadata()
        {
            UseTimer = EasyMode.UseTimer;
            RemainingHints = EasyMode.HintCount;
            RemainingSubmitCount = EasyMode.MaxSubmitCount;
            RemainingSeconds = EasyMode.TimeLimitSeconds;
        }

        /// <summary>
        /// 쉬움 단계 첫 조각 고정 처리
        /// </summary>
        private void ApplyEasyFixedPieceIfNeeded(WordOrderQuestion question)
        {
            if (question is null)
            {
                return;
            }

            if (!EasyMode.IsFirstPieceFixed)
            {
                return;
            }

            if (question.CorrectSequence is null || question.CorrectSequence.Count == 0)
            {
                return;
            }

            string firstText = question.CorrectSequence[0];

            WordOrderPieceItem? firstPiece = AvailablePieces
                .FirstOrDefault(x => !x.IsDistractor && string.Equals(x.Text, firstText, StringComparison.Ordinal));

            if (firstPiece is null)
            {
                return;
            }

            AvailablePieces.Remove(firstPiece);
            firstPiece.PlaceAt(0);
            AnswerPieces.Add(firstPiece);

            RefreshPlacedIndexes();
            FeedbackText = "첫 조각이 고정되었습니다. 나머지 순서를 맞춰보세요.";
        }
    }
}