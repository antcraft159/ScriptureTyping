using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Contracts
{
    /// <summary>
    /// 목적:
    /// 말씀과 난이도 규칙을 기반으로 순서 맞추기 문제를 생성한다.
    /// </summary>
    public interface IWordOrderQuestionGenerator
    {
        /// <summary>
        /// 목적:
        /// 현재 생성기가 담당하는 난이도를 나타낸다.
        /// </summary>
        string Difficulty { get; }

        /// <summary>
        /// 목적:
        /// 말씀과 조각 생성 규칙을 바탕으로 순서 맞추기 문제를 생성한다.
        /// </summary>
        /// <param name="verse">문제 원본 말씀</param>
        /// <param name="sourceVerses">방해 조각 생성을 위한 전체 말씀 목록</param>
        /// <param name="pieceBuilder">조각 생성 담당 객체</param>
        /// <param name="hintCount">허용 힌트 수</param>
        /// <param name="useTimer">타이머 사용 여부</param>
        /// <param name="timeLimitSeconds">제한 시간(초)</param>
        /// <param name="isFirstPieceFixed">첫 조각 고정 여부</param>
        /// <returns>생성된 문제 객체</returns>
        WordOrderQuestion Generate(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IWordOrderPieceBuilder pieceBuilder,
            int hintCount,
            bool useTimer,
            int timeLimitSeconds,
            bool isFirstPieceFixed);
    }
}