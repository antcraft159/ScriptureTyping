using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Contracts
{
    /// <summary>
    /// 목적:
    /// 말씀 텍스트를 순서 맞추기용 조각으로 분해하고,
    /// 필요 시 방해 조각까지 함께 구성하는 역할을 정의한다.
    ///
    /// 역할:
    /// - 원문을 문제 조각 단위로 분리
    /// - 정답 순서 텍스트 목록 생성
    /// - 표시용 조각(WordOrderPieceItem) 생성
    /// - 방해 조각 후보 생성
    ///
    /// 입력:
    /// - verse: 문제를 만들 원본 말씀
    /// - sourceVerses: 방해 조각 참고용 전체 말씀 목록
    ///
    /// 출력:
    /// - 정답 순서 문자열 목록
    /// - 화면에 표시할 조각 목록
    ///
    /// 주의사항:
    /// - 쉬움은 긴 덩어리, 어려움은 짧은 단위 등 난이도별 분해 방식이 달라질 수 있다.
    /// - 방해 조각 생성 규칙도 구현체마다 달라질 수 있다.
    /// </summary>
    public interface IWordOrderPieceBuilder
    {
        /// <summary>
        /// 현재 조각 생성기가 담당하는 난이도명
        /// </summary>
        string Difficulty { get; }

        /// <summary>
        /// 원문 말씀을 정답 순서 기준의 문자열 목록으로 분해한다.
        /// </summary>
        /// <param name="verse">원본 말씀</param>
        IReadOnlyList<string> BuildCorrectSequence(Verse verse);

        /// <summary>
        /// 실제 화면 표시용 조각 목록을 생성한다.
        /// 보통 정답 조각 + 방해 조각 + 셔플까지 포함할 수 있다.
        /// </summary>
        /// <param name="verse">원본 말씀</param>
        /// <param name="sourceVerses">방해 조각 후보용 전체 말씀</param>
        IReadOnlyList<WordOrderPieceItem> BuildPieces(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IReadOnlyList<string> correctSequence);

        /// <summary>
        /// 방해 조각 후보 문자열 목록을 생성한다.
        /// 필요하지 않으면 빈 목록을 반환하면 된다.
        /// </summary>
        /// <param name="verse">원본 말씀</param>
        /// <param name="sourceVerses">방해 조각 후보용 전체 말씀</param>
        IReadOnlyList<string> BuildDistractorTexts(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses);
    }
}