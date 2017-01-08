using System;
using System.Globalization;

using System.Windows.Data;
using Reg2015.RVDataModel;

namespace Reg2015.View.Convertors
{
    /// <summary>В архиве</summary>
    [ValueConversion(typeof(CardKind), typeof(bool?))]
    class CardKindToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            CardKind xCardKind = (CardKind)value;
            switch (xCardKind)
            {
                case CardKind.ForСataract:
                    return true;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return CardKind.Default;
            if ((bool)value)
                return CardKind.ForСataract;
            else
                return CardKind.Default;
        }
    }
}
