// 파일명: NormalScoringPolicy.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 모드 채점 정책.
    /// 
    /// 규칙:
    /// - 빈칸별 개별 채점
    /// - 부분 정답 허용
    /// - 공백 제거, 대소문자 무시
    /// </summary>
    public sealed class NormalScoringPolicy : IClozeScoringPolicy
    {
        public ClozeRoundResult Score(ClozeQuestion question, IReadOnlyList<string> submittedAnswers)
        {
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            IReadOnlyList<ClozeAnswer> answers = question.Answers ?? Array.Empty<ClozeAnswer>();
            List<string> submitted = (submittedAnswers ?? Array.Empty<string>()).ToList();

            List<bool> perBlank = new List<bool>();
            List<string> correctAnswers = answers.Select(x => x.Text).ToList();

            int correctCount = 0;

            for (int i = 0; i < answers.Count; i++)
            {
                string expected = Normalize(answers[i].Text);
                string actual = i < submitted.Count ? Normalize(submitted[i]) : string.Empty;

                bool isCorrect = string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
                perBlank.Add(isCorrect);

                if (isCorrect)
                {
                    correctCount++;
                }
            }

            bool allCorrect = answers.Count > 0 && correctCount == answers.Count;
            int score = correctCount * 100;

            return new ClozeRoundResult
            {
                IsCorrect = allCorrect,
                CorrectCount = correctCount,
                TotalCount = answers.Count,
                Score = score,
                SubmittedAnswers = submitted,
                CorrectAnswers = correctAnswers,
                PerBlankResults = perBlank,
                Message = allCorrect
                    ? "정답입니다."
                    : $"부분 정답 {correctCount}/{answers.Count}"
            };
        }

        private string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}