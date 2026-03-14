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
            if (values == null || values.Length < 2)
            {
                return Array.Empty<object>();
            }

            object group = values[0];
            object choice = values[1];

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