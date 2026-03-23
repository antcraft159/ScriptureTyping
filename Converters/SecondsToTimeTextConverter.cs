using System;
using System.Globalization;
using System.Windows.Data;

namespace ScriptureTyping.Converters
{
    /// <summary>
    /// 목적:
    /// 초 단위 값을 mm:ss 또는 hh:mm:ss 문자열로 변환한다.
    /// </summary>
    public sealed class SecondsToTimeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double seconds = 0;

            if (value is double doubleValue)
            {
                seconds = doubleValue;
            }
            else if (value is float floatValue)
            {
                seconds = floatValue;
            }
            else if (value is int intValue)
            {
                seconds = intValue;
            }
            else if (value is long longValue)
            {
                seconds = longValue;
            }
            else if (value is decimal decimalValue)
            {
                seconds = (double)decimalValue;
            }
            else if (value is string text && double.TryParse(text, out double parsedValue))
            {
                seconds = parsedValue;
            }

            if (seconds < 0)
            {
                seconds = 0;
            }

            TimeSpan time = TimeSpan.FromSeconds(seconds);

            if (time.TotalHours >= 1)
            {
                return time.ToString(@"hh\:mm\:ss");
            }

            return time.ToString(@"mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}