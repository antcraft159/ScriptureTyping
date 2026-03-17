using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 단계의 힌트 동작을 처리한다.
    ///
    /// 규칙:
    /// - 현재 답안 길이 위치에 들어가야 할 정답 조각 1개를 자동 배치한다.
    /// - 이미 정답 조각이 사용되었거나 찾을 수 없으면 실패한다.
    /// </summary>
    public sealed class NormalHintPolicy : IWordOrderHintPolicy
    {
        public string Difficulty => WordOrderDifficulty.Normal;

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
                message = "더 이상 힌트를 사용할 수 없습니다.";
                return false;
            }

            string targetText = question.CorrectSequence[nextIndex];

            WordOrderPieceItem? targetPiece = availablePieces
                .FirstOrDefault(x =>
                    !x.IsDistractor &&
                    string.Equals(x.Text, targetText, StringComparison.Ordinal));

            if (targetPiece is null)
            {
                message = "사용 가능한 힌트 조각이 없습니다.";
                return false;
            }

            availablePieces.Remove(targetPiece);
            answerPieces.Add(targetPiece);

            for (int i = 0; i < answerPieces.Count; i++)
            {
                answerPieces[i].PlaceAt(i);
            }

            message = $"{nextIndex + 1}번째 조각이 배치되었습니다.";
            return true;
        }
    }
}