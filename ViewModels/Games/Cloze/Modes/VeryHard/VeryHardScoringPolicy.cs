// 파일명: VeryHard/VeryHardScoringPolicy.cs
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 모드의 주관식 채점 정책.
    /// 
    /// 규칙:
    /// - 공백 Trim
    /// - 대소문자 무시
    /// - 완전 일치만 정답
    /// </summary>
    public sealed class VeryHardScoringPolicy : IClozeScoringPolicy
    {
        public ClozeRoundResult Score(ClozeQuestion question, IReadOnlyList<string> submittedAnswers)
        {
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            IReadOnlyList<ClozeAnswer> answers = question.Answers ?? Array.Empty<ClozeAnswer>();
            List<string> submitted = (submittedAnswers ?? Array.Empty<string>()).ToList();

            List<InputJudge> judges = new List<InputJudge>();
            List<bool> perBlankResults = new List<bool>();

            int correctCount = 0;

            for (int i = 0; i < answers.Count; i++)
            {
                string expected = Normalize(answers[i].Text);
                string actual = i < submitted.Count ? Normalize(submitted[i]) : string.Empty;

                bool isCorrect = string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);

                judges.Add(new InputJudge
                {
                    BlankIndex = i,
                    Expected = answers[i].Text,
                    Submitted = i < submitted.Count ? submitted[i] : string.Empty,
                    IsCorrect = isCorrect
                });

                perBlankResults.Add(isCorrect);

                if (isCorrect)
                {
                    correctCount++;
                }
            }

            bool allCorrect = answers.Count > 0 && correctCount == answers.Count;

            return new ClozeRoundResult
            {
                IsCorrect = allCorrect,
                CorrectCount = correctCount,
                TotalCount = answers.Count,
                Score = correctCount * 100,
                SubmittedAnswers = submitted,
                CorrectAnswers = answers.Select(x => x.Text).ToList(),
                PerBlankResults = perBlankResults,
                Message = allCorrect
                    ? "정답입니다."
                    : $"매우 어려움 결과: {correctCount}/{answers.Count}"
            };
        }

        private string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}