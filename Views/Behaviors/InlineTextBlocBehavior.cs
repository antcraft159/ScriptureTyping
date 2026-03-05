using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ScriptureTyping.Views.Behaviors
{
    /// <summary>
    /// 목적: TextBlock에 Inline 목록(InlinePart 리스트)을 바인딩으로 주입하기 위한 Attached Property.
    /// 사용: behaviors:InlineTextBlockBehavior.InlinesSource="{Binding ...}"
    /// </summary>
    public static class InlineTextBlockBehavior
    {
        public static readonly DependencyProperty InlinesSourceProperty =
            DependencyProperty.RegisterAttached(
                "InlinesSource",
                typeof(IEnumerable<InlinePart>),
                typeof(InlineTextBlockBehavior),
                new PropertyMetadata(null, OnInlinesSourceChanged));

        public static void SetInlinesSource(DependencyObject element, IEnumerable<InlinePart> value)
            => element.SetValue(InlinesSourceProperty, value);

        public static IEnumerable<InlinePart> GetInlinesSource(DependencyObject element)
            => (IEnumerable<InlinePart>)element.GetValue(InlinesSourceProperty);

        private static void OnInlinesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock tb)
            {
                return;
            }

            tb.Inlines.Clear();

            if (e.NewValue is not IEnumerable<InlinePart> parts)
            {
                return;
            }

            foreach (InlinePart part in parts)
            {
                if (part.Text == "\n")
                {
                    tb.Inlines.Add(new LineBreak());
                    continue;
                }

                Run run = new Run(part.Text ?? string.Empty);

                if (part.IsError)
                {
                    run.TextDecorations = TextDecorations.Underline;
                    run.Foreground = Brushes.Red;
                }

                tb.Inlines.Add(run);
            }
        }
    }

    /// <summary>
    /// 목적: Inline 렌더링 1조각(텍스트 + 에러 여부)
    /// </summary>
    public sealed class InlinePart
    {
        public string Text { get; set; } = string.Empty;
        public bool IsError { get; set; }
    }
}