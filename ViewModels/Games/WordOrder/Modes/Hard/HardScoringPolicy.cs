using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// Hard 난이도 답안을 채점한다.
    ///
    /// 규칙:
    /// - 조각 수가 정답 순서와 정확히 같아야 한다
    /// - 각 위치의 텍스트가 정확히 일치해야 한다
    /// - 방해 조각이 포함되면 오답이다
    /// </summary>
    public sealed class HardScoringPolicy : IWordOrderScoringPolicy
    {
        /// <summary>
        /// 현재 채점 정책이 담당하는 난이도명
        /// </summary>
        public string Difficulty => WordOrderDifficulty.Hard;

        /// <summary>
        /// 현재 답안이 정답인지 판정한다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="answerPieces">사용자가 배치한 답 조각 목록</param>
        /// <returns>정답이면 true, 아니면 false</returns>
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

            for (int i = 0; i < answerPieces.Count; i++)
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