using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Contracts
{
    /// <summary>
    /// 목적:
    /// 말씀과 조각 생성기를 바탕으로 순서 맞추기 문제를 생성한다.
    /// </summary>
    public interface IWordOrderQuestionGenerator
    {
        /// <summary>
        /// 담당 난이도
        /// </summary>
        string Difficulty { get; }

        /// <summary>
        /// 목적:
        /// 입력 말씀을 기반으로 문제 1개를 생성한다.
        /// </summary>
        WordOrderQuestion Generate(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IWordOrderPieceBuilder pieceBuilder);
    }
}