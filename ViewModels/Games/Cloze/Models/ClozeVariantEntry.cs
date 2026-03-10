namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    public sealed class ClozeVariantEntry
    {
        public string Word { get; init; } = string.Empty;
        public string Stem { get; init; } = string.Empty;
        public string Ending { get; init; } = string.Empty;
        public int GroupIndex { get; init; }
    }
}