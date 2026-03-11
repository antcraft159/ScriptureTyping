using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private bool IsEasyDifficulty()
        {
            return CurrentDifficulty == DIFFICULTY_EASY;
        }

        private int GetEasyBlankCount()
        {
            return 2;
        }

        private int GetEasyChoiceCount()
        {
            return 6;
        }

        private int GetEasyTryCount()
        {
            return 2;
        }

        private int GetEasyCorrectScore()
        {
            return 10;
        }

        private int GetEasyWrongPenalty()
        {
            return 2;
        }

        private bool IsEasyTimeAttack()
        {
            return false;
        }

        private int GetEasyTimeAttackSeconds()
        {
            return 15;
        }

        private List<string> SelectEasyAnswers(IReadOnlyList<string> candidates, int count)
        {
            List<string> pool = candidates.Distinct(StringComparer.Ordinal).ToList();
            Shuffle(pool);
            return pool.Take(count).ToList();
        }

        private IEnumerable<string> GenerateEasyDistractors(string answer)
        {
            List<string> sameLengthWords = _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Where(x => Math.Abs(x.Length - answer.Length) <= 1)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Shuffle(sameLengthWords);

            foreach (string item in sameLengthWords)
            {
                yield return item;
            }
        }

        private List<string> RebuildEasyChoices(string answer, IReadOnlyList<string> originalChoices)
        {
            int requiredCount = GetChoiceCount();

            List<string> rebuilt = new List<string> { answer };

            foreach (string choice in originalChoices)
            {
                if (rebuilt.Count >= requiredCount)
                {
                    break;
                }

                if (string.Equals(choice, answer, StringComparison.Ordinal))
                {
                    continue;
                }

                if (IsTooSimilarForEasy(answer, choice))
                {
                    continue;
                }

                if (rebuilt.Contains(choice, StringComparer.Ordinal))
                {
                    continue;
                }

                rebuilt.Add(choice);
            }

            List<string> fallbackPool = _globalWordPool
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Distinct(StringComparer.Ordinal)
                .Where(word => !string.Equals(word, answer, StringComparison.Ordinal))
                .Where(word => !IsTooSimilarForEasy(answer, word))
                .Where(word => !rebuilt.Contains(word, StringComparer.Ordinal))
                .ToList();

            Shuffle(fallbackPool);

            foreach (string word in fallbackPool)
            {
                if (rebuilt.Count >= requiredCount)
                {
                    break;
                }

                rebuilt.Add(word);
            }

            if (rebuilt.Count < requiredCount)
            {
                return new List<string>();
            }

            Shuffle(rebuilt);
            return rebuilt;
        }

        private bool IsTooSimilarForEasy(string answer, string candidate)
        {
            if (string.IsNullOrWhiteSpace(answer) || string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            string answerCore = NormalizeEasyWord(answer);
            string candidateCore = NormalizeEasyWord(candidate);

            if (string.IsNullOrWhiteSpace(answerCore) || string.IsNullOrWhiteSpace(candidateCore))
            {
                return false;
            }

            if (string.Equals(answerCore, candidateCore, StringComparison.Ordinal))
            {
                return true;
            }

            if (answerCore.Contains(candidateCore, StringComparison.Ordinal) ||
                candidateCore.Contains(answerCore, StringComparison.Ordinal))
            {
                return true;
            }

            int minLength = Math.Min(answerCore.Length, candidateCore.Length);
            if (minLength >= 2 && GetCommonPrefixLength(answerCore, candidateCore) >= minLength - 1)
            {
                return true;
            }

            return false;
        }

        private static string NormalizeEasyWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return string.Empty;
            }

            string normalized = TrimPunctuation(word.Trim());
            normalized = Regex.Replace(normalized, @"[^\p{L}\p{N}]", "");

            string[] particles =
            {
                "으로", "에서", "에게",
                "은", "는", "이", "가", "을", "를",
                "와", "과", "의", "에", "도", "만", "로",
                "나", "이나", "랑", "하고", "께"
            };

            foreach (string particle in particles.OrderByDescending(x => x.Length))
            {
                if (normalized.Length > particle.Length &&
                    normalized.EndsWith(particle, StringComparison.Ordinal))
                {
                    normalized = normalized.Substring(0, normalized.Length - particle.Length);
                    break;
                }
            }

            return normalized;
        }

        private static int GetCommonPrefixLength(string left, string right)
        {
            int max = Math.Min(left.Length, right.Length);
            int count = 0;

            for (int i = 0; i < max; i++)
            {
                if (left[i] != right[i])
                {
                    break;
                }

                count++;
            }

            return count;
        }
    }
}