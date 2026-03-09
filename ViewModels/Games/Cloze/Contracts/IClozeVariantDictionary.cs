using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Contracts
{
    public interface IClozeVariantDictionary
    {
        IReadOnlyList<string> GetVariants(string word);
        bool HasEnoughVariants(string word, int minimumCount);
    }
}