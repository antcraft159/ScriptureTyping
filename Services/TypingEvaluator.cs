using System;

namespace ScriptureTyping.Services
{
    /// <summary>
    /// 목적: 정답(expected)과 입력(typed)을 비교해서 오타(틀린 글자 수)를 계산한다.
    /// 규칙:
    /// - 같은 인덱스의 문자가 다르면 1개
    /// - 길이 차이(남거나 부족한 문자)도 오타로 포함
    /// - 줄바꿈은 \n 으로 통일
    /// </summary>
    public static class TypingEvaluator
    {
        public static int CountMistakes(string? expected, string? typed)
        {
            expected = Normalize(expected);
            typed = Normalize(typed);

            int min = Math.Min(expected.Length, typed.Length);
            int mistakes = 0;

            for (int i = 0; i < min; i++)
            {
                if (expected[i] != typed[i])
                {
                    mistakes++;
                }
            }

            mistakes += Math.Abs(expected.Length - typed.Length);
            return mistakes;
        }

        public static string Normalize(string? s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return s.Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Trim();
        }
    }
}
