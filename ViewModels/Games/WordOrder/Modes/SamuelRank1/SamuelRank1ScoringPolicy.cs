using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// SamuelRank1 난이도의 답안을 채점한다.
    ///
    /// 규칙:
    /// - 조각 수가 정답 순서와 정확히 같아야 한다.
    /// - 각 위치의 텍스트가 정확히 일치해야 한다.
    /// - 방해 조각이 하나라도 포함되면 오답이다.
    /// - 제출 기회는 1회로 제한한다.
    /// </summary>
    public sealed class SamuelRank1ScoringPolicy : IWordOrderScoringPolicy
    {
        /// <summary>
        /// 목적:
        /// 현재 채점 정책이 담당하는 난이도를 반환한다.
        /// </summary>
        public string Difficulty => WordOrderDifficulty.SamuelRank1;

        /// <summary>
        /// 목적:
        /// SamuelRank1 난이도 최대 제출 횟수를 반환한다.
        /// </summary>
        public int MaxSubmitCount => 1;

        /// <summary>
        /// 목적:
        /// 현재 답안이 정답인지 판정한다.
        /// </summary>
        public bool IsCorrect(WordOrderQuestion question, IReadOnlyList<WordOrderPieceItem> answerPieces)
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
                if (answerPieces[i].IsDistractor)
                {
                    return false;
                }

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