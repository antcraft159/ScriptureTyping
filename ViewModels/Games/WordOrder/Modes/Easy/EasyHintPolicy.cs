using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 난이도 힌트 동작을 담당한다.
    ///
    /// 규칙:
    /// - 현재 답안 다음 위치에 들어가야 할 정답 조각 1개를 자동 배치한다.
    /// - AvailablePieces에서 제거하고 AnswerPieces에 추가한다.
    /// </summary>
    public sealed class EasyHintPolicy : IWordOrderHintPolicy
    {
        public string Difficulty => WordOrderDifficulty.Easy;
        public bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message)
        {
            message = string.Empty;

            if (question is null)
            {
                message = "현재 문제가 없습니다.";
                return false;
            }

            if (availablePieces is null || answerPieces is null)
            {
                message = "힌트를 적용할 수 없습니다.";
                return false;
            }

            if (answerPieces.Count >= question.CorrectSequence.Count)
            {
                message = "더 이상 힌트를 사용할 수 없습니다.";
                return false;
            }

            int nextIndex = answerPieces.Count;
            string targetText = question.CorrectSequence[nextIndex];

            WordOrderPieceItem? targetPiece = availablePieces
                .FirstOrDefault(x => !x.IsDistractor && string.Equals(x.Text, targetText, StringComparison.Ordinal));

            if (targetPiece is null)
            {
                message = "사용할 수 있는 힌트 조각이 없습니다.";
                return false;
            }

            availablePieces.Remove(targetPiece);
            answerPieces.Add(targetPiece);

            targetPiece.PlaceAt(nextIndex);

            message = $"힌트를 사용했습니다. {nextIndex + 1}번째 조각이 배치되었습니다.";
            return true;
        }
    }
}