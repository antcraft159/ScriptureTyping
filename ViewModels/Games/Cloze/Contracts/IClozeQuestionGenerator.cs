// 파일명: IClozeQuestionGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Contracts
{
    /// <summary>
    /// 목적:
    /// 원본 문장에서 빈칸 문제를 만들어내는 계약.
    /// 
    /// 역할:
    /// - 어떤 단어를 가릴지 결정
    /// - 표시용 문장(masked text) 생성
    /// - 정답 정보(ClozeAnswer) 구성
    /// 
    /// 예:
    /// "태초에 하나님이 천지를 창조하시니라"
    /// -> "태초에 ____이 천지를 창조하시니라"
    /// </summary>
    public interface IClozeQuestionGenerator
    {
        /// <summary>
        /// 목적:
        /// 주어진 원본 텍스트에서 빈칸 문제를 생성한다.
        /// </summary>
        /// <param name="sourceText">원본 텍스트</param>
        /// <param name="blankCount">가릴 단어 수</param>
        /// <param name="wordPool">보기 생성용 전체 단어 풀</param>
        /// <returns>빈칸 문제</returns>
        ClozeQuestion Generate(
            string sourceText,
            int blankCount,
            IReadOnlyList<string> wordPool);
    }
}