// 파일명: IClozeChoiceGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Contracts
{
    /// <summary>
    /// 목적:
    /// 빈칸 문제에 사용할 보기(선택지) 목록을 생성하는 계약.
    /// 
    /// 역할:
    /// - 정답 단어를 포함한 보기 리스트 생성
    /// - 난이도별로 보기 생성 전략을 다르게 구현 가능
    /// 
    /// 예:
    /// - 쉬움: 정답 1개 + 오답 5개
    /// - 보통: 빈칸 2개 각각 보기 생성
    /// - 어려움: 비슷한 접미사/철자 오답 포함
    /// </summary>
    public interface IClozeChoiceGenerator
    {
        /// <summary>
        /// 목적:
        /// 주어진 정답 후보와 단어 풀을 바탕으로 보기 세트를 생성한다.
        /// </summary>
        /// <param name="correctAnswers">현재 문제의 정답들</param>
        /// <param name="wordPool">보기 생성에 사용할 전체 단어 풀</param>
        /// <param name="choiceCountPerBlank">빈칸 하나당 만들 보기 개수</param>
        /// <returns>빈칸별 보기 세트 목록</returns>
        IReadOnlyList<ClozeOptionSet> GenerateChoices(
            IReadOnlyList<ClozeAnswer> correctAnswers,
            IReadOnlyList<string> wordPool,
            int choiceCountPerBlank);
    }
}