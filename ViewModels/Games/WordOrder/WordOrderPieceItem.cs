namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임에서 사용하는 조각 1개의 상태를 보관한다.
    ///
    /// 역할:
    /// - 조각 텍스트 보관
    /// - 정답 순서 정보 보관
    /// - 방해 조각 여부 보관
    /// - 현재 답안 영역에 배치되었는지 상태 관리
    /// - 현재 배치된 인덱스 관리
    /// </summary>
    public sealed class WordOrderPieceItem
    {
        /// <summary>
        /// 조각에 표시할 텍스트
        /// </summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// 정답일 때 원래 순서
        /// </summary>
        public int CorrectOrder { get; init; }

        /// <summary>
        /// 방해 조각 여부
        /// </summary>
        public bool IsDistractor { get; init; }

        /// <summary>
        /// 현재 답안 영역에 배치되었는지 여부
        /// </summary>
        public bool IsPlaced { get; private set; }

        /// <summary>
        /// 현재 답안 영역에 배치된 위치
        /// 배치되지 않았으면 -1
        /// </summary>
        public int CurrentPlacedIndex { get; private set; } = -1;

        /// <summary>
        /// 목적:
        /// 조각을 답안 영역의 지정된 위치에 배치 상태로 변경한다.
        /// </summary>
        public void PlaceAt(int placedIndex)
        {
            IsPlaced = true;
            CurrentPlacedIndex = placedIndex;
        }

        /// <summary>
        /// 목적:
        /// 조각을 미배치 상태로 되돌린다.
        /// </summary>
        public void ResetPlacement()
        {
            IsPlaced = false;
            CurrentPlacedIndex = -1;
        }
    }
}