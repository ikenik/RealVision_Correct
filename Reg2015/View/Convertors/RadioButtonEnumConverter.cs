using System;
using System.Globalization;
using System.Windows.Data;
using Reg2015.RVDataModel;
using System.Windows;

namespace Reg2015.View.Convertors
{
    // [ValueConversion(typeof(Sex?), typeof(string))]
    class RadioButtonEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var xParameterString = parameter as string;
            if (xParameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object xParamValue = Enum.Parse(value.GetType(), xParameterString);
            return xParamValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var xParameterString = parameter as string;
            var xValueAsBool = (bool)value;

            if (xParameterString == null || !xValueAsBool)
                return DependencyProperty.UnsetValue;
            else
                return Enum.Parse(targetType, xParameterString);
        }
    }
}
