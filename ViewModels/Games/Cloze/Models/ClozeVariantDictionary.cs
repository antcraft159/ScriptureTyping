using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    public sealed class ClozeVariantDictionary
    {
        public Dictionary<string, IReadOnlyList<string>> VariantsByWord { get; } = new();
    }
}