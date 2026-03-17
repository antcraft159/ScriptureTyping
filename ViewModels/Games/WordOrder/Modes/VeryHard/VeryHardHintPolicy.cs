using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 단계의 힌트 동작을 담당한다.
    ///
    /// 규칙:
    /// - 현재 답안 개수 기준 다음 정답 조각 1개를 자동 배치
    /// </summary>
    public sealed class VeryHardHintPolicy : IWordOrderHintPolicy
    {
        public string Difficulty => WordOrderDifficulty.VeryHard;

        public bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (availablePieces is null)
            {
                throw new ArgumentNullException(nameof(availablePieces));
            }

            if (answerPieces is null)
            {
                throw new ArgumentNullException(nameof(answerPieces));
            }

            int nextIndex = answerPieces.Count;

            if (nextIndex < 0 || nextIndex >= question.CorrectSequence.Count)
            {
                message = "더 이상 사용할 힌트가 없습니다.";
                return false;
            }

            string targetText = question.CorrectSequence[nextIndex];

            WordOrderPieceItem? targetPiece = availablePieces
                .FirstOrDefault(x =>
                    !x.IsDistractor &&
                    string.Equals(x.Text, targetText, StringComparison.Ordinal));

            if (targetPiece is null)
            {
                message = "힌트로 배치할 정답 조각을 찾을 수 없습니다.";
                return false;
            }

            availablePieces.Remove(targetPiece);
            targetPiece.PlaceAt(answerPieces.Count);
            answerPieces.Add(targetPiece);

            message = $"힌트를 사용했습니다. {nextIndex + 1}번째 정답 조각이 배치되었습니다.";
            return true;
        }
    }
}