using System;

using System.Linq;
using System.Text;

using Reg2015.RVDataModel;

using System.Globalization;
using System.Diagnostics;

namespace Reg2015.Reports
{

    internal class Formater
    {
        private static string[] UNITS = { " ", "один ", "два ", "три ", "четыре ", "пять ", "шесть ", "семь ", "восемь ", "девять ", "десять ", "одиннадцать ", "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ", "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать " };
        private static string[] DECADES = { "", "", "двадцать ", "тридцать ", "сорок ", "пятьдесят ", "шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто " };
        private static string[] HUNDREDS = { "", "сто ", "двести ", "триста ", "четыреста ", "пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот " };
        private static string[] THOUSANDS = { "", "одна ", "две ", "три ", "четыре ", "пять ", "шесть ", "семь ", "восемь ", "девять ", "десять ", "одиннадцать ", "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ", "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать " };
        /// <summary>Падежи рублей</summary>
        /// <remarks>Именительный, родительный, родительный множествное число</remarks>
        private static string[] RUB_CASE = { "рубль", "рубля", "рублей" };
        /// <summary>Падежи тысяч</summary>
        /// <remarks>Именительный, родительный, родительный множествное число</remarks>
        private static string[] THOUSANDS_CASE = { "тысяча ", "тысячи ", "тысяч " };

        private static CultureInfo FLocale = new System.Globalization.CultureInfo("ru-Ru");
        //private static List<tblDocReportMeta> FDocReportMeta;
        //protected static List<tblDocReportMeta> DocReportMeta
        //{
        //    get
        //    {
        //        if (FDocReportMeta == null)
        //            using (var xContext = new RVDataModel.RealVisionDataContext())
        //                FDocReportMeta = xContext.tblDocReportMetas.AsNoTracking().ToList();
        //        return FDocReportMeta;
        //    }
        //}

        private static string GetDocFormats(int id)
        {
            return "{0}";
            //var xFmt = DocReportMeta.Find(fmt => fmt.ID == id);
            //return (xFmt == null) ? "{0}" : xFmt.Text;
        }

        ///// <summary>
        ///// Получить текст длительности обследования после операции
        ///// </summary>
        ///// <param name="id">ID tblDocReportMeta</param>
        //public static string GetMonitorText(int id)
        //{
        //    var xFmt = DocReportMeta.Find(fmt => fmt.ID == id);
        //    return (xFmt == null) ? "{0}" : xFmt.Text;
        //}

        //public static string ServiceNameFormate(tblDocumentService doc)
        //{
        //    return doc.FormeterID == null ? doc.Name : string.Format(GetDocFormats(doc.FormeterID.Value), doc.Name);
        //}

        public static string MoneyToStr(int value)
        {
            if (value >= 1000000)
                return "больше миллиона!";

            int xDigitCount = 0, tmp = value;
            int[] xDigits = new int[6];
            while (tmp > 0)
            {
                xDigits[xDigitCount] = tmp % 10; // остаток от деления
                tmp = tmp / 10;        // целочисленное деление
                xDigitCount++;
            }



            StringBuilder result = new StringBuilder();
            string xEnd = "";

            if (xDigitCount >= 4)
            {
                // формируем тысячи
                tmp = xDigits[4] * 10 + xDigits[3];
                if ((tmp >= 10) && (tmp <= 19))
                {
                    result.Append(HUNDREDS[xDigits[5]]);
                    result.Append(THOUSANDS[tmp]);
                }
                else
                {
                    result.Append(HUNDREDS[xDigits[5]]);
                    result.Append(DECADES[xDigits[4]]);
                    result.Append(THOUSANDS[xDigits[3]]);
                }
                // определяем окончание
                if (tmp >= 20)
                    tmp = xDigits[3];
                switch (tmp)
                {
                    case 1: xEnd = THOUSANDS_CASE[0]; break;
                    case 2:
                    case 3:
                    case 4: xEnd = THOUSANDS_CASE[1]; break;
                    default: xEnd = THOUSANDS_CASE[2]; break;
                }
                result.Append(xEnd);
            }

            // формируем единицы 
            tmp = xDigits[1] * 10 + xDigits[0];
            if ((tmp >= 10) && (tmp <= 19))
            {
                result.Append(HUNDREDS[xDigits[2]]);
                result.Append(UNITS[tmp]);
            }
            else
            {
                result.Append(HUNDREDS[xDigits[2]]);
                result.Append(DECADES[xDigits[1]]);
                result.Append(UNITS[xDigits[0]]);
            }
            // определяем окончание РУБЛЬ
            if (tmp >= 20)
                tmp = xDigits[0];
            switch (tmp)
            {
                case 1: xEnd = RUB_CASE[0]; break;
                case 2:
                case 3:
                case 4: xEnd = RUB_CASE[1]; break;
                default: xEnd = RUB_CASE[2]; break; // 0, 5 - 19
            }
            result.Append(xEnd);
            return result.ToString(); ;
        }
        /// <summary>
        /// Дата время в строку в русской локали
        /// </summary>
        public static string DateTymeToStr(DateTime value, string format = null)
        {
            return value.ToString(format, FLocale);
        }

