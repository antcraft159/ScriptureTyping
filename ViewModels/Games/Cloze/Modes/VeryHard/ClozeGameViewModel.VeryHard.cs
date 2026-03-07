using ScriptureTyping.Data;
using ScriptureTyping.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private const int VERY_HARD_BLANK_COUNT = 2;
        private const int VERY_HARD_CHOICE_COUNT = 6;

        private bool IsVeryHardMode =>
            string.Equals(CurrentDifficulty, DIFFICULTY_VERY_HARD, StringComparison.Ordinal);

        private bool TryMakeVeryHardQuestion(VerseItem verse, out ClozeQuestion? question)
        {
            question = null;

            List<string> candidates = ExtractCandidateWords(verse.Text)
                .Distinct(StringComparer.Ordinal)
                .Where(x => x.Length >= 3)
                .ToList();

            if (candidates.Count < VERY_HARD_BLANK_COUNT)
            {
                return false;
            }

            List<string> selectedAnswers = SelectVeryHardAnswers(verse.Text, candidates, VERY_HARD_BLANK_COUNT);
            if (selectedAnswers.Count != VERY_HARD_BLANK_COUNT)
            {
                return false;
            }

            List<string> orderedAnswers = OrderAnswersByAppearance(verse.Text, selectedAnswers);
            if (orderedAnswers.Count != VERY_HARD_BLANK_COUNT)
            {
                return false;
            }

            if (!TryBuildClozeText(verse.Text, orderedAnswers, out string clozeText))
            {
                return false;
            }

            List<IReadOnlyList<string>> choiceSets = new List<IReadOnlyList<string>>();

            foreach (string answer in orderedAnswers)
            {
                List<string> choices = BuildVeryHardChoices(answer);

                if (choices.Count < Math.Min(3, VERY_HARD_CHOICE_COUNT))
                {
                    return false;
                }

                choiceSets.Add(choices);
            }

            question = new ClozeQuestion
            {
                Reference = verse.Ref,
                OriginalText = verse.Text,
                ClozeText = clozeText,
                Answers = orderedAnswers,
                ChoiceSets = choiceSets
            };

            return true;
        }

        private List<string> SelectVeryHardAnswers(string originalText, IReadOnlyList<string> candidates, int blankCount)
        {
            List<string> ordered = candidates
                .Select(word => new
                {
                    Word = word,
                    Index = originalText.IndexOf(word, StringComparison.Ordinal),
                    Score = CalculateVeryHardAnswerPriority(word)
                })
                .Where(x => x.Index >= 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Index)
                .Select(x => x.Word)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (ordered.Count < blankCount)
            {
                return new List<string>();
            }

            return ordered.Take(blankCount).ToList();
        }

        private int CalculateVeryHardAnswerPriority(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return 0;
            }

            int score = 0;

            score += word.Length * 10;

            bool hasSameCharRepeated = word.GroupBy(c => c).Any(g => g.Count() >= 2);
            if (hasSameCharRepeated)
            {
                score += 7;
            }

            bool hasMixedCharTypes =
                word.Any(char.IsLetter) &&
                word.Any(c => !char.IsLetter(c));

            if (hasMixedCharTypes)
            {
                score += 5;
            }

            return score;
        }

        private List<string> BuildVeryHardChoices(string answer)
        {
            HashSet<string> candidateSet = new HashSet<string>(StringComparer.Ordinal);

            foreach (string word in _globalWordPool)
            {
                string candidate = word?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (string.Equals(candidate, answer, StringComparison.Ordinal))
                {
                    continue;
                }

                if (candidate.Length < 2)
                {
                    continue;
                }

                candidateSet.Add(candidate);
            }

            List<string> similarPool = candidateSet
                .Select(word => new
                {
                    Word = word,
                    Score = CalculateVeryHardChoiceSimilarity(answer, word)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Word.Length)
                .ThenBy(x => x.Word, StringComparer.Ordinal)
                .Select(x => x.Word)
                .ToList();

            List<string> choices = new List<string> { answer };

            foreach (string word in similarPool)
            {
                if (choices.Count >= VERY_HARD_CHOICE_COUNT)
                {
                    break;
                }

                if (!choices.Contains(word, StringComparer.Ordinal))
                {
                    choices.Add(word);
                }
            }

            if (choices.Count < VERY_HARD_CHOICE_COUNT)
            {
                foreach (string generated in CreateVeryHardFallbackChoices(answer))
                {
                    if (choices.Count >= VERY_HARD_CHOICE_COUNT)
                    {
                        break;
                    }

                    if (!choices.Contains(generated, StringComparer.Ordinal))
                    {
                        choices.Add(generated);
                    }
                }
            }

            Shuffle(choices);
            return choices;
        }

        private int CalculateVeryHardChoiceSimilarity(string answer, string candidate)
        {
            if (string.IsNullOrWhiteSpace(answer) || string.IsNullOrWhiteSpace(candidate))
            {
                return 0;
            }

            int score = 0;

            if (answer.Length == candidate.Length)
            {
                score += 40;
            }
            else
            {
                int lenDiff = Math.Abs(answer.Length - candidate.Length);
                score += Math.Max(0, 20 - (lenDiff * 5));
            }

            if (answer[0] == candidate[0])
            {
                score += 25;
            }

            if (answer[^1] == candidate[^1])
            {
                score += 20;
            }

            int samePositionCount = 0;
            int compareLength = Math.Min(answer.Length, candidate.Length);

            for (int i = 0; i < compareLength; i++)
            {
                if (answer[i] == candidate[i])
                {
                    samePositionCount++;
                }
            }

            score += samePositionCount * 8;

            int commonCharCount = candidate.Count(c => answer.Contains(c));
            score += commonCharCount * 3;

            if (string.Equals(answer, candidate, StringComparison.OrdinalIgnoreCase))
            {
                score = 0;
            }

            return score;
        }

        private IEnumerable<string> CreateVeryHardFallbackChoices(string answer)
        {
            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);

            if (string.IsNullOrWhiteSpace(answer))
            {
                return results;
            }

            if (answer.Length >= 3)
            {
                string first = answer.Substring(0, 1);
                string middle = answer.Length > 2
                    ? answer.Substring(1, answer.Length - 2)
                    : string.Empty;
                string last = answer.Substring(answer.Length - 1, 1);

                results.Add(first + ReverseString(middle) + last);

                if (middle.Length >= 1)
                {
                    string changedMiddle = ReplaceOneChar(middle);
                    results.Add(first + changedMiddle + last);
                }

                if (answer.Length >= 4)
                {
                    results.Add(answer.Substring(0, answer.Length - 1) + GetNearbyChar(answer[^1]));
                    results.Add(answer.Substring(0, 1) + GetNearbyChar(answer[1]) + answer.Substring(2));
                }
            }

            results.Add(answer + GetNearbyChar(answer[^1]));
            results.Add(GetNearbyChar(answer[0]) + answer.Substring(1));

            results.Remove(answer);
            results.RemoveWhere(string.IsNullOrWhiteSpace);

            return results;
        }

        private static string ReverseString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            char[] chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        private string ReplaceOneChar(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            char[] chars = value.ToCharArray();
            int index = _rng.Next(chars.Length);
            chars[index] = GetNearbyChar(chars[index]);
            return new string(chars);
        }

        private static char GetNearbyChar(char c)
        {
            if (char.IsDigit(c))
            {
                return c == '9' ? '8' : (char)(c + 1);
            }

            if (char.IsUpper(c))
            {
                return c == 'Z' ? 'Y' : (char)(c + 1);
            }

            if (char.IsLower(c))
            {
                return c == 'z' ? 'y' : (char)(c + 1);
            }

            return c;
        }
    }
}