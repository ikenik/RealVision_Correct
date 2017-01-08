using System;
using System.Globalization;

using System.Windows.Data;
using Reg2015.RVDataModel;

namespace Reg2015.View.Convertors
{
    /// <summary>В архиве</summary>
    [ValueConversion(typeof(CardLocation), typeof(bool?))]
    class LocationToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            CardLocation xCardKind = (CardLocation)value;
            switch (xCardKind)
            {
                case CardLocation.Uncnown:
                    return null;
                case CardLocation.Receptopn:
                    return false;
                case CardLocation.Archive:
                    return true;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return CardLocation.Uncnown;
            if ((bool)value)
                return CardLocation.Archive;
            else
                return CardLocation.Receptopn;
        }
    }
}
