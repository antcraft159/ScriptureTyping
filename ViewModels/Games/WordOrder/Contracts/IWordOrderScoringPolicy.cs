using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Contracts
{
    /// <summary>
    /// 목적:
    /// 사용자가 배치한 답안 조각이 정답인지 판정하는 규칙을 정의한다.
    ///
    /// 역할:
    /// - 정답 순서 일치 여부 판정
    /// - 방해 조각 포함 여부 반영
    /// - 필요 시 부분 정답/엄격 채점 등 확장 가능
    ///
    /// 입력:
    /// - question: 현재 문제
    /// - answerPieces: 사용자가 배치한 답 조각 목록
    ///
    /// 출력:
    /// - 정답 여부(true/false)
    ///
    /// 주의사항:
    /// - 기본 구현은 순서와 개수가 완전히 일치해야 정답 처리하는 방식이 될 가능성이 높다.
    /// - 난이도별로 채점 기준이 다르면 구현체를 분리한다.
    /// </summary>
    public interface IWordOrderScoringPolicy
    {
        /// <summary>
        /// 현재 채점 정책이 담당하는 난이도명
        /// </summary>
        string Difficulty { get; }

        /// <summary>
        /// 현재 답안이 정답인지 판정한다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="answerPieces">사용자가 배치한 답 조각 목록</param>
        bool IsCorrect(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces);
    }
}