using System;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace ScriptureTyping.Services
{
    public sealed class AppUpdateService
    {
        private const string UpdateRepositoryUrl = "https://github.com/antcraft159/ScriptureTyping";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(UpdateRepositoryUrl) &&
            !UpdateRepositoryUrl.Contains("여기에 계정", StringComparison.Ordinal) &&
            !UpdateRepositoryUrl.Contains("여기에 저장소", StringComparison.Ordinal);

        public async Task<AppUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
            {
                return AppUpdateCheckResult.NotConfigured();
            }

            UpdateManager manager = CreateUpdateManager();

            if (!manager.IsInstalled)
            {
                return AppUpdateCheckResult.NotInstalled(GetCurrentVersionText(manager));
            }

            UpdateInfo? updateInfo = await manager.CheckForUpdatesAsync();

            if (updateInfo is null)
            {
                return AppUpdateCheckResult.NoUpdate(GetCurrentVersionText(manager));
            }

            string currentVersion = GetCurrentVersionText(manager);
            string targetVersion = updateInfo.TargetFullRelease.Version?.ToString()
                                   ?? updateInfo.TargetFullRelease.FileName
                                   ?? "알 수 없음";

            string releaseNotes = updateInfo.TargetFullRelease.NotesMarkdown ?? string.Empty;

            return AppUpdateCheckResult.UpdateAvailable(
                currentVersion,
                targetVersion,
                releaseNotes,
                updateInfo);
        }

        public async Task DownloadUpdatesAsync(UpdateInfo updateInfo, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updateInfo);

            UpdateManager manager = CreateUpdateManager();

            if (!manager.IsInstalled)
            {
                throw new InvalidOperationException("설치된 프로그램이 아니어서 업데이트를 다운로드할 수 없습니다.");
            }

            await manager.DownloadUpdatesAsync(
                updateInfo,
                _ => { },
                cancellationToken);
        }

        public void ApplyUpdatesAndRestart(UpdateInfo updateInfo)
        {
            ArgumentNullException.ThrowIfNull(updateInfo);

            UpdateManager manager = CreateUpdateManager();
            manager.ApplyUpdatesAndRestart(updateInfo.TargetFullRelease);
        }

        private static UpdateManager CreateUpdateManager()
        {
            GithubSource source = new GithubSource(
                UpdateRepositoryUrl,
                string.Empty,
                false);

            return new UpdateManager(source);
        }

        private static string GetCurrentVersionText(UpdateManager manager)
        {
            return manager.CurrentVersion?.ToString() ?? "알 수 없음";
        }
    }

    public enum AppUpdateCheckState
    {
        NotConfigured,
        NotInstalled,
        NoUpdate,
        UpdateAvailable
    }

    public sealed class AppUpdateCheckResult
    {
        private AppUpdateCheckResult(
            AppUpdateCheckState state,
            string currentVersion,
            string targetVersion,
            string releaseNotes,
            UpdateInfo? updateInfo)
        {
            State = state;
            CurrentVersion = currentVersion;
            TargetVersion = targetVersion;
            ReleaseNotes = releaseNotes;
            UpdateInfo = updateInfo;
        }

        public AppUpdateCheckState State { get; }

        public string CurrentVersion { get; }

        public string TargetVersion { get; }

        public string ReleaseNotes { get; }

        public UpdateInfo? UpdateInfo { get; }

        public bool HasUpdate => State == AppUpdateCheckState.UpdateAvailable && UpdateInfo is not null;

        public static AppUpdateCheckResult NotConfigured()
        {
            return new AppUpdateCheckResult(
                AppUpdateCheckState.NotConfigured,
                string.Empty,
                string.Empty,
                string.Empty,
                null);
        }

        public static AppUpdateCheckResult NotInstalled(string currentVersion)
        {
            return new AppUpdateCheckResult(
                AppUpdateCheckState.NotInstalled,
                currentVersion,
                string.Empty,
                string.Empty,
                null);
        }

        public static AppUpdateCheckResult NoUpdate(string currentVersion)
        {
            return new AppUpdateCheckResult(
                AppUpdateCheckState.NoUpdate,
                currentVersion,
                string.Empty,
                string.Empty,
                null);
        }

        public static AppUpdateCheckResult UpdateAvailable(
            string currentVersion,
            string targetVersion,
            string releaseNotes,
            UpdateInfo updateInfo)
        {
            return new AppUpdateCheckResult(
                AppUpdateCheckState.UpdateAvailable,
                currentVersion,
                targetVersion,
                releaseNotes,
                updateInfo);
        }
    }
}