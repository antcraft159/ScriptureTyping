using System;
using System.Collections.Generic;
using System.Linq;

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
            return 1;
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
    }
}