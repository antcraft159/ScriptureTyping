namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    /// <summary>
    /// 목적:
    /// 단어 형태를 분류한다.
    /// 보기 생성 시 어떤 방식으로 오답을 만들지 결정하는 기준이 된다.
    /// </summary>
    public enum ClozeWordType
    {
        Unknown = 0,
        NounWithParticle = 1,
        PredicateEnding = 2,
        HonorificPredicateEnding = 3,
        PastPredicateEnding = 4,
        FuturePredicateEnding = 5,
        CopulaEnding = 6
    }
}