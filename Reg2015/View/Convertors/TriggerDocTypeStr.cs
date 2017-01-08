using System;

using System.Windows.Data;

namespace Reg2015.View.Convertors
{
    /// <summary>Тип документа</summary>
    [ValueConversion(typeof(int), typeof(string))]
    class TriggerDocTypeStr : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int xTriggerDocTypet = (int)value;
            // см. Скалярные функции в БД
            switch (xTriggerDocTypet)
            {
                case 1: return "Услуга"; // GetDocumentServiceMask
                case 2: return "Чек"; // GetDocumentSalesReceiptMask
                case 4: return "Дог. прод."; // GetDocumentProductMask
                default: return "?";

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
