using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 단계의 정답 판정을 담당한다.
    /// </summary>
    public sealed class VeryHardScoringPolicy : IWordOrderScoringPolicy
    {
        public string Difficulty => WordOrderDifficulty.VeryHard;

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

            if (answerPieces.Any(x => x.IsDistractor))
            {
                return false;
            }

            for (int i = 0; i < question.CorrectSequence.Count; i++)
            {
                if (!string.Equals(
                    answerPieces[i].Text,
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