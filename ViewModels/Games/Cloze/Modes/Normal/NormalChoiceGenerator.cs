// 파일명: NormalChoiceGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Normal
{
    public sealed class NormalChoiceGenerator : IClozeChoiceGenerator
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

                result.Add(new ClozeOptionSet
                {
                    BlankIndex = answer.BlankIndex,
                    CorrectOption = answer.Text,
                    Options = Shuffle(options.ToList())
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
            return items.OrderBy(_ => _random.Next()).ToList();
        }
    }
}