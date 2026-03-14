using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScriptureTyping.Views.Behaviors
{
    /// <summary>
    /// 목적:
    /// TextBox에서 붙여넣기(Ctrl+V, Shift+Insert, Paste 명령)를 차단한다.
    /// </summary>
    public static class TextBoxPasteBlockBehavior
    {
        public static readonly DependencyProperty IsPasteBlockedProperty =
            DependencyProperty.RegisterAttached(
                "IsPasteBlocked",
                typeof(bool),
                typeof(TextBoxPasteBlockBehavior),
                new PropertyMetadata(false, OnIsPasteBlockedChanged));

        public static bool GetIsPasteBlocked(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPasteBlockedProperty);
        }

        public static void SetIsPasteBlocked(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPasteBlockedProperty, value);
        }

        private static void OnIsPasteBlockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
            {
                return;
            }

            bool isPasteBlocked = (bool)e.NewValue;

            if (isPasteBlocked)
            {
                DataObject.AddPastingHandler(textBox, OnPasting);
                textBox.PreviewKeyDown += OnPreviewKeyDown;
                textBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteExecuted, OnPasteCanExecute));
            }
            else
            {
                DataObject.RemovePastingHandler(textBox, OnPasting);
                textBox.PreviewKeyDown -= OnPreviewKeyDown;
                RemovePasteBindings(textBox);
            }
        }

        private static void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
            e.Handled = true;
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrlV = Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V;
            bool isShiftInsert = Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.Insert;

            if (isCtrlV || isShiftInsert)
            {
                e.Handled = true;
            }
        }

        private static void OnPasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private static void OnPasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private static void RemovePasteBindings(TextBox textBox)
        {
            for (int i = textBox.CommandBindings.Count - 1; i >= 0; i--)
            {
                if (textBox.CommandBindings[i].Command == ApplicationCommands.Paste)
                {
                    textBox.CommandBindings.RemoveAt(i);
                }
            }
        }
    }
}