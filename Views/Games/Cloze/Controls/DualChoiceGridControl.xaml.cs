using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ScriptureTyping.Views.Games.Cloze.Controls
{
    /// <summary>
    /// 목적:
    /// 위/아래 2개의 선택지 그룹을 보여주는 공용 컨트롤.
    /// 보통/어려움 단계처럼 선택지 그룹이 2세트일 때 사용한다.
    ///
    /// 사용 예:
    /// <local:DualChoiceGridControl TopTitle="1번 보기"
    ///                              BottomTitle="2번 보기"
    ///                              TopItemsSource="{Binding FirstBlankOptions}"
    ///                              BottomItemsSource="{Binding SecondBlankOptions}"
    ///                              TopMaxColumns="3"
    ///                              BottomMaxColumns="3"/>
    ///
    /// 기본 전제:
    /// - 각 ItemsSource 항목은 Text, Command, CommandParameter, IsEnabled 속성을 가지면 기본 템플릿으로 바로 사용 가능
    /// - 다른 UI가 필요하면 TopItemTemplate, BottomItemTemplate 으로 외부 템플릿 교체 가능
    /// </summary>
    public partial class DualChoiceGridControl : UserControl
    {
        private INotifyCollectionChanged? _topNotifyCollection;
        private INotifyCollectionChanged? _bottomNotifyCollection;

        public DualChoiceGridControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// 위쪽 제목
        /// </summary>
        public string? TopTitle
        {
            get => (string?)GetValue(TopTitleProperty);
            set => SetValue(TopTitleProperty, value);
        }

        public static readonly DependencyProperty TopTitleProperty =
            DependencyProperty.Register(
                nameof(TopTitle),
                typeof(string),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(string.Empty, OnTopTitleChanged));

        /// <summary>
        /// 아래쪽 제목
        /// </summary>
        public string? BottomTitle
        {
            get => (string?)GetValue(BottomTitleProperty);
            set => SetValue(BottomTitleProperty, value);
        }

        public static readonly DependencyProperty BottomTitleProperty =
            DependencyProperty.Register(
                nameof(BottomTitle),
                typeof(string),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(string.Empty, OnBottomTitleChanged));

        /// <summary>
        /// 위쪽 항목 목록
        /// </summary>
        public IEnumerable? TopItemsSource
        {
            get => (IEnumerable?)GetValue(TopItemsSourceProperty);
            set => SetValue(TopItemsSourceProperty, value);
        }

        public static readonly DependencyProperty TopItemsSourceProperty =
            DependencyProperty.Register(
                nameof(TopItemsSource),
                typeof(IEnumerable),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(null, OnTopItemsSourceChanged));

        /// <summary>
        /// 아래쪽 항목 목록
        /// </summary>
        public IEnumerable? BottomItemsSource
        {
            get => (IEnumerable?)GetValue(BottomItemsSourceProperty);
            set => SetValue(BottomItemsSourceProperty, value);
        }

        public static readonly DependencyProperty BottomItemsSourceProperty =
            DependencyProperty.Register(
                nameof(BottomItemsSource),
                typeof(IEnumerable),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(null, OnBottomItemsSourceChanged));

        /// <summary>
        /// 위쪽 외부 항목 템플릿
        /// </summary>
        public DataTemplate? TopItemTemplate
        {
            get => (DataTemplate?)GetValue(TopItemTemplateProperty);
            set => SetValue(TopItemTemplateProperty, value);
        }

        public static readonly DependencyProperty TopItemTemplateProperty =
            DependencyProperty.Register(
                nameof(TopItemTemplate),
                typeof(DataTemplate),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(null, OnTopItemTemplateChanged));

        /// <summary>
        /// 아래쪽 외부 항목 템플릿
        /// </summary>
        public DataTemplate? BottomItemTemplate
        {
            get => (DataTemplate?)GetValue(BottomItemTemplateProperty);
            set => SetValue(BottomItemTemplateProperty, value);
        }

        public static readonly DependencyProperty BottomItemTemplateProperty =
            DependencyProperty.Register(
                nameof(BottomItemTemplate),
                typeof(DataTemplate),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(null, OnBottomItemTemplateChanged));

        /// <summary>
        /// 위쪽 최대 열 개수
        /// </summary>
        public int TopMaxColumns
        {
            get => (int)GetValue(TopMaxColumnsProperty);
            set => SetValue(TopMaxColumnsProperty, value);
        }

        public static readonly DependencyProperty TopMaxColumnsProperty =
            DependencyProperty.Register(
                nameof(TopMaxColumns),
                typeof(int),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(3, OnTopMaxColumnsChanged));

        /// <summary>
        /// 아래쪽 최대 열 개수
        /// </summary>
        public int BottomMaxColumns
        {
            get => (int)GetValue(BottomMaxColumnsProperty);
            set => SetValue(BottomMaxColumnsProperty, value);
        }

        public static readonly DependencyProperty BottomMaxColumnsProperty =
            DependencyProperty.Register(
                nameof(BottomMaxColumns),
                typeof(int),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(3, OnBottomMaxColumnsChanged));

        /// <summary>
        /// 위쪽 실제 표시 열 수
        /// </summary>
        public int TopDisplayColumns
        {
            get => (int)GetValue(TopDisplayColumnsProperty);
            private set => SetValue(TopDisplayColumnsPropertyKey, value);
        }

        private static readonly DependencyPropertyKey TopDisplayColumnsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TopDisplayColumns),
                typeof(int),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(1));

        public static readonly DependencyProperty TopDisplayColumnsProperty =
            TopDisplayColumnsPropertyKey.DependencyProperty;

        /// <summary>
        /// 아래쪽 실제 표시 열 수
        /// </summary>
        public int BottomDisplayColumns
        {
            get => (int)GetValue(BottomDisplayColumnsProperty);
            private set => SetValue(BottomDisplayColumnsPropertyKey, value);
        }

        private static readonly DependencyPropertyKey BottomDisplayColumnsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(BottomDisplayColumns),
                typeof(int),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(1));

        public static readonly DependencyProperty BottomDisplayColumnsProperty =
            BottomDisplayColumnsPropertyKey.DependencyProperty;

        /// <summary>
        /// 위쪽 제목 존재 여부
        /// </summary>
        public bool HasTopTitle
        {
            get => (bool)GetValue(HasTopTitleProperty);
            private set => SetValue(HasTopTitlePropertyKey, value);
        }

        private static readonly DependencyPropertyKey HasTopTitlePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasTopTitle),
                typeof(bool),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasTopTitleProperty =
            HasTopTitlePropertyKey.DependencyProperty;

        /// <summary>
        /// 아래쪽 제목 존재 여부
        /// </summary>
        public bool HasBottomTitle
        {
            get => (bool)GetValue(HasBottomTitleProperty);
            private set => SetValue(HasBottomTitlePropertyKey, value);
        }

        private static readonly DependencyPropertyKey HasBottomTitlePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasBottomTitle),
                typeof(bool),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasBottomTitleProperty =
            HasBottomTitlePropertyKey.DependencyProperty;

        /// <summary>
        /// 위쪽 실제 템플릿
        /// </summary>
        public DataTemplate? EffectiveTopItemTemplate
        {
            get => (DataTemplate?)GetValue(EffectiveTopItemTemplateProperty);
            private set => SetValue(EffectiveTopItemTemplatePropertyKey, value);
        }

        private static readonly DependencyPropertyKey EffectiveTopItemTemplatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EffectiveTopItemTemplate),
                typeof(DataTemplate),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EffectiveTopItemTemplateProperty =
            EffectiveTopItemTemplatePropertyKey.DependencyProperty;

        /// <summary>
        /// 아래쪽 실제 템플릿
        /// </summary>
        public DataTemplate? EffectiveBottomItemTemplate
        {
            get => (DataTemplate?)GetValue(EffectiveBottomItemTemplateProperty);
            private set => SetValue(EffectiveBottomItemTemplatePropertyKey, value);
        }

        private static readonly DependencyPropertyKey EffectiveBottomItemTemplatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EffectiveBottomItemTemplate),
                typeof(DataTemplate),
                typeof(DualChoiceGridControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EffectiveBottomItemTemplateProperty =
            EffectiveBottomItemTemplatePropertyKey.DependencyProperty;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateTopTitleState();
            UpdateBottomTitleState();
            UpdateEffectiveTopItemTemplate();
            UpdateEffectiveBottomItemTemplate();
            UpdateTopDisplayColumns();
            UpdateBottomDisplayColumns();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachTopCollectionChanged();
            DetachBottomCollectionChanged();
        }

        private static void OnTopTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.UpdateTopTitleState();
            }
        }

        private static void OnBottomTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.UpdateBottomTitleState();
            }
        }

        private static void OnTopItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.DetachTopCollectionChanged();
                control.AttachTopCollectionChanged(e.NewValue as IEnumerable);
                control.UpdateTopDisplayColumns();
            }
        }

        private static void OnBottomItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.DetachBottomCollectionChanged();
                control.AttachBottomCollectionChanged(e.NewValue as IEnumerable);
                control.UpdateBottomDisplayColumns();
            }
        }

        private static void OnTopItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.UpdateEffectiveTopItemTemplate();
            }
        }

        private static void OnBottomItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.UpdateEffectiveBottomItemTemplate();
            }
        }

        private static void OnTopMaxColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.UpdateTopDisplayColumns();
            }
        }

        private static void OnBottomMaxColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DualChoiceGridControl control)
            {
                control.UpdateBottomDisplayColumns();
            }
        }

        private void AttachTopCollectionChanged(IEnumerable? source)
        {
            if (source is INotifyCollectionChanged notifyCollection)
            {
                _topNotifyCollection = notifyCollection;
                _topNotifyCollection.CollectionChanged += OnTopItemsCollectionChanged;
            }
        }

        private void AttachBottomCollectionChanged(IEnumerable? source)
        {
            if (source is INotifyCollectionChanged notifyCollection)
            {
                _bottomNotifyCollection = notifyCollection;
                _bottomNotifyCollection.CollectionChanged += OnBottomItemsCollectionChanged;
            }
        }

        private void DetachTopCollectionChanged()
        {
            if (_topNotifyCollection != null)
            {
                _topNotifyCollection.CollectionChanged -= OnTopItemsCollectionChanged;
                _topNotifyCollection = null;
            }
        }

        private void DetachBottomCollectionChanged()
        {
            if (_bottomNotifyCollection != null)
            {
                _bottomNotifyCollection.CollectionChanged -= OnBottomItemsCollectionChanged;
                _bottomNotifyCollection = null;
            }
        }

        private void OnTopItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateTopDisplayColumns();
        }

        private void OnBottomItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateBottomDisplayColumns();
        }

        private void UpdateTopTitleState()
        {
            HasTopTitle = !string.IsNullOrWhiteSpace(TopTitle);
        }

        private void UpdateBottomTitleState()
        {
            HasBottomTitle = !string.IsNullOrWhiteSpace(BottomTitle);
        }

        private void UpdateEffectiveTopItemTemplate()
        {
            EffectiveTopItemTemplate = TopItemTemplate ?? (DataTemplate?)Resources["DefaultChoiceItemTemplate"];
        }

        private void UpdateEffectiveBottomItemTemplate()
        {
            EffectiveBottomItemTemplate = BottomItemTemplate ?? (DataTemplate?)Resources["DefaultChoiceItemTemplate"];
        }

        private void UpdateTopDisplayColumns()
        {
            TopDisplayColumns = CalculateDisplayColumns(TopItemsSource, TopMaxColumns);
        }

        private void UpdateBottomDisplayColumns()
        {
            BottomDisplayColumns = CalculateDisplayColumns(BottomItemsSource, BottomMaxColumns);
        }

        private static int CalculateDisplayColumns(IEnumerable? itemsSource, int maxColumns)
        {
            int itemCount = CountItems(itemsSource);
            int safeMaxColumns = maxColumns < 1 ? 1 : maxColumns;

            if (itemCount <= 0)
            {
                return 1;
            }

            if (itemCount == 1)
            {
                return 1;
            }

            if (itemCount == 2)
            {
                return Math.Min(2, safeMaxColumns);
            }

            return Math.Min(safeMaxColumns, itemCount);
        }

        private static int CountItems(IEnumerable? itemsSource)
        {
            if (itemsSource == null)
            {
                return 0;
            }

            if (itemsSource is ICollection collection)
            {
                return collection.Count;
            }

            return itemsSource.Cast<object>().Count();
        }
    }
}