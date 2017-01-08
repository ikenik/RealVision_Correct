using System;

using System.Windows.Data;
using Reg2015.RVDataModel;

namespace Reg2015.View.Convertors
{
    /// <summary>Описание пациента</summary>
    [ValueConversion(typeof(tblPatient), typeof(string))]
    class PatientDesc : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            tblPatient xPatient = value as tblPatient;
            if (xPatient == null)
                return "";
            return string.Format("{0} {1} {2}",
                xPatient.FirstName,
                xPatient.LastName,
                xPatient.FatherName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