        /// <summary>
        /// Пол в строку
        /// </summary>
        public static string SexToStr(Sex? sex)
        {
            switch (sex)
            {
                case Sex.Man: return "муж.";
                case Sex.Woman: return "жен.";
                default: return "муж. - 1, жен. - 2";
            }
        }

    }

    public class CardDataAdapter
    {

        private tblPatientInfo FPatient;
        public CardDataAdapter(tblPatientInfo patient)
        {
            FPatient = patient;
        }

        public DateTime DateCreate { get { return FPatient.DateCreate; } }
        public string DateCreateStr { get { return Formater.DateTymeToStr(DateCreate, "dd MMMM yyyy г."); } }
        public string FirstName { get { return FPatient.FirstName; } }
        public string LastName { get { return FPatient.LastName; } }
        public string FatherName { get { return FPatient.FatherName; } }
        public DateTime BirthDay { get { return FPatient.BirthDay ?? new DateTime(); } }
        public string BirthDayStr { get { return Formater.DateTymeToStr(BirthDay, "dd.MM.yyyy"); } }
        public string Address { get { return FPatient.Address; } }
        public string Job { get { return FPatient.Job; } }
        public string Post { get { return FPatient.Post; } }
        public string Phon { get { return FPatient.Phon; } }
        public int Number { get { return FPatient.Number; } }
        public bool ForСataract { get { return FPatient.Kind == CardKind.ForСataract; } }
    }

    /// <summary>
    /// Адаптер для карьты со всеми полями адреса. 
    /// </summary>
    /// <remarks>Этот адаптер не реализован, нужно подключить КЛАДР или ФИАС</remarks>
    public class CardFormN_025uAdapter_
    {
        private tblPatientInfo FPatient;
        public CardFormN_025uAdapter_(tblPatientInfo patient)
        {
            FPatient = patient;
        }

        public DateTime DateCreate { get { return FPatient.DateCreate; } }
        public string DateCreateStr { get { return Formater.DateTymeToStr(DateCreate, "dd MMMM yyyy г."); } }
        public string FirstName { get { return FPatient.FirstName; } }
        public string LastName { get { return FPatient.LastName; } }
        public string FatherName { get { return FPatient.FatherName; } }
        public DateTime BirthDay { get { return FPatient.BirthDay ?? new DateTime(); } }
        public string BirthDayStr { get { return Formater.DateTymeToStr(BirthDay, "dd.MM.yyyy"); } }

        public string Job { get { return FPatient.Job; } }
        public string Post { get { return FPatient.Post; } }
        public string Phon { get { return FPatient.Phon; } }
        public int Number { get { return FPatient.Number; } }
        public bool ForСataract { get { return FPatient.Kind == CardKind.ForСataract; } }

