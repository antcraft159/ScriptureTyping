using System;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Models
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기에서 사용하는 조각 1개를 표현한다.
    ///
    /// 역할:
    /// - 조각 텍스트 보관
    /// - 방해 조각 여부 보관
    /// - 현재 답안 칸에 몇 번째로 배치되었는지 관리
    /// </summary>
    public sealed class WordOrderPieceItem
    {
        /// <summary>
        /// 목적:
        /// 조각을 생성한다.
        /// </summary>
        /// <param name="text">조각 텍스트</param>
        /// <param name="isDistractor">방해 조각 여부</param>
        public WordOrderPieceItem(string text, bool isDistractor)
        {
            Text = text ?? string.Empty;
            IsDistractor = isDistractor;
            CurrentPlacedIndex = -1;
        }

        /// <summary>
        /// 조각 텍스트
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 방해 조각 여부
        /// </summary>
        public bool IsDistractor { get; }

        /// <summary>
        /// 현재 답안 칸에 배치된 인덱스
        /// -1이면 아직 배치되지 않음
        /// </summary>
        public int CurrentPlacedIndex { get; private set; }

        /// <summary>
        /// 목적:
        /// 현재 조각을 답안 n번째 칸에 배치된 상태로 표시한다.
        /// </summary>
        public void PlaceAt(int index)
        {
            CurrentPlacedIndex = index;
        }

        /// <summary>
        /// 목적:
        /// 현재 배치 상태를 초기화한다.
        /// </summary>
        public void ResetPlacement()
        {
            CurrentPlacedIndex = -1;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}