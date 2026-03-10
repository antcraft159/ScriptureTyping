// 파일명: IClozeMode.cs
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Contracts
{
    /// <summary>
    /// 목적:
    /// 빈칸 채우기 게임의 난이도/모드별 동작을 정의하는 계약.
    /// 
    /// 역할:
    /// - 문제 생성
    /// - 보기 생성
    /// - 정답 채점
    /// 
    /// 의도:
    /// 모드별 규칙(Easy/Normal/Hard/VeryHard 등)을
    /// 하나의 객체로 분리하기 위한 인터페이스.
    /// </summary>
    public interface IClozeMode
    {
        /// <summary>
        /// 현재 모드 이름
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 한 문제에 포함될 빈칸 수
        /// </summary>
        int BlankCount { get; }

        /// <summary>
        /// 빈칸 하나당 보기 개수
        /// </summary>
        int ChoiceCountPerBlank { get; }

        /// <summary>
        /// 목적:
        /// 원본 구절과 단어 풀을 바탕으로 현재 모드 규칙에 맞는 문제를 생성한다.
        /// </summary>
        /// <param name="verseText">원본 구절</param>
        /// <param name="wordPool">보기 생성에 사용할 전체 단어 풀</param>
        /// <returns>생성된 빈칸 문제</returns>
        ClozeQuestion CreateQuestion(string verseText, IReadOnlyList<string> wordPool);

        /// <summary>
        /// 목적:
        /// 사용자의 입력/선택값을 채점하여 결과를 만든다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="submittedAnswers">사용자가 제출한 답</param>
        /// <returns>라운드 결과</returns>
        ClozeRoundResult Score(ClozeQuestion question, IReadOnlyList<string> submittedAnswers);
    }
}