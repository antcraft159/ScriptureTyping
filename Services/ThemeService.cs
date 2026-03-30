using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ScriptureTyping.Services
{
    public enum AppTheme
    {
        Dark,
        Light
    }

    public static class ThemeService
    {
        private const string DarkThemeFileName = "Theme.Dark.xaml";
        private const string LightThemeFileName = "Theme.Light.xaml";

        public static AppTheme CurrentTheme => GetCurrentTheme();

        public static void ToggleTheme()
        {
            AppTheme nextTheme = GetCurrentTheme() == AppTheme.Dark
                ? AppTheme.Light
                : AppTheme.Dark;

            ApplyTheme(nextTheme);
        }

        public static void ApplyTheme(AppTheme theme)
        {
            Application? app = Application.Current;
            if (app is null)
            {
                return;
            }

            string assemblyName =
                Assembly.GetEntryAssembly()?.GetName().Name
                ?? Assembly.GetExecutingAssembly().GetName().Name
                ?? "ScriptureTyping";

            string themeFileName = theme == AppTheme.Dark
                ? DarkThemeFileName
                : LightThemeFileName;

            string themeUri = $"/{assemblyName};component/Resources/Themes/{themeFileName}";

            RemoveThemeDictionaries(app);

            ResourceDictionary newThemeDictionary = new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            };

            app.Resources.MergedDictionaries.Add(newThemeDictionary);
        }

        private static AppTheme GetCurrentTheme()
        {
            Application? app = Application.Current;
            if (app is null)
            {
                return AppTheme.Dark;
            }

            ResourceDictionary? currentThemeDictionary = app.Resources.MergedDictionaries
                .LastOrDefault(d =>
                    d.Source is not null &&
                    (
                        d.Source.OriginalString.Contains(DarkThemeFileName, StringComparison.OrdinalIgnoreCase) ||
                        d.Source.OriginalString.Contains(LightThemeFileName, StringComparison.OrdinalIgnoreCase)
                    ));

            if (currentThemeDictionary?.Source?.OriginalString.Contains(LightThemeFileName, StringComparison.OrdinalIgnoreCase) == true)
            {
                return AppTheme.Light;
            }

            return AppTheme.Dark;
        }

        private static void RemoveThemeDictionaries(Application app)
        {
            for (int i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                ResourceDictionary dictionary = app.Resources.MergedDictionaries[i];
                string source = dictionary.Source?.OriginalString ?? string.Empty;

                bool isThemeDictionary =
                    source.Contains(DarkThemeFileName, StringComparison.OrdinalIgnoreCase) ||
                    source.Contains(LightThemeFileName, StringComparison.OrdinalIgnoreCase);

                if (isThemeDictionary)
                {
                    app.Resources.MergedDictionaries.RemoveAt(i);
                }
            }
        }
    }
}