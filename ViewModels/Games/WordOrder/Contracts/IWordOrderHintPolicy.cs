using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Contracts
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임에서 힌트를 어떻게 적용할지 정의한다.
    ///
    /// 역할:
    /// - 다음 정답 조각 자동 배치
    /// - 특정 위치 조각 공개
    /// - 난이도별 힌트 동작 차등 적용
    ///
    /// 입력:
    /// - question: 현재 문제
    /// - availablePieces: 현재 선택 가능한 남은 조각 목록
    /// - answerPieces: 현재 사용자가 쌓은 답안 조각 목록
    ///
    /// 출력:
    /// - 힌트 적용 성공 여부
    /// - 사용자에게 보여줄 메시지
    ///
    /// 주의사항:
    /// - 실제 힌트 차감은 ViewModel에서 할지 정책 내부에서 할지 기준을 정해서 일관되게 유지해야 한다.
    /// - 여기서는 "힌트 동작 자체"만 담당하고, 남은 힌트 수 관리 책임은 보통 ViewModel이 가지는 편이 단순하다.
    /// </summary>
    public interface IWordOrderHintPolicy
    {
        /// <summary>
        /// 현재 힌트 정책이 담당하는 난이도명
        /// </summary>
        string Difficulty { get; }

        /// <summary>
        /// 힌트를 적용한다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="availablePieces">현재 남아 있는 보기 조각 목록</param>
        /// <param name="answerPieces">현재 배치된 답안 조각 목록</param>
        /// <param name="message">힌트 적용 결과 메시지</param>
        /// <returns>힌트 적용 성공 여부</returns>
        bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message);
    }
}