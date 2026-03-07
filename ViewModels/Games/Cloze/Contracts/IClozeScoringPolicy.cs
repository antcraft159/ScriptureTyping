// 파일명: IClozeScoringPolicy.cs
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Contracts
{
    /// <summary>
    /// 목적:
    /// 빈칸 문제 채점 규칙을 정의하는 계약.
    /// 
    /// 역할:
    /// - 제출 답안과 정답 비교
    /// - 정답 여부 및 점수 계산
    /// - 부분 정답 허용 여부 등 정책 분리
    /// </summary>
    public interface IClozeScoringPolicy
    {
        /// <summary>
        /// 목적:
        /// 문제와 제출 답안을 채점하여 라운드 결과를 반환한다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="submittedAnswers">사용자가 제출한 답</param>
        /// <returns>채점 결과</returns>
        ClozeRoundResult Score(ClozeQuestion question, IReadOnlyList<string> submittedAnswers);
    }
}