using System;

namespace ScriptureTyping.ViewModels.RecitingMusic
{
    /// <summary>
    /// 목적:
    /// 암송 동요 목록의 1개 항목 정보를 관리한다.
    ///
    /// 상태 규칙:
    /// - MP3 파일이 없으면 무조건 "불가능"
    /// - MP3 파일이 있으면 원본 상태가 "완료"일 때만 "완료"
    /// - 그 외에는 "가능"
    /// </summary>
    public sealed class RecitingMusicItemViewModel
    {
        public int Number { get; }
        public string Course { get; }
        public string Day { get; }
        public string Reference { get; }
        public string OriginalVerse { get; }
        public string LyricsPreview { get; }

        /// <summary>
        /// JSON 등 원본 데이터의 상태값.
        /// 예: 가능, 완료
        /// </summary>
        public string RawStatus { get; }

        public string Mp3FileName { get; }

        /// <summary>
        /// 실제 MP3 파일 존재 여부.
        /// </summary>
        public bool HasMp3File { get; }

        /// <summary>
        /// 화면 표시용 상태값.
        /// </summary>
        public string Status
        {
            get
            {
                if (!HasMp3File)
                {
                    return "불가능";
                }

                if (string.Equals(RawStatus, "완료", StringComparison.Ordinal))
                {
                    return "완료";
                }

                return "가능";
            }
        }

        public bool IsReady =>
            string.Equals(Status, "가능", StringComparison.Ordinal);

        public bool IsUnavailable =>
            string.Equals(Status, "불가능", StringComparison.Ordinal);

        public bool IsCompleted =>
            string.Equals(Status, "완료", StringComparison.Ordinal);

        public RecitingMusicItemViewModel(
            int number,
            string course,
            string day,
            string reference,
            string originalVerse,
            string lyricsPreview,
            string rawStatus,
            string mp3FileName,
            bool hasMp3File)
        {
            Number = number;
            Course = course ?? string.Empty;
            Day = day ?? string.Empty;
            Reference = reference ?? string.Empty;
            OriginalVerse = originalVerse ?? string.Empty;
            LyricsPreview = lyricsPreview ?? string.Empty;
            RawStatus = rawStatus ?? string.Empty;
            Mp3FileName = mp3FileName ?? string.Empty;
            HasMp3File = hasMp3File;
        }
    }
}