using ScriptureTyping.ViewModels.RecitingMusic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ScriptureTyping.Views.RecitingMusic
{
    public partial class RecitingMusicView : UserControl
    {
        private const double SEEK_POPUP_MARGIN = 8d;
        private const double SEEK_SECONDS = 10d;

        public RecitingMusicView()
        {
            InitializeComponent();
        }

        private void RecitingMusicView_Unloaded(object sender, RoutedEventArgs e)
        {
            CloseSeekPopup();

            if (DataContext is not RecitingMusicViewModel vm)
            {
                return;
            }

            vm.StopPlaybackOnLeaveView();
        }

        private void SeekBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            SeekBySeconds(-SEEK_SECONDS);
        }

        private void SeekForwardButton_Click(object sender, RoutedEventArgs e)
        {
            SeekBySeconds(SEEK_SECONDS);
        }

        private void SeekBySeconds(double deltaSeconds)
        {
            if (DataContext is not RecitingMusicViewModel vm)
            {
                return;
            }

            double minimum = PlaybackSlider.Minimum;
            double maximum = PlaybackSlider.Maximum;
            double currentValue = PlaybackSlider.Value;
            double nextValue = Math.Clamp(currentValue + deltaSeconds, minimum, maximum);

            PlaybackSlider.Value = nextValue;
            vm.CommitSeek(nextValue);
        }

        private void PlaybackSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Slider slider)
            {
                return;
            }

            if (DataContext is RecitingMusicViewModel vm)
            {
                vm.BeginSeekDrag();
            }

            OpenSeekPopup(slider);
        }

        private void PlaybackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is not Slider slider)
            {
                return;
            }

            if (!slider.IsMouseCaptureWithin && !PlaybackSeekPopup.IsOpen)
            {
                return;
            }

            if (DataContext is RecitingMusicViewModel vm)
            {
                vm.BeginSeekDrag();
            }

            UpdateSeekPopupPosition(slider);
        }

        private void PlaybackSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Slider slider)
            {
                return;
            }

            if (DataContext is RecitingMusicViewModel vm)
            {
                vm.CommitSeek(slider.Value);
            }

            CloseSeekPopup();
        }

        private void PlaybackSlider_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is not Slider slider)
            {
                CloseSeekPopup();
                return;
            }

            if (PlaybackSeekPopup.IsOpen && DataContext is RecitingMusicViewModel vm)
            {
                vm.CommitSeek(slider.Value);
            }

            CloseSeekPopup();
        }

        private void OpenSeekPopup(Slider slider)
        {
            PlaybackSeekPopup.IsOpen = true;
            UpdateSeekPopupPosition(slider);
        }

        private void CloseSeekPopup()
        {
            PlaybackSeekPopup.IsOpen = false;
        }

        private void UpdateSeekPopupPosition(Slider slider)
        {
            if (PlaybackSeekPopup.Child is not FrameworkElement popupChild)
            {
                return;
            }

            popupChild.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double popupWidth = popupChild.DesiredSize.Width;
            double popupHeight = popupChild.DesiredSize.Height;

            Thumb? thumb = FindVisualChild<Thumb>(slider);
            if (thumb != null && thumb.ActualWidth > 0)
            {
                Point thumbTopLeft = thumb.TranslatePoint(new Point(0, 0), PlaybackSliderHost);

                PlaybackSeekPopup.HorizontalOffset = thumbTopLeft.X + (thumb.ActualWidth / 2d) - (popupWidth / 2d);
                PlaybackSeekPopup.VerticalOffset = -popupHeight - SEEK_POPUP_MARGIN;
                return;
            }

            double range = slider.Maximum - slider.Minimum;
            double ratio = 0d;

            if (range > 0d)
            {
                ratio = (slider.Value - slider.Minimum) / range;
            }

            double x = ratio * slider.ActualWidth;

            PlaybackSeekPopup.HorizontalOffset = x - (popupWidth / 2d);
            PlaybackSeekPopup.VerticalOffset = -popupHeight - SEEK_POPUP_MARGIN;
        }

        private static T? FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int index = 0; index < childCount; index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, index);

                if (child is T target)
                {
                    return target;
                }

                T? result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}