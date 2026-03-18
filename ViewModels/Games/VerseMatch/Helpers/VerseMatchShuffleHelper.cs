using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Helpers
{
    /// <summary>
    /// 목적:
    /// 리스트를 무작위로 섞는 공용 헬퍼를 제공한다.
    /// </summary>
    public static class VerseMatchShuffleHelper
    {
        /// <summary>
        /// 목적:
        /// Fisher-Yates 방식으로 리스트를 섞는다.
        /// </summary>
        public static void Shuffle<T>(IList<T> items, Random random)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (random is null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}