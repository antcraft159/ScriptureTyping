using ScriptureTyping.Services;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            window.Show();
        }
    }
}