using System;
using System.Globalization;

using System.Windows.Data;

namespace Reg2015.View.Convertors
{
    /// <summary>В архиве</summary>
    [ValueConversion(typeof(object), typeof(bool))]
    class NotNullToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
