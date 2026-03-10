using Microsoft.Extensions.DependencyInjection;
using ScriptureTyping.Services;
using ScriptureTyping.ViewModels;
using ScriptureTyping.ViewModels.Games;
using ScriptureTyping.ViewModels.Games.Cloze.Contracts;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Windows;

namespace ScriptureTyping
{
    public partial class App : Application
    {
        /// <summary>
        /// 목적: 앱 전체에서 공유되는 선택 상태(과정/일차/구절목록).
        ///       학습/게임 모두 여기 값을 그대로 사용한다.
        /// </summary>
        public static SelectionContext SelectionContext { get; } = new SelectionContext();

        /// <summary>
        /// 목적: 앱 전체 DI 컨테이너
        /// </summary>
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            Services = services.BuildServiceProvider();

            MainWindow window = Services.GetRequiredService<MainWindow>();
            window.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // 공용 상태
            services.AddSingleton(SelectionContext);

            // 분석 서비스
            services.AddSingleton<IClozeWordAnalyzer, ClozeWordAnalyzer>();

            // ViewModel
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<ClozeGameViewModel>();

            // View
            services.AddSingleton<MainWindow>();
        }
    }
}