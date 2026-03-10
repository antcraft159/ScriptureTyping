using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScriptureTyping.Views.Games.Cloze.Controls
{
    /// <summary>
    /// 목적:
    /// 2개의 입력창을 위/아래로 제공하는 재사용 가능한 UserControl.
    /// 
    /// 사용 예:
    /// - 보통 난이도: 가린 단어 2개를 각각 입력
    /// - 어려움 난이도: 비슷한 보기와 함께 정답 단어 2개 입력
    /// 
    /// 특징:
    /// - 각 입력창 제목/placeholder/text 바인딩 가능
    /// - Enter 키로 위 -> 아래 입력창 포커스 이동
    /// - 아래 입력창에서 Enter 입력 시 제출 이벤트 발생
    /// </summary>
    public partial class DualInputControl : UserControl
    {
        public DualInputControl()
        {
            InitializeComponent();
        }

        #region DependencyProperty - FirstTitle
        public static readonly DependencyProperty FirstTitleProperty =
            DependencyProperty.Register(
                nameof(FirstTitle),
                typeof(string),
                typeof(DualInputControl),
                new PropertyMetadata("첫 번째 답"));

        public string FirstTitle
        {
            get => (string)GetValue(FirstTitleProperty);
            set => SetValue(FirstTitleProperty, value);
        }
        #endregion

        #region DependencyProperty - SecondTitle
        public static readonly DependencyProperty SecondTitleProperty =
            DependencyProperty.Register(
                nameof(SecondTitle),
                typeof(string),
                typeof(DualInputControl),
                new PropertyMetadata("두 번째 답"));

        public string SecondTitle
        {
            get => (string)GetValue(SecondTitleProperty);
            set => SetValue(SecondTitleProperty, value);
        }
        #endregion

        #region DependencyProperty - FirstText
        public static readonly DependencyProperty FirstTextProperty =
            DependencyProperty.Register(
                nameof(FirstText),
                typeof(string),
                typeof(DualInputControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string FirstText
        {
            get => (string)GetValue(FirstTextProperty);
            set => SetValue(FirstTextProperty, value);
        }
        #endregion

        #region DependencyProperty - SecondText
        public static readonly DependencyProperty SecondTextProperty =
            DependencyProperty.Register(
                nameof(SecondText),
                typeof(string),
                typeof(DualInputControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SecondText
        {
            get => (string)GetValue(SecondTextProperty);
            set => SetValue(SecondTextProperty, value);
        }
        #endregion

        #region DependencyProperty - FirstPlaceholder
        public static readonly DependencyProperty FirstPlaceholderProperty =
            DependencyProperty.Register(
                nameof(FirstPlaceholder),
                typeof(string),
                typeof(DualInputControl),
                new PropertyMetadata("첫 번째 정답을 입력하세요"));

        public string FirstPlaceholder
        {
            get => (string)GetValue(FirstPlaceholderProperty);
            set => SetValue(FirstPlaceholderProperty, value);
        }
        #endregion

        #region DependencyProperty - SecondPlaceholder
        public static readonly DependencyProperty SecondPlaceholderProperty =
            DependencyProperty.Register(
                nameof(SecondPlaceholder),
                typeof(string),
                typeof(DualInputControl),
                new PropertyMetadata("두 번째 정답을 입력하세요"));

        public string SecondPlaceholder
        {
            get => (string)GetValue(SecondPlaceholderProperty);
            set => SetValue(SecondPlaceholderProperty, value);
        }
        #endregion

        #region DependencyProperty - IsFirstEnabled
        public static readonly DependencyProperty IsFirstEnabledProperty =
            DependencyProperty.Register(
                nameof(IsFirstEnabled),
                typeof(bool),
                typeof(DualInputControl),
                new PropertyMetadata(true));

        public bool IsFirstEnabled
        {
            get => (bool)GetValue(IsFirstEnabledProperty);
            set => SetValue(IsFirstEnabledProperty, value);
        }
        #endregion

        #region DependencyProperty - IsSecondEnabled
        public static readonly DependencyProperty IsSecondEnabledProperty =
            DependencyProperty.Register(
                nameof(IsSecondEnabled),
                typeof(bool),
                typeof(DualInputControl),
                new PropertyMetadata(true));

        public bool IsSecondEnabled
        {
            get => (bool)GetValue(IsSecondEnabledProperty);
            set => SetValue(IsSecondEnabledProperty, value);
        }
        #endregion

        #region DependencyProperty - FirstMaxLength
        public static readonly DependencyProperty FirstMaxLengthProperty =
            DependencyProperty.Register(
                nameof(FirstMaxLength),
                typeof(int),
                typeof(DualInputControl),
                new PropertyMetadata(50));

        public int FirstMaxLength
        {
            get => (int)GetValue(FirstMaxLengthProperty);
            set => SetValue(FirstMaxLengthProperty, value);
        }
        #endregion

        #region DependencyProperty - SecondMaxLength
        public static readonly DependencyProperty SecondMaxLengthProperty =
            DependencyProperty.Register(
                nameof(SecondMaxLength),
                typeof(int),
                typeof(DualInputControl),
                new PropertyMetadata(50));

        public int SecondMaxLength
        {
            get => (int)GetValue(SecondMaxLengthProperty);
            set => SetValue(SecondMaxLengthProperty, value);
        }
        #endregion

        #region Brush Properties
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register(
                nameof(BackgroundBrush),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(35, 38, 46))));

        public Brush BackgroundBrush
        {
            get => (Brush)GetValue(BackgroundBrushProperty);
            set => SetValue(BackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty BorderBrushExProperty =
            DependencyProperty.Register(
                nameof(BorderBrushEx),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(70, 74, 84))));

        public Brush BorderBrushEx
        {
            get => (Brush)GetValue(BorderBrushExProperty);
            set => SetValue(BorderBrushExProperty, value);
        }

        public static readonly DependencyProperty InputBackgroundBrushProperty =
            DependencyProperty.Register(
                nameof(InputBackgroundBrush),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(24, 26, 32))));

        public Brush InputBackgroundBrush
        {
            get => (Brush)GetValue(InputBackgroundBrushProperty);
            set => SetValue(InputBackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty InputBorderBrushProperty =
            DependencyProperty.Register(
                nameof(InputBorderBrush),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(82, 88, 102))));

        public Brush InputBorderBrush
        {
            get => (Brush)GetValue(InputBorderBrushProperty);
            set => SetValue(InputBorderBrushProperty, value);
        }

        public static readonly DependencyProperty TitleForegroundProperty =
            DependencyProperty.Register(
                nameof(TitleForeground),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(Brushes.White));

        public Brush TitleForeground
        {
            get => (Brush)GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }

        public static readonly DependencyProperty InputForegroundProperty =
            DependencyProperty.Register(
                nameof(InputForeground),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(Brushes.White));

        public Brush InputForeground
        {
            get => (Brush)GetValue(InputForegroundProperty);
            set => SetValue(InputForegroundProperty, value);
        }

        public static readonly DependencyProperty PlaceholderForegroundProperty =
            DependencyProperty.Register(
                nameof(PlaceholderForeground),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(145, 150, 160))));

        public Brush PlaceholderForeground
        {
            get => (Brush)GetValue(PlaceholderForegroundProperty);
            set => SetValue(PlaceholderForegroundProperty, value);
        }

        public static readonly DependencyProperty CaretBrushExProperty =
            DependencyProperty.Register(
                nameof(CaretBrushEx),
                typeof(Brush),
                typeof(DualInputControl),
                new PropertyMetadata(Brushes.White));

        public Brush CaretBrushEx
        {
            get => (Brush)GetValue(CaretBrushExProperty);
            set => SetValue(CaretBrushExProperty, value);
        }
        #endregion

        #region RoutedEvent - SubmitRequested
        public static readonly RoutedEvent SubmitRequestedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(SubmitRequested),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(DualInputControl));

        public event RoutedEventHandler SubmitRequested
        {
            add => AddHandler(SubmitRequestedEvent, value);
            remove => RemoveHandler(SubmitRequestedEvent, value);
        }
        #endregion

        /// <summary>
        /// 첫 번째 입력창 Enter:
        /// 아래 입력창으로 포커스 이동
        /// </summary>
        private void OnFirstTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            e.Handled = true;
            PART_SecondTextBox.Focus();
            PART_SecondTextBox.SelectAll();
        }

        /// <summary>
        /// 두 번째 입력창 Enter:
        /// 외부로 제출 이벤트 발생
        /// </summary>
        private void OnSecondTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            e.Handled = true;
            RaiseEvent(new RoutedEventArgs(SubmitRequestedEvent, this));
        }

        /// <summary>
        /// 외부에서 첫 번째 입력창에 포커스를 주고 싶을 때 사용
        /// </summary>
        public void FocusFirstInput()
        {
            PART_FirstTextBox.Focus();
            PART_FirstTextBox.SelectAll();
        }

        /// <summary>
        /// 외부에서 두 번째 입력창에 포커스를 주고 싶을 때 사용
        /// </summary>
        public void FocusSecondInput()
        {
            PART_SecondTextBox.Focus();
            PART_SecondTextBox.SelectAll();
        }

        /// <summary>
        /// 두 입력값 초기화
        /// </summary>
        public void ClearInputs()
        {
            FirstText = string.Empty;
            SecondText = string.Empty;
        }
    }
}