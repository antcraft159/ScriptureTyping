using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Models
{
    /// <summary>
    /// 목적:
    /// 사용자가 한 문제에 대해 제출한 답안 정보를 표현한다.
    ///
    /// 주요 역할:
    /// - 사용자가 배치한 조각 순서 보관
    /// - 방해 조각 포함 여부 보관
    /// - 정답 여부 계산에 필요한 원본 텍스트 배열 제공
    ///
    /// 주의사항:
    /// - SubmittedSequence는 제출 시점의 스냅샷으로 다루는 것이 좋다.
    /// </summary>
    public sealed class WordOrderAnswer
    {
        /// <summary>
        /// 목적:
        /// 사용자가 제출한 조각 텍스트 순서를 보관한다.
        /// </summary>
        public IReadOnlyList<string> SubmittedSequence { get; init; } = Array.Empty<string>();

        /// <summary>
        /// 목적:
        /// 사용자가 제출한 조각에 방해 조각이 포함되어 있는지 여부를 보관한다.
        /// </summary>
        public bool ContainsDistractor { get; init; }

        /// <summary>
        /// 목적:
        /// 제출된 조각 개수를 반환한다.
        /// </summary>
        public int Count => SubmittedSequence.Count;

        /// <summary>
        /// 목적:
        /// 조각 아이템 목록으로부터 답안 객체를 생성한다.
        ///
        /// 입력:
        /// - pieces: 사용자가 배치한 조각 목록
        ///
        /// 출력:
        /// - WordOrderAnswer
        /// </summary>
        public static WordOrderAnswer FromPieces(IEnumerable<WordOrderPieceItem> pieces)
        {
            if (pieces is null)
            {
                return new WordOrderAnswer();
            }

            List<WordOrderPieceItem> pieceList = pieces.ToList();

            return new WordOrderAnswer
            {
                SubmittedSequence = pieceList
                    .Select(piece => piece.Text)
                    .ToList(),
                ContainsDistractor = pieceList.Any(piece => piece.IsDistractor)
            };
        }
    }
}