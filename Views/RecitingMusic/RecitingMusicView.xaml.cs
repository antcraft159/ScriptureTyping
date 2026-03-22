using ScriptureTyping.ViewModels.RecitingMusic;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScriptureTyping.Views.RecitingMusic
{
    /// <summary>
    /// RecitingMusicView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecitingMusicView : UserControl
    {
        public RecitingMusicView()
        {
            InitializeComponent();
            DataContext = new RecitingMusicViewModel();
        }

        private void PlaybackSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RecitingMusicViewModel viewModel)
            {
                viewModel.BeginSeekDrag();
            }
        }

        private void PlaybackSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not RecitingMusicViewModel viewModel)
            {
                return;
            }

            if (sender is not Slider slider)
            {
                return;
            }

            viewModel.CommitSeek(slider.Value);
        }
    }
}