        public string Address { get { return FPatient.Address; } }

        /// <summary>Пол</summary>
        public string Sex { get { return Formater.SexToStr(FPatient.Sex); } }

        /// <summary>Cубъект Российской Федерации</summary>
        public string Subject { get { throw new NotImplementedException(); } }

        /// <summary>Район</summary>
        public string District { get { return "#Реализовать"; } }
        /// <summary>Город</summary>
        public string City { get { return "#Реализовать"; } }
        /// <summary>Населенный пункт</summary>
        public string InhabitedLocality { get { return "#Реализовать"; } }
        /// <summary>Улица</summary>
        public string Street { get { return "#Реализовать"; } }
        /// <summary>Дом</summary>
        public string House { get { return "#Реализовать"; } }
        /// <summary>Квартира</summary>
        public string Flat { get { return "#Реализовать"; } }

        /// <summary>Местность</summary>
        public string Terrain { get { return "городская - 1, сельская - 2"; } }


    }

    public class CardFormN_025uAdapter
    {
        private tblPatientInfo FPatient;
        public CardFormN_025uAdapter(tblPatientInfo patient)
        {
            FPatient = patient;
        }

        public DateTime DateCreate { get { return FPatient.DateCreate; } }
        public string DateCreateStr { get { return Formater.DateTymeToStr(DateCreate, "dd MMMM yyyy г."); } }
        public string FirstName { get { return FPatient.FirstName; } }
        public string LastName { get { return FPatient.LastName; } }
        public string FatherName { get { return FPatient.FatherName; } }
        public DateTime BirthDay { get { return FPatient.BirthDay ?? new DateTime(); } }
        public string BirthDayStr { get { return Formater.DateTymeToStr(BirthDay, "dd.MM.yyyy"); } }

        public string Job { get { return FPatient.Job; } }
        public string Post { get { return FPatient.Post; } }
        public string Phon { get { return FPatient.Phon; } }
        public int Number { get { return FPatient.Number; } }
        public bool ForСataract { get { return FPatient.Kind == CardKind.ForСataract; } }

        public string Address { get { return FPatient.Address; } }

        /// <summary>Пол</summary>
        public string Sex { get { return Formater.SexToStr(FPatient.Sex); } }

        ///// <summary>Cубъект Российской Федерации</summary>
        //public string Subject { get { throw new NotImplementedException(); } }

        ///// <summary>Район</summary>
        //public string District { get { return "#Реализовать"; } }
        ///// <summary>Город</summary>
        //public string City { get { return "#Реализовать"; } }
        ///// <summary>Населенный пункт</summary>
        //public string InhabitedLocality { get { return "#Реализовать"; } }
        ///// <summary>Улица</summary>
        //public string Street { get { return "#Реализовать"; } }
        ///// <summary>Дом</summary>
        //public string House { get { return "#Реализовать"; } }
        ///// <summary>Квартира</summary>
        //public string Flat { get { return "#Реализовать"; } }

        ///// <summary>Местность</summary>
        //public string Terrain { get { return "городская - 1, сельская - 2"; } }


    }

    public class DocumentAdapter
    {
        private tblDocumentCommon FDocumrnt;
        private tblOffer FBaseOffer;
        private tblOffer GetBaseOffer()
        {
            if (FBaseOffer == null)
                FBaseOffer = FDocumrnt.tblManipulation.tblOffer;
            return FBaseOffer;
        }
        public DocumentAdapter(tblDocumentCommon documrnt)
        {
            FDocumrnt = documrnt;

        }

