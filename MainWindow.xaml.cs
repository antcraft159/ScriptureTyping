using System.Windows;
using ScriptureTyping.Services;

namespace ScriptureTyping
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ToggleTheme();
        }

        private void MainMenu_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}