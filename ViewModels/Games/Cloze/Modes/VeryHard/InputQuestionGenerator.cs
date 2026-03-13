using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 모드용 문제 생성기.
    ///
    /// 특징:
    /// - 보기 없이 주관식 입력 중심
    /// - 문장의 대부분을 빈칸 처리
    /// - 너무 짧은 토큰은 후보에서 제외 가능
    /// - 전체가 전부 빈칸이 되지 않도록 일부 토큰은 남긴다
    /// </summary>
    public sealed class InputQuestionGenerator : IClozeQuestionGenerator
    {
        /// <summary>
        /// 목적:
        /// 무작위 선택을 위한 Random 인스턴스.
        /// </summary>
        private readonly Random _random = new Random();

        /// <summary>
        /// 목적:
        /// 매우 어려움 문제를 생성한다.
        ///
        /// 동작:
        /// 1. 원문을 공백 기준으로 토큰화한다.
        /// 2. 빈칸 후보를 추린다.
        /// 3. 빈칸 개수를 계산한다.
        /// 4. 선택된 토큰을 ____ 로 치환한다.
        /// 5. 정답 목록을 만든다.
        /// </summary>
        public ClozeQuestion Generate(
            string sourceText,
            int blankCount,
            IReadOnlyList<string> wordPool)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return CreateEmptyQuestion();
            }

            List<string> tokens = Tokenize(sourceText);

            if (tokens.Count == 0)
            {
                return CreateEmptyQuestion();
            }

            List<int> candidates = GetCandidates(tokens);

            if (candidates.Count == 0)
            {
                return new ClozeQuestion
                {
                    OriginalText = sourceText,
                    MaskedText = sourceText,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "VeryHard"
                };
            }

            int targetBlankCount = ResolveBlankCount(blankCount, tokens.Count, candidates.Count);

            if (targetBlankCount <= 0)
            {
                return new ClozeQuestion
                {
                    OriginalText = sourceText,
                    MaskedText = sourceText,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "VeryHard"
                };
            }

            List<int> selected = SelectBlankIndexes(tokens, candidates, targetBlankCount);

            if (selected.Count == 0)
            {
                return new ClozeQuestion
                {
                    OriginalText = sourceText,
                    MaskedText = sourceText,
                    Answers = Array.Empty<ClozeAnswer>(),
                    OptionSets = Array.Empty<ClozeOptionSet>(),
                    ModeName = "VeryHard"
                };
            }

            List<ClozeAnswer> answers = BuildAnswers(tokens, selected);
            List<string> maskedTokens = BuildMaskedTokens(tokens, selected);

            return new ClozeQuestion
            {
                OriginalText = sourceText,
                MaskedText = string.Join(" ", maskedTokens),
                Answers = answers,
                OptionSets = Array.Empty<ClozeOptionSet>(),
                ModeName = "VeryHard"
            };
        }

        /// <summary>
        /// 목적:
        /// 빈 문제 객체를 만든다.
        /// </summary>
        private static ClozeQuestion CreateEmptyQuestion()
        {
            return new ClozeQuestion
            {
                OriginalText = string.Empty,
                MaskedText = string.Empty,
                Answers = Array.Empty<ClozeAnswer>(),
                OptionSets = Array.Empty<ClozeOptionSet>(),
                ModeName = "VeryHard"
            };
        }

        /// <summary>
        /// 목적:
        /// 원문을 공백 기준으로 토큰화한다.
        /// </summary>
        private List<string> Tokenize(string text)
        {
            return text
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim())
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .ToList();
        }

        /// <summary>
        /// 목적:
        /// 빈칸 후보가 될 토큰 인덱스를 찾는다.
        ///
        /// 규칙:
        /// - 기본적으로 2글자 이상만 후보로 본다.
        /// - 첫 단어와 마지막 단어는 문맥 힌트로 남기기 위해 제외한다.
        /// - 단, 토큰 수가 매우 적으면 예외적으로 전체 범위에서 후보를 찾는다.
        /// </summary>
        private List<int> GetCandidates(IReadOnlyList<string> tokens)
        {
            List<int> result = new List<int>();

            if (tokens.Count == 1)
            {
                if (tokens[0].Length >= 2)
                {
                    result.Add(0);
                }

                return result;
            }

            int start = tokens.Count >= 3 ? 1 : 0;
            int end = tokens.Count >= 3 ? tokens.Count - 2 : tokens.Count - 1;

            for (int i = start; i <= end; i++)
            {
                if (i < 0 || i >= tokens.Count)
                {
                    continue;
                }

                if (tokens[i].Length >= 2)
                {
                    result.Add(i);
                }
            }

            if (result.Count > 0)
            {
                return result;
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Length >= 2)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// 목적:
        /// 실제 빈칸 개수를 결정한다.
        ///
        /// 우선순위:
        /// 1. blankCount가 1 이상이면 그 값을 최대한 존중
        /// 2. blankCount가 0 이하이면 토큰 수에 따라 자동 계산
        ///
        /// 자동 계산 규칙:
        /// - 매우 짧은 문장: 후보의 약 60%
        /// - 짧은 문장: 후보의 약 70%
        /// - 보통 문장: 후보의 약 80%
        /// - 긴 문장: 후보의 약 90%
        /// </summary>
        private int ResolveBlankCount(int requestedBlankCount, int totalTokenCount, int candidateCount)
        {
            if (candidateCount <= 0)
            {
                return 0;
            }

            int resolvedCount;

            if (requestedBlankCount > 0)
            {
                resolvedCount = requestedBlankCount;
            }
            else
            {
                double ratio = GetMaskRatio(totalTokenCount);
                resolvedCount = (int)Math.Round(candidateCount * ratio, MidpointRounding.AwayFromZero);
            }

            if (resolvedCount < 1)
            {
                resolvedCount = 1;
            }

            if (resolvedCount > candidateCount)
            {
                resolvedCount = candidateCount;
            }

            return resolvedCount;
        }

        /// <summary>
        /// 목적:
        /// 문장 길이에 따라 빈칸 비율을 반환한다.
        /// </summary>
        private double GetMaskRatio(int totalTokenCount)
        {
            if (totalTokenCount <= 4)
            {
                return 0.60;
            }

            if (totalTokenCount <= 6)
            {
                return 0.70;
            }

            if (totalTokenCount <= 10)
            {
                return 0.80;
            }

            return 0.90;
        }

        /// <summary>
        /// 목적:
        /// 실제로 빈칸 처리할 인덱스를 선택한다.
        ///
        /// 규칙:
        /// - 긴 단어를 우선한다.
        /// - 길이가 같으면 랜덤하게 섞는다.
        /// - 최종 결과는 원문 순서대로 정렬한다.
        /// </summary>
        private List<int> SelectBlankIndexes(
            IReadOnlyList<string> tokens,
            IReadOnlyList<int> candidates,
            int targetBlankCount)
        {
            return candidates
                .OrderByDescending(index => tokens[index].Length)
                .ThenBy(_ => _random.Next())
                .Take(targetBlankCount)
                .OrderBy(index => index)
                .ToList();
        }

        /// <summary>
        /// 목적:
        /// 선택된 빈칸 인덱스를 바탕으로 정답 목록을 만든다.
        /// </summary>
        private List<ClozeAnswer> BuildAnswers(
            IReadOnlyList<string> tokens,
            IReadOnlyList<int> selectedIndexes)
        {
            List<ClozeAnswer> answers = new List<ClozeAnswer>();

            for (int i = 0; i < selectedIndexes.Count; i++)
            {
                int tokenIndex = selectedIndexes[i];

                answers.Add(new ClozeAnswer
                {
                    BlankIndex = i,
                    Text = tokens[tokenIndex],
                    TokenIndex = tokenIndex
                });
            }

            return answers;
        }

        /// <summary>
        /// 목적:
        /// 선택된 토큰을 ____ 로 치환한 마스킹 결과를 만든다.
        /// </summary>
        private List<string> BuildMaskedTokens(
            IReadOnlyList<string> tokens,
            IReadOnlyList<int> selectedIndexes)
        {
            List<string> maskedTokens = new List<string>(tokens);

            foreach (int tokenIndex in selectedIndexes)
            {
                maskedTokens[tokenIndex] = "____";
            }

            return maskedTokens;
        }
    }
}