        public int DocNumber { get { return FDocumrnt.Number; } }
        public string FirstName { get { return FDocumrnt.FirstName; } }
        public string LastName { get { return FDocumrnt.LastName; } }
        public string FatherName { get { return FDocumrnt.FatherName; } }
        public string FullName { get { return string.Format("{0} {1} {2}", FDocumrnt.FirstName, FDocumrnt.LastName, FDocumrnt.FatherName); } }
        public string ShortName { get { return FDocumrnt.ShortName; } }
        public string Address { get { return FDocumrnt.Address; } }
        public string Phon { get { return FDocumrnt.Phon; } }
        public string PaspSeriya { get { return FDocumrnt.PaspSeriya; } }
        public string PaspNumber { get { return FDocumrnt.PaspNumber; } }
        public string PaspIssuing { get { return FDocumrnt.PaspIssuing; } }
        public string PaspSummary { get { return string.Format("{0} {1} {2}", PaspSeriya, PaspNumber, PaspIssuing); } }

        public decimal Price { get { return FDocumrnt.Price ?? 0; } }
        public string PriceText { get { return Formater.MoneyToStr((int)Price); } }

        public string EyeStr
        {
            get
            {
                string result = "---";
                if (!FDocumrnt.Eye.HasValue)
                    return result;
                switch (FDocumrnt.Eye.Value)
                {
                    case RVDataModel.Eye.OS: return "левого";
                    case RVDataModel.Eye.OD: return "правого";
                }
                return result;
            }
        }

        public DateTime DateRealization { get { return FDocumrnt.DateRealization ?? new DateTime(); } }
        public string DateRealizationStr { get { return Formater.DateTymeToStr(DateRealization, "dd MMMM yyyyг."); } }



        public string ServiceName
        {
            get
            {
                tblDocumentService xDoc = FDocumrnt as tblDocumentService;
                return xDoc == null ? "" : xDoc.Name;
            }
        }


        public string ProductName
        {
            get
            {
                IDocProduct xDoc = FDocumrnt as IDocProduct;
                Debug.WriteLineIf(xDoc == null, "Попытка получить ProductName у услуги!!!");
                return xDoc == null ? "" : xDoc.ProductName;
            }
        }
        public string ProductModel
        {
            get
            {
                IDocProduct xDoc = FDocumrnt as IDocProduct;
                Debug.WriteLineIf(xDoc == null, "Попытка получить ProductModel у услуги!!!");
                return xDoc == null ? "" : xDoc.ProductModel;
            }
        }

        /// <summary>
        /// Период осмотра например "1 (одного) месяца"
        /// </summary>
        public string MonitorText { get { return (GetBaseOffer().MonitorText ?? "").Trim(); } }

        /// <summary>
        /// Дополнительный текст зависимости стоимости операции
        /// </summary>
        /// <remarks>
        /// Например:
        ///    " и стоимостью интраокулярной линзы"
        ///    " и выражается в диоптриях"
        /// </remarks>
        public string PriceDependenceEx { get { return " " + (GetBaseOffer().PriceDependence ?? "").Trim(); } }

        /// <summary>
        /// Дополнительный анализы
        /// </summary>
        /// <remarks>
        /// Например:
        ///    ", при необходимости и по требованию Клиники"
        ///    ", RW, ЭКГ + консультацию терапевта, ФЛГ"
        /// </remarks>
        public string AnaliseEx
        {
            get
            {
                string xResult = (GetBaseOffer().AnaliseText ?? "").Trim();
                return xResult == "" ? xResult : ", " + xResult;
            }
        }

        /// <summary>
        /// В течение {7 суток} после операции не управлять транспортным средством
        /// </summary>
        public string DontDrive { get { return (GetBaseOffer().DontDriveText ?? "").Trim(); } }

        public string PatientRequired
        {
            get
            {
                string xPatientRequireds = GetBaseOffer().PatientRequired ?? "";
                if (xPatientRequireds == "")
                    return xPatientRequireds;

                string[] xList = xPatientRequireds.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder xResult = new StringBuilder();
                int xFirstRow = 14, n = xFirstRow;
                foreach (var xLine in xList.AsEnumerable())
                {
                    xResult.Append(";\r\n\t");
                    xResult.AppendFormat("{0}) {1}", n++, xLine.Trim());
                }
                return xResult.ToString();
            }
        }
    }

}

