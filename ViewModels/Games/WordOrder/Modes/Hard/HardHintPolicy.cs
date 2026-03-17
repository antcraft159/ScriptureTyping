using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// Hard 난이도 힌트 동작을 처리한다.
    ///
    /// 규칙:
    /// - 현재 AnswerPieces.Count 위치에 들어가야 할 정답 조각 1개를 자동 배치한다
    /// - AvailablePieces 안에서 정확한 정답 조각을 찾아 AnswerPieces로 이동시킨다
    /// </summary>
    public sealed class HardHintPolicy : IWordOrderHintPolicy
    {
        /// <summary>
        /// 목적:
        /// 현재 힌트 정책이 담당하는 난이도를 나타낸다.
        /// </summary>
        public string Difficulty => WordOrderDifficulty.Hard;

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
                message = "더 이상 사용할 수 있는 힌트가 없습니다.";
                return false;
            }

            string correctText = question.CorrectSequence[nextIndex];

            WordOrderPieceItem? targetPiece = availablePieces
                .FirstOrDefault(x =>
                    !x.IsDistractor &&
                    string.Equals(x.Text, correctText, StringComparison.Ordinal));

            if (targetPiece is null)
            {
                message = "힌트에 사용할 정답 조각을 찾을 수 없습니다.";
                return false;
            }

            availablePieces.Remove(targetPiece);
            targetPiece.PlaceAt(answerPieces.Count);
            answerPieces.Add(targetPiece);

            message = $"{nextIndex + 1}번째 정답 조각 힌트를 적용했습니다.";
            return true;
        }
    }
}