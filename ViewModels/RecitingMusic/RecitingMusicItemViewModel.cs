namespace ScriptureTyping.ViewModels.RecitingMusic
{
    public sealed class RecitingMusicItemViewModel
    {
        public int Number { get; }
        public string Course { get; }
        public string Day { get; }
        public string Reference { get; }
        public string OriginalVerse { get; }
        public string LyricsPreview { get; }
        public string Status { get; }
        public string Mp3FileName { get; }

        public RecitingMusicItemViewModel(
            int number,
            string course,
            string day,
            string reference,
            string originalVerse,
            string lyricsPreview,
            string status,
            string mp3FileName)
        {
            Number = number;
            Course = course;
            Day = day;
            Reference = reference;
            OriginalVerse = originalVerse;
            LyricsPreview = lyricsPreview;
            Status = status;
            Mp3FileName = mp3FileName;
        }
    }
}