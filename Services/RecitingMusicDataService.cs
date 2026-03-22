using ScriptureTyping.ViewModels.RecitingMusic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ScriptureTyping.Services
{
    /// <summary>
    /// 목적:
    /// 동요 학습 JSON 데이터를 읽어서 RecitingMusicItemViewModel 목록으로 변환한다.
    /// </summary>
    public sealed class RecitingMusicDataService
    {
        /// <summary>
        /// 목적:
        /// 기본 JSON 경로에서 동요 목록을 읽는다.
        /// </summary>
        public List<RecitingMusicItemViewModel> LoadItems()
        {
            string jsonPath = ResolveJsonPath();
            return LoadItems(jsonPath);
        }

        /// <summary>
        /// 목적:
        /// 지정한 JSON 파일 경로에서 동요 목록을 읽는다.
        /// </summary>
        public List<RecitingMusicItemViewModel> LoadItems(string jsonFilePath)
        {
            if (string.IsNullOrWhiteSpace(jsonFilePath))
            {
                return new List<RecitingMusicItemViewModel>();
            }

            if (!File.Exists(jsonFilePath))
            {
                return new List<RecitingMusicItemViewModel>();
            }

            try
            {
                string json = File.ReadAllText(jsonFilePath);

                List<RecitingMusicItemData>? rawItems = JsonSerializer.Deserialize<List<RecitingMusicItemData>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    });

                if (rawItems == null || rawItems.Count == 0)
                {
                    return new List<RecitingMusicItemViewModel>();
                }

                List<RecitingMusicItemViewModel> items = new List<RecitingMusicItemViewModel>();

                foreach (RecitingMusicItemData rawItem in rawItems)
                {
                    if (rawItem == null)
                    {
                        continue;
                    }

                    RecitingMusicItemViewModel item = new RecitingMusicItemViewModel(
                        number: rawItem.Number,
                        course: rawItem.Course ?? string.Empty,
                        day: rawItem.Day ?? string.Empty,
                        reference: rawItem.Reference ?? string.Empty,
                        originalVerse: rawItem.OriginalVerse ?? string.Empty,
                        lyricsPreview: rawItem.LyricsPreview ?? string.Empty,
                        status: rawItem.Status ?? string.Empty,
                        mp3FileName: rawItem.Mp3FileName ?? string.Empty);

                    items.Add(item);
                }

                return items;
            }
            catch
            {
                return new List<RecitingMusicItemViewModel>();
            }
        }

        /// <summary>
        /// 목적:
        /// 실행 폴더 기준 경로와 프로젝트 루트 기준 경로를 순서대로 검사하여 JSON 파일 경로를 찾는다.
        /// </summary>
        private static string ResolveJsonPath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string outputPath = Path.Combine(
                baseDirectory,
                "RecitingData",
                "RecitingMusic",
                "reciting_music.json");

            if (File.Exists(outputPath))
            {
                return outputPath;
            }

            string projectRootPath = Path.GetFullPath(Path.Combine(
                baseDirectory,
                @"..\..\..\..",
                "RecitingData",
                "RecitingMusic",
                "reciting_music.json"));

            if (File.Exists(projectRootPath))
            {
                return projectRootPath;
            }

            return outputPath;
        }

        /// <summary>
        /// 목적:
        /// JSON 역직렬화 전용 데이터 모델.
        /// </summary>
        private sealed class RecitingMusicItemData
        {
            public int Number { get; set; }
            public string? Course { get; set; }
            public string? Day { get; set; }
            public string? Reference { get; set; }
            public string? OriginalVerse { get; set; }
            public string? SongTitle { get; set; }
            public string? LyricsPreview { get; set; }
            public string? Status { get; set; }
            public string? Mp3FileName { get; set; }
        }
    }
}