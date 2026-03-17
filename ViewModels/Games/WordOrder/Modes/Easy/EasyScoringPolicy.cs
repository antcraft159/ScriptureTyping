using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 난이도 정답 판정을 담당한다.
    ///
    /// 규칙:
    /// - 조각 개수가 정답 개수와 같아야 한다.
    /// - 순서가 정확히 일치해야 한다.
    /// - 방해 조각이 하나라도 있으면 오답이다.
    /// </summary>
    public sealed class EasyScoringPolicy :IWordOrderScoringPolicy
    {
        public string Difficulty => WordOrderDifficulty.Easy;
        public bool IsCorrect(WordOrderQuestion question, IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            if (question is null)
            {
                return false;
            }

            if (answerPieces is null)
            {
                return false;
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
                if (!string.Equals(answerPieces[i].Text, question.CorrectSequence[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public string BuildWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor)
        {
            if (question is null)
            {
                return "오답입니다.";
            }

            if (containsDistractor)
            {
                return "오답입니다. 방해 조각이 포함되어 있습니다.";
            }

            if (answerPieces is null || answerPieces.Count != question.CorrectSequence.Count)
            {
                return $"오답입니다. 정답 칸을 모두 채워야 합니다. ({answerPieces?.Count ?? 0}/{question.CorrectSequence.Count})";
            }

            int mismatchIndex = FindFirstMismatchIndex(question, answerPieces);
            if (mismatchIndex >= 0)
            {
                return $"오답입니다. {mismatchIndex + 1}번째 조각부터 순서를 다시 확인해 보세요.";
            }

            return "오답입니다. 순서를 다시 확인해 보세요.";
        }

        private static int FindFirstMismatchIndex(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            for (int i = 0; i < question.CorrectSequence.Count; i++)
            {
                if (!string.Equals(answerPieces[i].Text, question.CorrectSequence[i], StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}