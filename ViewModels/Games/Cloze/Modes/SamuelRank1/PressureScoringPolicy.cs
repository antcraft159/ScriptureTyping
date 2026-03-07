// 파일명: PressureScoringPolicy.cs
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// 상위 난이도용 압박형 채점 정책.
    /// 
    /// 특징:
    /// - 부분 정답은 인정하지만 메시지를 더 엄격하게 줌
    /// - 전체 정답이 아니면 실패 압박감을 주는 용도
    /// </summary>
    public sealed class PressureScoringPolicy : IClozeScoringPolicy
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

            string message;
            if (allCorrect)
            {
                message = "정답입니다. 압박 모드 통과.";
            }
            else if (correctCount == 0)
            {
                message = "오답입니다. 다시 집중해서 입력하세요.";
            }
            else
            {
                message = $"아쉽습니다. {correctCount}/{answers.Count} 정답.";
            }

            return new ClozeRoundResult
            {
                IsCorrect = allCorrect,
                CorrectCount = correctCount,
                TotalCount = answers.Count,
                Score = allCorrect ? answers.Count * 120 : correctCount * 80,
                SubmittedAnswers = submitted,
                CorrectAnswers = answers.Select(x => x.Text).ToList(),
                PerBlankResults = perBlank,
                Message = message
            };
        }

        private string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}