using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    public sealed class ClozeWordAnalyzer : IClozeWordAnalyzer
    {
        private static readonly string[][] EndingGroups =
        {
            new[]
            {
                "하였더니",
                "하였더라",
                "하였도다",
                "하였느니라",
                "하였니라",
                "하였으리라",
                "하였음이라",
                "하였으니"
            },
            new[]
            {
                "였더니",
                "였더라",
                "였도다",
                "였느니라",
                "였니라",
                "였으리라",
                "였음이라",
                "였으니"
            },
            new[]
            {
                "하니라",
                "하느니라",
                "하도다",
                "하더라",
                "하리라",
                "하리니"
            },
            new[]
            {
                "더니",
                "더라",
                "도다",
                "느니라",
                "니라",
                "리라",
                "리니"
            },
            new[]
            {
                "으리라",
                "으리니",
                "으니라",
                "으느니라"
            },
            new[]
            {
                "이니라",
                "이로다",
                "로다"
            },
            new[]
            {
                "시니라",
                "시도다",
                "시로다",
                "시더라",
                "시리라"
            },
            new[]
            {
                "셨도다",
                "셨느니라",
                "셨더라",
                "셨으니",
                "셨음이라"
            },
            new[]
            {
                "하심이라",
                "하심이니라",
                "심이라",
                "심이니라"
            },
            new[]
            {
                "음이라",
                "음이니라",
                "었음이라",
                "였음이라"
            },
            new[]
            {
                "것이로다",
                "것이니라",
                "것이라"
            },
            new[]
            {
                "할찌라",
                "될찌라",
                "을찌라",
                "찌어다"
            },
            new[]
            {
                "하였으니",
                "였으니",
                "으니"
            },
            new[]
            {
                "거니와",
                "노니",
                "노라",
                "로되"
            },
            new[]
            {
                "되리라",
                "되리요"
            },
            new[]
            {
                "하시리라",
                "하시리니",
                "하리니"
            },
            new[]
            {
                "있느니라",
                "없느니라",
                "아니하느니라"
            },
            new[]
            {
                "하였도다",
                "었도다",
                "았도다"
            }
        };

        private static readonly string[] InvalidTailAttachments =
        {
            "들의",
            "들이",
            "들을",
            "에서",
            "에게",
            "으로",
            "으로서",
            "으로써",
            "은",
            "는",
            "이",
            "가",
            "을",
            "를",
            "와",
            "과",
            "도",
            "만",
            "의",
            "에",
            "께",
            "로",
            "나"
        };

        private static readonly string[] InvalidWholeEndings =
        {
            "리로다"
        };

        private static readonly string[] VerbLikeStemSuffixes =
        {
            "하",
            "하였",
            "되",
            "되었",
            "있",
            "없",
            "이르",
            "가",
            "오",
            "보",
            "듣",
            "알",
            "받",
            "얻",
            "주",
            "내리",
            "들으",
            "보이",
            "지키",
            "행하",
            "구속하",
            "도말하",
            "생각하",
            "경배하",
            "책망하",
            "잠잠하",
            "말하",
            "대언하",
            "알리",
            "이루",
            "멸하",
            "보존하",
            "기억하",
            "돌아오",
            "세우",
            "창조하",
            "지으",
            "만드",
            "부르",
            "흩으",
            "모으",
            "정하",
            "올라가",
            "내려가",
            "쏟아지",
            "떨어뜨리",
            "사하",
            "믿",
            "구하",
            "사랑하",
            "심판하",
            "죽",
            "살",
            "취하",
            "버리",
            "드리",
            "부으",
            "마시",
            "받으",
            "먹",
            "잡",
            "치",
            "가르치"
        };

        private static readonly string[] HaConjugationNounBases =
        {
            "가증",
            "경배",
            "증거",
            "송사",
            "변명",
            "심판",
            "책망",
            "구속",
            "도말",
            "보존",
            "창조"
        };

        private static readonly string[] NounLikeStandaloneStems =
        {
            "사",
            "표",
            "죄",
            "말",
            "책",
            "불",
            "손",
            "발",
            "눈",
            "뼈",
            "산",
            "물",
            "땅",
            "성",
            "왕",
            "백성"
        };

        private static readonly string[] NounParticleEndings =
        {
            "으로서",
            "으로써",
            "으로부터",
            "에게서",
            "들의",
            "들이",
            "들을",
            "에서",
            "에게",
            "으로",
            "은",
            "는",
            "이",
            "가",
            "을",
            "를",
            "에",
            "의",
            "께",
            "와",
            "과",
            "도",
            "만",
            "로",
            "나",
            "들"
        };

        public ClozeWordAnalysisResult Analyze(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return Invalid(word);
            }

            string normalized = word.Trim();

            if (TryAnalyzeParticleWord(normalized, out ClozeWordAnalysisResult? particleResult))
            {
                return particleResult!;
            }

            if (HasInvalidTailAttachment(normalized))
            {
                return Invalid(normalized);
            }

            for (int groupIndex = 0; groupIndex < EndingGroups.Length; groupIndex++)
            {
                string[] endings = EndingGroups[groupIndex];

                foreach (string ending in endings.OrderByDescending(x => x.Length))
                {
                    if (InvalidWholeEndings.Any(x => string.Equals(x, ending, StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    if (!normalized.EndsWith(ending, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string stem = normalized.Substring(0, normalized.Length - ending.Length);
                    if (!LooksLikePredicateStem(stem))
                    {
                        continue;
                    }

                    ClozeWordType wordType = GetWordTypeByEnding(ending);

                    return new ClozeWordAnalysisResult
                    {
                        IsValid = true,
                        Word = normalized,
                        Stem = stem,
                        Ending = ending,
                        GroupIndex = groupIndex,
                        WordType = wordType,
                        Particle = string.Empty,
                        IsHonorific = IsHonorificEnding(ending),
                        IsPast = IsPastEnding(ending),
                        IsFuture = IsFutureEnding(ending),
                        IsCopula = IsCopulaEnding(ending)
                    };
                }
            }

            return Invalid(normalized);
        }

        private static bool TryAnalyzeParticleWord(string word, out ClozeWordAnalysisResult? result)
        {
            foreach (string particle in NounParticleEndings.OrderByDescending(x => x.Length))
            {
                if (!word.EndsWith(particle, StringComparison.Ordinal))
                {
                    continue;
                }

                if (word.Length <= particle.Length + 1)
                {
                    continue;
                }

                string stem = word.Substring(0, word.Length - particle.Length);

                if (!LooksLikeNounStem(stem))
                {
                    continue;
                }

                result = new ClozeWordAnalysisResult
                {
                    IsValid = true,
                    Word = word,
                    Stem = stem,
                    Ending = string.Empty,
                    GroupIndex = -1,
                    WordType = ClozeWordType.NounWithParticle,
                    Particle = particle,
                    IsHonorific = false,
                    IsPast = false,
                    IsFuture = false,
                    IsCopula = false
                };
                return true;
            }

            result = null;
            return false;
        }

        private static ClozeWordAnalysisResult Invalid(string? word)
        {
            return new ClozeWordAnalysisResult
            {
                IsValid = false,
                Word = word ?? string.Empty,
                Stem = string.Empty,
                Ending = string.Empty,
                GroupIndex = -1,
                WordType = ClozeWordType.Unknown,
                Particle = string.Empty,
                IsHonorific = false,
                IsPast = false,
                IsFuture = false,
                IsCopula = false
            };
        }

        private static bool HasInvalidTailAttachment(string word)
        {
            string[] allEndings = EndingGroups
                .SelectMany(x => x)
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => x.Length)
                .ToArray();

            foreach (string ending in allEndings)
            {
                foreach (string attachment in InvalidTailAttachments)
                {
                    if (word.EndsWith(ending + attachment, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool LooksLikePredicateStem(string stem)
        {
            if (string.IsNullOrWhiteSpace(stem))
            {
                return false;
            }

            if (stem.Length < 2)
            {
                return false;
            }

            if (stem.EndsWith("것", StringComparison.Ordinal))
            {
                return true;
            }

            if (stem.EndsWith("수", StringComparison.Ordinal))
            {
                return false;
            }

            if (NounLikeStandaloneStems.Any(x => string.Equals(stem, x, StringComparison.Ordinal)))
            {
                return false;
            }

            if (VerbLikeStemSuffixes.Any(x => stem.EndsWith(x, StringComparison.Ordinal)))
            {
                return true;
            }

            if (stem.EndsWith("하시", StringComparison.Ordinal) ||
                stem.EndsWith("하셨", StringComparison.Ordinal) ||
                stem.EndsWith("하였", StringComparison.Ordinal) ||
                stem.EndsWith("되었", StringComparison.Ordinal) ||
                stem.EndsWith("하심", StringComparison.Ordinal))
            {
                return true;
            }

            if (HaConjugationNounBases.Any(x => string.Equals(stem, x, StringComparison.Ordinal)))
            {
                return true;
            }

            return false;
        }

        private static bool LooksLikeNounStem(string stem)
        {
            if (string.IsNullOrWhiteSpace(stem))
            {
                return false;
            }

            if (stem.Length < 2)
            {
                return false;
            }

            if (LooksLikePredicateStem(stem))
            {
                return false;
            }

            return true;
        }

        private static ClozeWordType GetWordTypeByEnding(string ending)
        {
            if (IsCopulaEnding(ending))
            {
                return ClozeWordType.CopulaEnding;
            }

            if (IsHonorificEnding(ending))
            {
                return ClozeWordType.HonorificPredicateEnding;
            }

            if (IsPastEnding(ending))
            {
                return ClozeWordType.PastPredicateEnding;
            }

            if (IsFutureEnding(ending))
            {
                return ClozeWordType.FuturePredicateEnding;
            }

            return ClozeWordType.PredicateEnding;
        }

        private static bool IsHonorificEnding(string ending)
        {
            return ending.Contains("시", StringComparison.Ordinal) || ending.Contains("셨", StringComparison.Ordinal);
        }

        private static bool IsPastEnding(string ending)
        {
            return ending.Contains("였", StringComparison.Ordinal) ||
                   ending.Contains("었", StringComparison.Ordinal) ||
                   ending.Contains("았", StringComparison.Ordinal) ||
                   ending.Contains("더", StringComparison.Ordinal);
        }

        private static bool IsFutureEnding(string ending)
        {
            return ending.Contains("리라", StringComparison.Ordinal) ||
                   ending.Contains("리니", StringComparison.Ordinal) ||
                   ending.Contains("찌라", StringComparison.Ordinal);
        }

        private static bool IsCopulaEnding(string ending)
        {
            return ending.Contains("이니라", StringComparison.Ordinal) ||
                   ending.Contains("이로다", StringComparison.Ordinal) ||
                   ending == "로다" ||
                   ending == "것이로다" ||
                   ending == "것이니라" ||
                   ending == "것이라";
        }
    }
}