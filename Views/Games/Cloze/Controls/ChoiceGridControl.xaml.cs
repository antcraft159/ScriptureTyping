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
    /// 선택지 목록을 카드 형태의 그리드로 보여주는 공용 컨트롤.
    /// 항목 개수에 따라 열 개수를 자동 계산한다.
    /// 
    /// 사용 예:
    /// <local:ChoiceGridControl Title="보기"
    ///                          ItemsSource="{Binding CurrentOptions}"
    ///                          MaxColumns="3"/>
    /// 
    /// 기본 전제:
    /// - ItemsSource 각 항목은 Text, Command, CommandParameter, IsEnabled 속성을 가지면 기본 템플릿으로 바로 사용 가능
    /// - 다른 모양이 필요하면 ItemTemplate 을 외부에서 넘겨서 교체 가능
    /// </summary>
    public partial class ChoiceGridControl : UserControl
    {
        private INotifyCollectionChanged? _notifyCollection;

        public ChoiceGridControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// 제목
        /// </summary>
        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ChoiceGridControl),
                new PropertyMetadata(string.Empty, OnTitleChanged));

        /// <summary>
        /// 바깥에서 넘기는 항목 목록
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(ChoiceGridControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        /// 외부에서 넘길 수 있는 항목 템플릿
        /// </summary>
        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(ChoiceGridControl),
                new PropertyMetadata(null, OnItemTemplateChanged));

        /// <summary>
        /// 최대 열 개수
        /// </summary>
        public int MaxColumns
        {
            get => (int)GetValue(MaxColumnsProperty);
            set => SetValue(MaxColumnsProperty, value);
        }

        public static readonly DependencyProperty MaxColumnsProperty =
            DependencyProperty.Register(
                nameof(MaxColumns),
                typeof(int),
                typeof(ChoiceGridControl),
                new PropertyMetadata(3, OnMaxColumnsChanged));

        /// <summary>
        /// 실제 표시할 열 개수
        /// </summary>
        public int DisplayColumns
        {
            get => (int)GetValue(DisplayColumnsProperty);
            private set => SetValue(DisplayColumnsPropertyKey, value);
        }

        private static readonly DependencyPropertyKey DisplayColumnsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(DisplayColumns),
                typeof(int),
                typeof(ChoiceGridControl),
                new PropertyMetadata(1));

        public static readonly DependencyProperty DisplayColumnsProperty =
            DisplayColumnsPropertyKey.DependencyProperty;

        /// <summary>
        /// 제목 존재 여부
        /// </summary>
        public bool HasTitle
        {
            get => (bool)GetValue(HasTitleProperty);
            private set => SetValue(HasTitlePropertyKey, value);
        }

        private static readonly DependencyPropertyKey HasTitlePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasTitle),
                typeof(bool),
                typeof(ChoiceGridControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasTitleProperty =
            HasTitlePropertyKey.DependencyProperty;

        /// <summary>
        /// 실제로 사용할 템플릿
        /// 외부 ItemTemplate 이 없으면 내부 기본 템플릿 사용
        /// </summary>
        public DataTemplate? EffectiveItemTemplate
        {
            get => (DataTemplate?)GetValue(EffectiveItemTemplateProperty);
            private set => SetValue(EffectiveItemTemplatePropertyKey, value);
        }

        private static readonly DependencyPropertyKey EffectiveItemTemplatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EffectiveItemTemplate),
                typeof(DataTemplate),
                typeof(ChoiceGridControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EffectiveItemTemplateProperty =
            EffectiveItemTemplatePropertyKey.DependencyProperty;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateTitleState();
            UpdateEffectiveItemTemplate();
            UpdateDisplayColumns();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachCollectionChanged();
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChoiceGridControl control)
            {
                control.UpdateTitleState();
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChoiceGridControl control)
            {
                control.DetachCollectionChanged();
                control.AttachCollectionChanged(e.NewValue as IEnumerable);
                control.UpdateDisplayColumns();
            }
        }

        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChoiceGridControl control)
            {
                control.UpdateEffectiveItemTemplate();
            }
        }

        private static void OnMaxColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChoiceGridControl control)
            {
                control.UpdateDisplayColumns();
            }
        }

        private void AttachCollectionChanged(IEnumerable? source)
        {
            if (source is INotifyCollectionChanged notifyCollection)
            {
                _notifyCollection = notifyCollection;
                _notifyCollection.CollectionChanged += OnItemsCollectionChanged;
            }
        }

        private void DetachCollectionChanged()
        {
            if (_notifyCollection != null)
            {
                _notifyCollection.CollectionChanged -= OnItemsCollectionChanged;
                _notifyCollection = null;
            }
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDisplayColumns();
        }

        private void UpdateTitleState()
        {
            HasTitle = !string.IsNullOrWhiteSpace(Title);
        }

        private void UpdateEffectiveItemTemplate()
        {
            EffectiveItemTemplate = ItemTemplate ?? (DataTemplate?)Resources["DefaultChoiceItemTemplate"];
        }

        private void UpdateDisplayColumns()
        {
            int itemCount = CountItems(ItemsSource);
            int maxColumns = MaxColumns < 1 ? 1 : MaxColumns;

            if (itemCount <= 0)
            {
                DisplayColumns = 1;
                return;
            }

            if (itemCount == 1)
            {
                DisplayColumns = 1;
                return;
            }

            if (itemCount == 2)
            {
                DisplayColumns = Math.Min(2, maxColumns);
                return;
            }

            DisplayColumns = Math.Min(maxColumns, itemCount);
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