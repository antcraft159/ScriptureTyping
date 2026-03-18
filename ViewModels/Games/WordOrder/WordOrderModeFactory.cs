// 파일명: ViewModels/Games/WordOrder/WordOrderModeFactory.cs
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.Hard;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.SamuelRank1;
using ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard;
using System;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 난이도 문자열에 맞는 순서 맞추기 모드 객체를 생성한다.
    ///
    /// 입력:
    /// - difficulty: 쉬움/보통/어려움/매우 어려움
    ///
    /// 출력:
    /// - IWordOrderMode 구현 객체
    ///
    /// 주의사항:
    /// - 지원하지 않는 난이도가 들어오면 기본값으로 쉬움 모드를 반환한다.
    /// - ViewModel은 이 팩토리를 통해 모드 객체를 받아 사용하고,
    ///   각 난이도별 세부 규칙은 모드 클래스 내부에서 처리한다.
    /// </summary>
    public sealed class WordOrderModeFactory
    {
        /// <summary>
        /// 목적:
        /// 난이도에 맞는 순서 맞추기 모드 객체를 생성한다.
        /// </summary>
        /// <param name="difficulty">선택된 난이도 문자열</param>
        /// <returns>난이도에 대응하는 IWordOrderMode 구현 객체</returns>
        public IWordOrderMode Create(string? difficulty)
        {
            string normalizedDifficulty = string.IsNullOrWhiteSpace(difficulty)
                ? WordOrderDifficulty.Easy
                : difficulty.Trim();

            return normalizedDifficulty switch
            {
                WordOrderDifficulty.Easy => new EasyWordOrderMode(),
                WordOrderDifficulty.Normal => new NormalWordOrderMode(),
                WordOrderDifficulty.Hard => new HardWordOrderMode(),
                WordOrderDifficulty.VeryHard => new VeryHardWordOrderMode(),
                WordOrderDifficulty.SamuelRank1 => new SamuelRank1WordOrderMode(),
                _ => new EasyWordOrderMode()
            };
        }
    }
}