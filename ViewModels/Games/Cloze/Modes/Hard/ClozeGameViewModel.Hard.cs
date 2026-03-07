using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private static readonly string[] KnownJosa =
        {
            "으로부터", "에게서", "께서는", "에서는", "으로는", "으로도",
            "이었다", "이니라", "이라는", "이라도",
            "으로", "에게", "께서", "에서", "이다",
            "이나", "나마", "처럼", "같이",
            "이", "가", "은", "는", "을", "를",
            "와", "과", "도", "만", "의", "로", "에", "께"
        };

        private static readonly string[] EndingPatterns =
        {
            "하였느니라", "하였더라", "하였도다",
            "되었느니라", "되었더라", "되었도다",
            "하시느니라", "하시니라", "하느니라", "하더라", "하도다", "하노라",
            "되느니라", "되니라", "되리라", "되더라", "되도다",
            "느니라", "이니라", "이로다",
            "으리라", "리라", "니라", "더라", "도다", "노라", "이라",
            "하라", "하여라", "하니", "하되", "하고",
            "이다", "이니", "이며"
        };

        private static readonly string[] ReplacementEndings =
        {
            "느니라", "이니라", "이로다",
            "으리라", "리라", "니라", "더라", "도다", "노라", "이라",
            "하라", "하여라", "하니", "하되", "하고",
            "이다", "이니", "이며"
        };

        private List<string> BuildChoices(string answer)
        {
            int choiceCount = GetChoiceCount();

            HashSet<string> set = new HashSet<string>(StringComparer.Ordinal)
            {
                answer
            };

            foreach (string distractor in GenerateDistractors(answer))
            {
                if (set.Count >= choiceCount)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(distractor))
                {
                    continue;
                }

                if (string.Equals(distractor, answer, StringComparison.Ordinal))
                {
                    continue;
                }

                set.Add(distractor);
            }

            bool allowGlobalFallback =
                CurrentDifficulty == DIFFICULTY_EASY ||
                CurrentDifficulty == DIFFICULTY_NORMAL;

            if (allowGlobalFallback)
            {
                int safety = 0;

                while (set.Count < choiceCount && safety < 300)
                {
                    safety++;

                    if (_globalWordPool.Count <= 0)
                    {
                        break;
                    }

                    string word = _globalWordPool[_rng.Next(_globalWordPool.Count)];

                    if (string.IsNullOrWhiteSpace(word) ||
                        string.Equals(word, answer, StringComparison.Ordinal) ||
                        !IsValidChoiceWord(word))
                    {
                        continue;
                    }

                    set.Add(word);
                }
            }
            else
            {
                foreach (string extra in GenerateExtendedHardDistractors(answer))
                {
                    if (set.Count >= choiceCount)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(extra) ||
                        string.Equals(extra, answer, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    set.Add(extra);
                }
            }

            List<string> list = set.ToList();
            Shuffle(list);
            return list.Take(choiceCount).ToList();
        }

        private IEnumerable<string> GenerateDistractors(string answer)
        {
            List<string> sameLengthWords = _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Where(x => Math.Abs(x.Length - answer.Length) <= 1)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (CurrentDifficulty == DIFFICULTY_EASY)
            {
                Shuffle(sameLengthWords);

                foreach (string item in sameLengthWords)
                {
                    yield return item;
                }

                yield break;
            }

            if (CurrentDifficulty == DIFFICULTY_NORMAL)
            {
                List<string> normalWords = sameLengthWords
                    .OrderBy(x => Math.Abs(x.Length - answer.Length))
                    .ThenByDescending(x => SharedPrefixScore(answer, x))
                    .ToList();

                foreach (string item in normalWords)
                {
                    yield return item;
                }

                yield break;
            }

            foreach (string variant in BuildSuffixVariants(answer))
            {
                yield return variant;
            }
        }

        private IEnumerable<string> GenerateExtendedHardDistractors(string answer)
        {
            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);

            foreach (string item in BuildEndingVariants(answer))
            {
                results.Add(item);
            }

            foreach (string item in BuildJosaLikeVariants(answer))
            {
                results.Add(item);
            }

            foreach (string item in BuildStemSimilarWords(answer))
            {
                results.Add(item);
            }

            foreach (string item in results)
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildSuffixVariants(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                yield break;
            }

            string normalized = answer.Trim();
            string stem = RemoveKnownJosa(normalized, out string? originalJosa);

            if (!string.IsNullOrWhiteSpace(originalJosa) && !string.IsNullOrWhiteSpace(stem))
            {
                foreach (string variant in BuildJosaVariants(stem))
                {
                    if (!string.Equals(variant, normalized, StringComparison.Ordinal))
                    {
                        yield return variant;
                    }
                }

                yield break;
            }

            bool yielded = false;

            foreach (string variant in BuildEndingVariants(normalized))
            {
                if (!string.Equals(variant, normalized, StringComparison.Ordinal))
                {
                    yielded = true;
                    yield return variant;
                }
            }

            if (!yielded)
            {
                foreach (string word in BuildStemSimilarWords(normalized))
                {
                    if (!string.Equals(word, normalized, StringComparison.Ordinal))
                    {
                        yield return word;
                    }
                }
            }
        }

        private IEnumerable<string> BuildEndingVariants(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                yield break;
            }

            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);
            string normalized = answer.Trim();

            foreach (string ending in EndingPatterns.OrderByDescending(x => x.Length))
            {
                if (normalized.Length > ending.Length && normalized.EndsWith(ending, StringComparison.Ordinal))
                {
                    string stem = normalized.Substring(0, normalized.Length - ending.Length);

                    foreach (string newEnding in ReplacementEndings)
                    {
                        string candidate = stem + newEnding;

                        if (!string.Equals(candidate, normalized, StringComparison.Ordinal) &&
                            IsValidChoiceWord(candidate))
                        {
                            results.Add(candidate);
                        }
                    }

                    break;
                }
            }

            foreach (string item in results)
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildJosaLikeVariants(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                yield break;
            }

            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);
            string normalized = answer.Trim();
            string stem = RemoveKnownJosa(normalized, out string? originalJosa);

            if (!string.IsNullOrWhiteSpace(originalJosa) && !string.IsNullOrWhiteSpace(stem))
            {
                foreach (string item in BuildJosaVariants(stem))
                {
                    results.Add(item);
                }
            }
            else if (normalized.Length >= 2 && IsMostlyNounLike(normalized))
            {
                bool hasBatchim = HasFinalConsonant(normalized);

                results.Add(normalized + (hasBatchim ? "이" : "가"));
                results.Add(normalized + (hasBatchim ? "은" : "는"));
                results.Add(normalized + (hasBatchim ? "을" : "를"));
                results.Add(normalized + (hasBatchim ? "과" : "와"));
                results.Add(normalized + "의");
                results.Add(normalized + "도");
                results.Add(normalized + "만");
            }

            foreach (string item in results)
            {
                if (!string.Equals(item, normalized, StringComparison.Ordinal) &&
                    IsValidChoiceWord(item))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<string> BuildJosaVariants(string stem)
        {
            bool hasBatchim = HasFinalConsonant(stem);

            List<string> variants = new List<string>
            {
                stem + (hasBatchim ? "이" : "가"),
                stem + (hasBatchim ? "은" : "는"),
                stem + (hasBatchim ? "을" : "를"),
                stem + (hasBatchim ? "과" : "와"),
                stem + "도",
                stem + "만",
                stem + "의",
                stem + (hasBatchim ? "으로" : "로"),
                stem + "에",
                stem + (hasBatchim ? "이나" : "나")
            };

            Shuffle(variants);

            foreach (string item in variants.Distinct(StringComparer.Ordinal))
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildStemSimilarWords(string answer)
        {
            List<string> similarWords = _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Where(x => Math.Abs(x.Length - answer.Length) <= 1)
                .Where(x => SharedPrefixScore(answer, x) >= Math.Max(2, answer.Length / 2))
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => SharedPrefixScore(answer, x))
                .ThenBy(x => Math.Abs(x.Length - answer.Length))
                .ToList();

            foreach (string item in similarWords)
            {
                yield return item;
            }
        }

        private static bool IsValidChoiceWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string normalized = word.Trim();

            if (normalized.Length < 2)
            {
                return false;
            }

            if (normalized.Any(char.IsWhiteSpace))
            {
                return false;
            }

            int koreanCount = normalized.Count(ch => ch >= 0xAC00 && ch <= 0xD7A3);
            if (koreanCount < Math.Max(1, normalized.Length / 2))
            {
                return false;
            }

            return true;
        }

        private static bool IsMostlyNounLike(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string[] verbLikeEndings =
            {
                "하다", "되다", "가다", "오다", "보다", "이다",
                "니라", "리라", "더라", "도다", "노라",
                "하라", "하여라", "하며", "하고", "하니", "하되"
            };

            return !verbLikeEndings.Any(word.EndsWith);
        }

        private static string RemoveKnownJosa(string word, out string? josa)
        {
            foreach (string candidate in KnownJosa.OrderByDescending(x => x.Length))
            {
                if (word.Length > candidate.Length && word.EndsWith(candidate, StringComparison.Ordinal))
                {
                    josa = candidate;
                    return word.Substring(0, word.Length - candidate.Length);
                }
            }

            josa = null;
            return word;
        }

        private static bool HasFinalConsonant(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            char lastChar = text[^1];

            if (lastChar < 0xAC00 || lastChar > 0xD7A3)
            {
                return false;
            }

            int code = lastChar - 0xAC00;
            int jong = code % 28;
            return jong != 0;
        }

        private int SharedPrefixScore(string a, string b)
        {
            int len = Math.Min(a.Length, b.Length);
            int count = 0;

            for (int i = 0; i < len; i++)
            {
                if (a[i] != b[i])
                {
                    break;
                }

                count++;
            }

            return count;
        }
    }
}