using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ScriptureTyping.Converters
{
    /// <summary>
    /// 목적:
    /// ChoiceGroups 버튼 클릭 시
    /// [0] = ClozeChoiceGroupItem
    /// [1] = 현재 선택한 보기 문자열
    /// 형태의 object[]를 만들어 CommandParameter로 전달한다.
    /// </summary>
    public sealed class ChoiceGroupCommandParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object group = values.Length > 0 ? values[0] : Binding.DoNothing;
            object choice = values.Length > 1 ? values[1] : Binding.DoNothing;

            if (group is not ClozeChoiceGroupItem)
            {
                return Array.Empty<object>();
            }

            if (choice is not string)
            {
                return Array.Empty<object>();
            }

            return new object[] { group, choice };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}