// 파일명: ClozeDifficulty.cs
namespace ScriptureTyping.ViewModels.Games.Cloze
{
    /// <summary>
    /// 목적:
    /// 빈칸 채우기 게임의 난이도 종류를 정의한다.
    /// 
    /// 사용처:
    /// - 화면에서 난이도 선택
    /// - 팩토리에서 모드 생성
    /// - ViewModel에서 현재 모드 관리
    /// </summary>
    public enum ClozeDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        VeryHard = 3,
        SamuelRank1 = 4
    }
}