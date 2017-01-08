using System;
using System.Collections.Generic;
using System.Linq;

using Reg2015.RVDataModel;
using CrystalDecisions.CrystalReports.Engine;
using System.Drawing.Printing;

namespace Reg2015.Reports
{
    public class PrintContext
    {

        private static PrintContext _instance;
        public static PrintContext Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PrintContext();
                return _instance;
            }
        }

        private PrinterSettings FCardPrinterSettings;
        protected PrinterSettings CardPrinterSettings
        {
            get
            {
                if (FCardPrinterSettings == null)
                    FCardPrinterSettings = new PrinterSettings();
                return FCardPrinterSettings;
            }
        }

        private PrinterSettings FDefaultDocPrinterSettings;
        /// <summary>2 копии двухстороняя печать</summary>
        protected PrinterSettings DefaultDocPrinterSettings
        {
            get
            {
                if (FDefaultDocPrinterSettings == null)
                {
                    FDefaultDocPrinterSettings = new PrinterSettings();
                    if (FDefaultDocPrinterSettings.CanDuplex)
                        FDefaultDocPrinterSettings.Duplex = Duplex.Vertical;
                    FDefaultDocPrinterSettings.Collate = true;
                    // FDefaultDocPrinterSettings.Copies = 2;
                }
                return FDefaultDocPrinterSettings;
            }
        }

        private PrinterSettings FSalesReceiptPrinterSettings;
        /// <summary>1 копия</summary>
        protected PrinterSettings SalesReceiptPrinterSettings
        {
            get
            {
                if (FSalesReceiptPrinterSettings == null)
                    FSalesReceiptPrinterSettings = new PrinterSettings();
                return FSalesReceiptPrinterSettings;
            }
        }

        public void PrintCards(IEnumerable<tblPatientInfo> patients)
        {
            var gquery = from ds in patients
                         group ds by ds.Kind;// into kindGroup
            // TODO : попробоватьне уничтожать отчет

            foreach (var patItems in gquery)
            {
                ReportClass xReport = new CardDefault();
                var xSource = patItems.Select(ptnt => new CardDataAdapter(ptnt));

                //switch (patItems.Key)
                //{
                //    case CardKind.Default:
                //        xReport = new Card();
                //        break;
                //    case CardKind.ForСataract:
                //        xReport = new CardСataract();
                //        break;
                //}
                //Debug.Assert(xReport != null);
                //if (xReport == null)
                //    continue;
                try
                {
                    xReport.SetDataSource(xSource);
                    xReport.PrintToPrinter(CardPrinterSettings, CardPrinterSettings.DefaultPageSettings, false);
                }
                finally
                {
                    xReport.Dispose();
                }
                //foreach (var patient in patItems)
                //    patient.Printed = true; // будет использоваться для распечатки листа пациентов
            }
        }
        public void PrintCard(tblPatientInfo patient)
        {
            var xCardSource = new List<tblPatientInfo>(1);
            xCardSource.Add(patient);
            PrintCards(xCardSource);
        }

        public ReportClass GetReport(tblDocumentCommon doc, List<DocumentAdapter> reportSource = null)
        {
            List<DocumentAdapter> xSource = reportSource;
            if (xSource == null)
            {
                DocumentAdapter xPrintAdapter = new DocumentAdapter(doc);
                xSource = new List<DocumentAdapter>(1);
                xSource.Add(xPrintAdapter);
            }
            ReportClass xReport;
            if (doc is tblDocumentService)
                xReport = new DocService();
            else if (doc is tblDocumentProduct)
                xReport = new DocSale();
            else if (doc is tblDocumentSalesReceipt)
                xReport = new SaleReceipt();
            else
                throw new Exception("Не определен шаблон документа");
            xReport.SetDataSource(xSource);
            return xReport;
        }

        public void PrintDocuments(IEnumerable<tblDocumentCommon> documents)
        {
            // сортируем по фио и приоритету печати

            // для пациента
            // 2 договора на продажу
            // товарный чек
            // 2 договора на услугу

            var xSortedDocs = (from doc in documents
                               let docOrdPriority = (doc is tblDocumentProduct) ? 0 : ((doc is tblDocumentSalesReceipt) ? 1 : 2)
                               orderby doc.ShortName, docOrdPriority
                               select doc).ToList();
            List<DocumentAdapter> xSource = new List<DocumentAdapter>(1);
            xSource.Add(null);
            foreach (tblDocumentCommon doc in xSortedDocs)
            {
                DocumentAdapter xPrintAdapter = new DocumentAdapter(doc);
                xSource[0] = xPrintAdapter;
                PrinterSettings xPrintsettings = (doc is tblDocumentSalesReceipt) ? SalesReceiptPrinterSettings : DefaultDocPrinterSettings;
                xPrintsettings.Copies = doc.CopiesCount;
                ReportClass xReport = GetReport(doc, xSource);
                try
                {
                    xReport.PrintToPrinter(xPrintsettings, xPrintsettings.DefaultPageSettings, false);
                    doc.Printed = true;
                    doc.IsSelected = false;
                }
                finally
                {
                    xReport.Dispose();
                }
            }
        }

        internal void PrintDocument(tblDocumentCommon tblDocumentCommon)
        {
            var xDodument = (new List<tblDocumentCommon>(1));
            xDodument.Add(tblDocumentCommon);
            PrintDocuments(xDodument);
        }
    }
}

