using System;
using System.Globalization;

using System.Windows.Data;
using Reg2015.RVDataModel;

namespace Reg2015.View.Convertors
{

    /// <summary>Полных лет сегодня</summary>
    [ValueConversion(typeof(CardKind), typeof(string))]
    class CardKindDescription : IValueConverter
    {
        private string LS_Default = "Карточка общая";
        private string LS_ForСataract = "Карточка для катаракиы";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CardKind xCardKind = (CardKind)value;
            switch (xCardKind)
            {
                case CardKind.ForСataract:
                    return LS_ForСataract;
                default:
                    return LS_Default;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string xCardKindDesc = (string)value;
            if (xCardKindDesc == LS_ForСataract)
                return CardKind.ForСataract;
            else
                return CardKind.Default;
        }
    }
}
