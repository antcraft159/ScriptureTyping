using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// SamuelRank1 난이도의 힌트 사용 규칙을 정의한다.
    ///
    /// 규칙:
    /// - SamuelRank1은 매우 어려움 기반 상위 단계이므로 힌트를 허용하지 않는다.
    /// - 힌트는 항상 실패 처리하고 안내 메시지만 반환한다.
    /// </summary>
    public sealed class SamuelRank1HintPolicy : IWordOrderHintPolicy
    {
        /// <summary>
        /// 현재 힌트 정책이 담당하는 난이도명
        /// </summary>
        public string Difficulty => WordOrderDifficulty.SamuelRank1;

        /// <summary>
        /// 목적:
        /// SamuelRank1 난이도에서는 힌트를 적용하지 않는다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="availablePieces">선택 가능한 보기 조각 목록</param>
        /// <param name="answerPieces">현재 사용자가 배치한 답안 조각 목록</param>
        /// <param name="message">사용자 안내 메시지</param>
        /// <returns>항상 false</returns>
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

            message = "사무엘 1등 단계는 힌트를 사용할 수 없습니다.";
            return false;
        }
    }
}