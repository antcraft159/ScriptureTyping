using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 단계 답안을 채점한다.
    ///
    /// 규칙:
    /// - 조각 수가 정답 순서와 정확히 같아야 한다.
    /// - 각 위치의 텍스트가 정확히 일치해야 한다.
    /// - 방해 조각이 포함되면 오답이다.
    /// </summary>
    public sealed class NormalScoringPolicy : IWordOrderScoringPolicy
    {
        public string Difficulty => WordOrderDifficulty.Normal;

        public bool IsCorrect(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (answerPieces is null)
            {
                throw new ArgumentNullException(nameof(answerPieces));
            }

            if (answerPieces.Count != question.CorrectSequence.Count)
            {
                return false;
            }

            for (int i = 0; i < answerPieces.Count; i++)
            {
                WordOrderPieceItem piece = answerPieces[i];

                if (piece is null)
                {
                    return false;
                }

                if (piece.IsDistractor)
                {
                    return false;
                }

                if (!string.Equals(
                    piece.Text,
                    question.CorrectSequence[i],
                    StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
    }
}