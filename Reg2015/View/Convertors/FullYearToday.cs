using System;


using System.Windows.Data;

namespace Reg2015.View.Convertors
{
    /// <summary>Полных лет сегодня</summary>
    [ValueConversion(typeof(DateTime), typeof(int?))]
    class FullYearToday : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime? xBirthDateObj = value as DateTime?;
            if (xBirthDateObj == null)
                return null;
            DateTime xBirthDate = xBirthDateObj.Value;
            DateTime xNow = DateTime.Now;
            int? xFullYear;
            xFullYear = xNow.Year - xBirthDate.Year;
            if ((xBirthDate.Month * 100 + xBirthDate.Day) > (xNow.Month * 100 + xNow.Day))
                xFullYear--;
            return xFullYear;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
