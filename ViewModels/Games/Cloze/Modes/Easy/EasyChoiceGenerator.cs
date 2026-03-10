// 파일명: EasyChoiceGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 모드의 보기 생성기.
    /// 
    /// 역할:
    /// - 빈칸 1개 기준 보기 6개 생성
    /// - 정답 1개를 반드시 포함
    /// - 나머지는 단어 풀에서 중복 없이 추출
    /// </summary>
    public sealed class EasyChoiceGenerator : IClozeChoiceGenerator
    {
        private readonly Random _random = new Random();

        public IReadOnlyList<ClozeOptionSet> GenerateChoices(
            IReadOnlyList<ClozeAnswer> correctAnswers,
            IReadOnlyList<string> wordPool,
            int choiceCountPerBlank)
        {
            if (correctAnswers == null || correctAnswers.Count == 0)
            {
                return Array.Empty<ClozeOptionSet>();
            }

            List<ClozeOptionSet> result = new List<ClozeOptionSet>();

            foreach (ClozeAnswer answer in correctAnswers)
            {
                HashSet<string> options = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    answer.Text
                };

                foreach (string word in Shuffle(wordPool))
                {
                    if (options.Count >= choiceCountPerBlank)
                    {
                        break;
                    }

                    string normalized = NormalizeWord(word);

                    if (string.IsNullOrWhiteSpace(normalized))
                    {
                        continue;
                    }

                    if (string.Equals(normalized, answer.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    options.Add(normalized);
                }

                List<string> shuffledOptions = Shuffle(options.ToList());

                result.Add(new ClozeOptionSet
                {
                    BlankIndex = answer.BlankIndex,
                    CorrectOption = answer.Text,
                    Options = shuffledOptions
                });
            }

            return result;
        }

        private string NormalizeWord(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private List<T> Shuffle<T>(IEnumerable<T> items)
        {
            return items
                .OrderBy(_ => _random.Next())
                .ToList();
        }
    }
}