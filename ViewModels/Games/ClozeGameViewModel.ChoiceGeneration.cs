using Microsoft.Extensions.DependencyInjection;
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private const int MAX_SAME_STEM_VARIANTS = 3;

        private static readonly string[][] ParticleConfusionGroups =
        {
            new[] { "이", "가" },
            new[] { "은", "는" },
            new[] { "을", "를" },
            new[] { "와", "과" },
            new[] { "로", "으로" },
            new[] { "에", "에게", "에서" },
            new[] { "도", "만", "나" },
            new[] { "의" }
        };

        private static readonly string[][] EndingConfusionGroups =
        {
            new[] { "느니라", "니라", "더라", "도다", "노라" },
            new[] { "이니라", "이로다", "로다" },
            new[] { "으리라", "리라", "리니", "으리니" },
            new[] { "시니라", "시도다", "시더라", "시리라", "시로다" },
            new[] { "하였더라", "하였도다", "하였느니라", "하였니라" },
            new[] { "였더라", "였도다", "였느니라", "였니라" },
            new[] { "되리라", "되리요" },
            new[] { "하시리라", "하시리니", "하리니" }
        };

        private IClozeWordAnalyzer WordAnalyzer =>
            global::ScriptureTyping.App.Services.GetRequiredService<IClozeWordAnalyzer>();

        private List<string> BuildChoices(string answer)
        {
            if (CurrentDifficulty == DIFFICULTY_HARD)
            {
                return BuildHardParticleOnlyChoices(answer);
            }

            int choiceCount = GetChoiceCount();

            ClozeWordAnalysisResult answerAnalysis = WordAnalyzer.Analyze(answer);

            HashSet<string> selected = new HashSet<string>(StringComparer.Ordinal)
            {
                answer
            };

            List<string> rankedCandidates = GenerateDistractors(answer, answerAnalysis)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => ScoreCandidate(answer, answerAnalysis, x))
                .ToList();

            int sameStemCount = 1;

            foreach (string candidate in rankedCandidates)
            {
                if (selected.Count >= choiceCount)
                {
                    break;
                }

                ClozeWordAnalysisResult candidateAnalysis = WordAnalyzer.Analyze(candidate);

                bool sameStem = answerAnalysis.IsValid &&
                                candidateAnalysis.IsValid &&
                                !string.IsNullOrWhiteSpace(answerAnalysis.Stem) &&
                                string.Equals(answerAnalysis.Stem, candidateAnalysis.Stem, StringComparison.Ordinal);

                if (sameStem && sameStemCount >= MAX_SAME_STEM_VARIANTS)
                {
                    continue;
                }

                if (!selected.Add(candidate))
                {
                    continue;
                }

                if (sameStem)
                {
                    sameStemCount++;
                }
            }

            foreach (string fallback in BuildFallbackChoices(answer, answerAnalysis))
            {
                if (selected.Count >= choiceCount)
                {
                    break;
                }

                selected.Add(fallback);
            }

            List<string> finalChoices = selected.ToList();
            Shuffle(finalChoices);
            return finalChoices.Take(choiceCount).ToList();
        }

        private List<string> BuildHardParticleOnlyChoices(string answer)
        {
            int choiceCount = GetChoiceCount();

            ClozeWordAnalysisResult answerAnalysis = WordAnalyzer.Analyze(answer);

            if (!answerAnalysis.IsValid ||
                answerAnalysis.WordType != ClozeWordType.NounWithParticle ||
                string.IsNullOrWhiteSpace(answerAnalysis.Stem) ||
                string.IsNullOrWhiteSpace(answerAnalysis.Particle))
            {
                return new List<string>();
            }

            if (!HardAllowedParticles.Contains(answerAnalysis.Particle))
            {
                return new List<string>();
            }

            if (!IsNaturalHardNounStem(answerAnalysis.Stem))
            {
                return new List<string>();
            }

            HashSet<string> selected = new HashSet<string>(StringComparer.Ordinal)
            {
                answer
            };

            foreach (string candidate in BuildStrictParticleVariants(answerAnalysis))
            {
                if (selected.Count >= choiceCount)
                {
                    break;
                }

                selected.Add(candidate);
            }

            if (selected.Count < choiceCount)
            {
                return new List<string>();
            }

            List<string> finalChoices = selected.ToList();
            Shuffle(finalChoices);
            return finalChoices.Take(choiceCount).ToList();
        }

        private IEnumerable<string> BuildStrictParticleVariants(ClozeWordAnalysisResult answerAnalysis)
        {
            if (string.IsNullOrWhiteSpace(answerAnalysis.Stem) ||
                string.IsNullOrWhiteSpace(answerAnalysis.Particle))
            {
                yield break;
            }

            if (!HardAllowedParticles.Contains(answerAnalysis.Particle))
            {
                yield break;
            }

            if (!IsNaturalHardNounStem(answerAnalysis.Stem))
            {
                yield break;
            }

            string stem = answerAnalysis.Stem;
            bool hasBatchim = HasFinalConsonant(stem);

            List<string> candidates = new List<string>
            {
                stem + (hasBatchim ? "이" : "가"),
                stem + (hasBatchim ? "은" : "는"),
                stem + (hasBatchim ? "을" : "를"),
                stem + (hasBatchim ? "과" : "와"),
                stem + "에",
                stem + "에서",
                stem + "에게",
                stem + (hasBatchim ? "으로" : "로"),
                stem + "도",
                stem + "만",
                stem + "의"
            };

            foreach (string[] group in ParticleConfusionGroups)
            {
                if (!group.Contains(answerAnalysis.Particle, StringComparer.Ordinal))
                {
                    continue;
                }

                foreach (string particle in group)
                {
                    string candidate = stem + particle;
                    if (HardAllowedParticles.Contains(particle))
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            HashSet<string> yielded = new HashSet<string>(StringComparer.Ordinal);

            foreach (string candidate in candidates
                .Where(x => !string.Equals(x, answerAnalysis.Word, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Distinct(StringComparer.Ordinal))
            {
                if (yielded.Add(candidate))
                {
                    yield return candidate;
                }
            }
        }

        private IEnumerable<string> GenerateDistractors(string answer, ClozeWordAnalysisResult answerAnalysis)
        {
            if (answerAnalysis.IsValid)
            {
                if (answerAnalysis.WordType == ClozeWordType.NounWithParticle)
                {
                    foreach (string item in BuildParticleDistractors(answerAnalysis))
                    {
                        yield return item;
                    }

                    foreach (string item in BuildStemNeighborWords(answer, answerAnalysis))
                    {
                        yield return item;
                    }

                    yield break;
                }

                foreach (string item in BuildEndingGroupDistractors(answerAnalysis))
                {
                    yield return item;
                }

                foreach (string item in BuildPredicateNeighborWords(answer, answerAnalysis))
                {
                    yield return item;
                }

                yield break;
            }

            foreach (string item in BuildGenericSimilarWords(answer))
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildParticleDistractors(ClozeWordAnalysisResult answerAnalysis)
        {
            if (string.IsNullOrWhiteSpace(answerAnalysis.Stem) || string.IsNullOrWhiteSpace(answerAnalysis.Particle))
            {
                yield break;
            }

            HashSet<string> yielded = new HashSet<string>(StringComparer.Ordinal);

            foreach (string[] group in ParticleConfusionGroups)
            {
                if (!group.Contains(answerAnalysis.Particle, StringComparer.Ordinal))
                {
                    continue;
                }

                foreach (string particle in group)
                {
                    string candidate = answerAnalysis.Stem + particle;
                    if (!string.Equals(candidate, answerAnalysis.Word, StringComparison.Ordinal) &&
                        IsValidChoiceWord(candidate) &&
                        yielded.Add(candidate))
                    {
                        yield return candidate;
                    }
                }
            }

            bool hasBatchim = HasFinalConsonant(answerAnalysis.Stem);

            string[] extras =
            {
                answerAnalysis.Stem + (hasBatchim ? "이" : "가"),
                answerAnalysis.Stem + (hasBatchim ? "은" : "는"),
                answerAnalysis.Stem + (hasBatchim ? "을" : "를"),
                answerAnalysis.Stem + (hasBatchim ? "과" : "와"),
                answerAnalysis.Stem + "도",
                answerAnalysis.Stem + "만",
                answerAnalysis.Stem + "의"
            };

            foreach (string candidate in extras)
            {
                if (!string.Equals(candidate, answerAnalysis.Word, StringComparison.Ordinal) &&
                    IsValidChoiceWord(candidate) &&
                    yielded.Add(candidate))
                {
                    yield return candidate;
                }
            }
        }

        private IEnumerable<string> BuildEndingGroupDistractors(ClozeWordAnalysisResult answerAnalysis)
        {
            if (string.IsNullOrWhiteSpace(answerAnalysis.Stem) || string.IsNullOrWhiteSpace(answerAnalysis.Ending))
            {
                yield break;
            }

            HashSet<string> yielded = new HashSet<string>(StringComparer.Ordinal);

            foreach (string[] group in EndingConfusionGroups)
            {
                if (!group.Contains(answerAnalysis.Ending, StringComparer.Ordinal))
                {
                    continue;
                }

                foreach (string ending in group)
                {
                    string candidate = answerAnalysis.Stem + ending;
                    if (!string.Equals(candidate, answerAnalysis.Word, StringComparison.Ordinal) &&
                        IsValidChoiceWord(candidate) &&
                        yielded.Add(candidate))
                    {
                        yield return candidate;
                    }
                }
            }

            foreach (string candidate in BuildAnalyzerMatchedEndingVariants(answerAnalysis))
            {
                if (yielded.Add(candidate))
                {
                    yield return candidate;
                }
            }
        }

        private IEnumerable<string> BuildAnalyzerMatchedEndingVariants(ClozeWordAnalysisResult answerAnalysis)
        {
            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);

            foreach (string word in _globalWordPool.Distinct(StringComparer.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(word) || string.Equals(word, answerAnalysis.Word, StringComparison.Ordinal))
                {
                    continue;
                }

                ClozeWordAnalysisResult candidateAnalysis = WordAnalyzer.Analyze(word);

                if (!candidateAnalysis.IsValid)
                {
                    continue;
                }

                if (candidateAnalysis.WordType != answerAnalysis.WordType)
                {
                    continue;
                }

                if (candidateAnalysis.IsHonorific != answerAnalysis.IsHonorific)
                {
                    continue;
                }

                if (candidateAnalysis.IsPast != answerAnalysis.IsPast && CurrentDifficulty != DIFFICULTY_NORMAL)
                {
                    continue;
                }

                if (Math.Abs(word.Length - answerAnalysis.Word.Length) > 2)
                {
                    continue;
                }

                results.Add(word);
            }

            foreach (string item in results
                .OrderByDescending(x => ScoreCandidate(answerAnalysis.Word, answerAnalysis, x))
                .ThenBy(x => x.Length))
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildStemNeighborWords(string answer, ClozeWordAnalysisResult answerAnalysis)
        {
            foreach (string word in _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => ScoreCandidate(answer, answerAnalysis, x)))
            {
                ClozeWordAnalysisResult candidateAnalysis = WordAnalyzer.Analyze(word);

                if (!candidateAnalysis.IsValid)
                {
                    continue;
                }

                if (candidateAnalysis.WordType != ClozeWordType.NounWithParticle)
                {
                    continue;
                }

                if (candidateAnalysis.Particle != answerAnalysis.Particle)
                {
                    continue;
                }

                if (SharedPrefixScore(answerAnalysis.Stem, candidateAnalysis.Stem) < 1)
                {
                    continue;
                }

                yield return word;
            }
        }

        private IEnumerable<string> BuildPredicateNeighborWords(string answer, ClozeWordAnalysisResult answerAnalysis)
        {
            foreach (string word in _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => ScoreCandidate(answer, answerAnalysis, x)))
            {
                ClozeWordAnalysisResult candidateAnalysis = WordAnalyzer.Analyze(word);

                if (!candidateAnalysis.IsValid)
                {
                    continue;
                }

                if (candidateAnalysis.WordType == ClozeWordType.NounWithParticle)
                {
                    continue;
                }

                if (candidateAnalysis.IsHonorific != answerAnalysis.IsHonorific)
                {
                    continue;
                }

                if (Math.Abs(word.Length - answer.Length) > 2)
                {
                    continue;
                }

                yield return word;
            }
        }

        private IEnumerable<string> BuildGenericSimilarWords(string answer)
        {
            List<string> candidates = _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => SharedPrefixScore(answer, x))
                .ThenBy(x => Math.Abs(x.Length - answer.Length))
                .ToList();

            foreach (string item in candidates)
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildFallbackChoices(string answer, ClozeWordAnalysisResult answerAnalysis)
        {
            if (CurrentDifficulty == DIFFICULTY_HARD)
            {
                yield break;
            }

            foreach (string word in _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => ScoreCandidate(answer, answerAnalysis, x)))
            {
                yield return word;
            }
        }

        private int ScoreCandidate(string answer, ClozeWordAnalysisResult answerAnalysis, string candidate)
        {
            int score = 0;

            ClozeWordAnalysisResult candidateAnalysis = WordAnalyzer.Analyze(candidate);

            if (candidateAnalysis.IsValid)
            {
                score += 5;
            }

            if (Math.Abs(candidate.Length - answer.Length) <= 1)
            {
                score += 4;
            }

            score += SharedPrefixScore(answer, candidate) * 2;

            if (answerAnalysis.IsValid && candidateAnalysis.IsValid)
            {
                if (candidateAnalysis.WordType == answerAnalysis.WordType)
                {
                    score += 8;
                }

                if (!string.IsNullOrWhiteSpace(answerAnalysis.Stem) &&
                    string.Equals(answerAnalysis.Stem, candidateAnalysis.Stem, StringComparison.Ordinal))
                {
                    score += 10;
                }

                if (!string.IsNullOrWhiteSpace(answerAnalysis.Particle) &&
                    answerAnalysis.Particle == candidateAnalysis.Particle)
                {
                    score += 6;
                }

                if (!string.IsNullOrWhiteSpace(answerAnalysis.Ending) &&
                    answerAnalysis.Ending == candidateAnalysis.Ending)
                {
                    score += 6;
                }

                if (candidateAnalysis.IsHonorific == answerAnalysis.IsHonorific)
                {
                    score += 3;
                }

                if (candidateAnalysis.IsPast == answerAnalysis.IsPast)
                {
                    score += 2;
                }

                if (candidateAnalysis.IsFuture == answerAnalysis.IsFuture)
                {
                    score += 2;
                }
            }

            return score;
        }

        private bool IsValidChoiceWord(string word)
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

            ClozeWordAnalysisResult result = WordAnalyzer.Analyze(normalized);

            if (CurrentDifficulty == DIFFICULTY_EASY)
            {
                return koreanCount >= Math.Max(1, normalized.Length / 2);
            }

            if (CurrentDifficulty == DIFFICULTY_NORMAL)
            {
                return result.IsValid || koreanCount >= Math.Max(1, normalized.Length / 2);
            }

            return result.IsValid;
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