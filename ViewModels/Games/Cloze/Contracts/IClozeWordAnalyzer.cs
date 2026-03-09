using ScriptureTyping.ViewModels.Games.Cloze.Models;

namespace ScriptureTyping.ViewModels.Games.Cloze.Contracts
{
    public interface IClozeWordAnalyzer
    {
        ClozeWordAnalysisResult Analyze(string word);
    }
}