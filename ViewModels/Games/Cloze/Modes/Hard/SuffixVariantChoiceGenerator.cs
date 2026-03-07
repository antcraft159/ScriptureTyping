// 파일명: SuffixVariantChoiceGenerator.cs
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// 어려움 모드용 보기 생성기.
    /// 
    /// 특징:
    /// - 정답과 유사한 접미사 변형 오답 생성
    /// - 부족한 경우 wordPool에서 추가
    /// </summary>
    public sealed class SuffixVariantChoiceGenerator : IClozeChoiceGenerator
    {
        private static readonly string[] Suffixes =
        {
            "이", "가", "은", "는", "을", "를", "와", "과", "도", "만", "의", "로", "에", "께", "함"
        };

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

                foreach (string variant in BuildVariants(answer.Text))
                {
                    if (options.Count >= choiceCountPerBlank)
                    {
                        break;
                    }

                    if (!string.Equals(variant, answer.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(variant);
                    }
                }

                foreach (string poolWord in Shuffle(wordPool))
                {
                    if (options.Count >= choiceCountPerBlank)
                    {
                        break;
                    }

                    string normalized = Normalize(poolWord);

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

        private IEnumerable<string> BuildVariants(string answer)
        {
            string baseWord = Normalize(answer);

            foreach (string suffix in Shuffle(Suffixes))
            {
                yield return baseWord + suffix;
            }

            if (baseWord.Length >= 2)
            {
                yield return baseWord.Substring(0, baseWord.Length - 1);
            }

            if (baseWord.Length >= 1)
            {
                yield return baseWord + baseWord[^1];
            }
        }

        private string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private List<T> Shuffle<T>(IEnumerable<T> items)
        {
            return items.OrderBy(_ => _random.Next()).ToList();
        }
    }
}