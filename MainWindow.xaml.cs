using System;
using System.Windows;
using System.Windows.Input;
using ScriptureTyping.Services;

namespace ScriptureTyping
{
    public partial class MainWindow : Window
    {
        private bool _isMainPlanDragging;
        private Point _mainPlanDragStartPoint;
        private Point _mainPlanDragStartTranslate;

        // 딱 맞는 상태가 1.0이므로 더 축소되지 않게 막음
        private const double MainPlanMinScale = 1.0;
        private const double MainPlanMaxScale = 5.0;
        private const double MainPlanScaleStep = 0.1;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ClampMainPlanTransform();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClampMainPlanTransform();
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ToggleTheme();
        }

        private void MainMenu_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MainPlanViewport_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point cursor = e.GetPosition(MainPlanViewport);

            double currentScale = MainPlanScaleTransform.ScaleX;
            double nextScale = e.Delta > 0
                ? currentScale + MainPlanScaleStep
                : currentScale - MainPlanScaleStep;

            nextScale = Math.Clamp(nextScale, MainPlanMinScale, MainPlanMaxScale);

            if (Math.Abs(nextScale - currentScale) < 0.0001)
            {
                return;
            }

            double scaleRatio = nextScale / currentScale;

            MainPlanTranslateTransform.X =
                cursor.X - ((cursor.X - MainPlanTranslateTransform.X) * scaleRatio);

            MainPlanTranslateTransform.Y =
                cursor.Y - ((cursor.Y - MainPlanTranslateTransform.Y) * scaleRatio);

            MainPlanScaleTransform.ScaleX = nextScale;
            MainPlanScaleTransform.ScaleY = nextScale;

            ClampMainPlanTransform();

            e.Handled = true;
        }

        private void MainPlanViewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMainPlanDragging = true;
            _mainPlanDragStartPoint = e.GetPosition(MainPlanViewport);
            _mainPlanDragStartTranslate = new Point(
                MainPlanTranslateTransform.X,
                MainPlanTranslateTransform.Y);

            MainPlanViewport.CaptureMouse();
            MainPlanViewport.Cursor = Cursors.SizeAll;
            e.Handled = true;
        }

        private void MainPlanViewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMainPlanDragging)
            {
                return;
            }

            Point currentPoint = e.GetPosition(MainPlanViewport);
            Vector delta = currentPoint - _mainPlanDragStartPoint;

            MainPlanTranslateTransform.X = _mainPlanDragStartTranslate.X + delta.X;
            MainPlanTranslateTransform.Y = _mainPlanDragStartTranslate.Y + delta.Y;

            ClampMainPlanTransform();
        }

        private void MainPlanViewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndMainPlanDrag();
            e.Handled = true;
        }

        private void MainPlanViewport_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                EndMainPlanDrag();
            }
        }

        private void MainPlanViewport_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetMainPlanTransform();
            e.Handled = true;
        }

        private void EndMainPlanDrag()
        {
            if (!_isMainPlanDragging)
            {
                return;
            }

            _isMainPlanDragging = false;
            MainPlanViewport.ReleaseMouseCapture();
            MainPlanViewport.Cursor = Cursors.Hand;
        }

        private void ResetMainPlanTransform()
        {
            MainPlanScaleTransform.ScaleX = 1.0;
            MainPlanScaleTransform.ScaleY = 1.0;
            MainPlanTranslateTransform.X = 0.0;
            MainPlanTranslateTransform.Y = 0.0;

            ClampMainPlanTransform();
        }

        private void ClampMainPlanTransform()
        {
            if (MainPlanViewport == null || MainPlanContent == null)
            {
                return;
            }

            double viewportWidth = MainPlanViewport.ActualWidth;
            double viewportHeight = MainPlanViewport.ActualHeight;

            double scaledContentWidth = MainPlanContent.ActualWidth * MainPlanScaleTransform.ScaleX;
            double scaledContentHeight = MainPlanContent.ActualHeight * MainPlanScaleTransform.ScaleY;

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }

            if (scaledContentWidth <= viewportWidth)
            {
                MainPlanTranslateTransform.X = 0;
            }
            else
            {
                double minX = viewportWidth - scaledContentWidth;
                double maxX = 0;
                MainPlanTranslateTransform.X = Math.Clamp(MainPlanTranslateTransform.X, minX, maxX);
            }

            if (scaledContentHeight <= viewportHeight)
            {
                MainPlanTranslateTransform.Y = 0;
            }
            else
            {
                double minY = viewportHeight - scaledContentHeight;
                double maxY = 0;
                MainPlanTranslateTransform.Y = Math.Clamp(MainPlanTranslateTransform.Y, minY, maxY);
            }
        }
    }
}