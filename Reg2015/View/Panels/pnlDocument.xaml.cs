using CrystalDecisions.CrystalReports.Engine;
using Reg2015.Reports;
using Reg2015.RVDataModel;
using Reg2015.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reg2015.View.Panels
{

    /// <summary>
    /// Логика взаимодействия для pnlDocument.xaml
    /// </summary>
    public partial class pnlDocument : UserControl
    {
        public pnlDocument()
        {
            InitializeComponent();
        }

        private ViewDataContext FViewDataContext;

        private void pnlDocuments_Loaded(object sender, RoutedEventArgs e)
        {
            //Не загружайте свои данные во время разработки.
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
            FViewDataContext = ViewDataContext.Instance;
            CollectionViewSource tblProductOfferViewSource = ((CollectionViewSource)(this.FindResource("tblProductOfferViewSource")));
            tblProductOfferViewSource.Source = FViewDataContext.DictProductOffers;
            CollectionViewSource tblOperationOfferViewSource = ((CollectionViewSource)(this.FindResource("tblOperationOfferViewSource")));
            tblOperationOfferViewSource.Source = FViewDataContext.DictOperationOffers;


            //cbbEye.Items = [ Eye.OS, Eye.OS, Eye.OU ]
        }

        private async void dtpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? xSelected = ((DatePicker)sender).SelectedDate;
            if (xSelected == null)
                await FViewDataContext.SetDefaultDocsViewSource();
            else
                await FViewDataContext.SetDocsViewSourceByDate(xSelected.Value);
        }

        private bool CheckBeforeAddDoc()
        {
            tblPatientInfo xPatient = FViewDataContext.CurrentPatient;
            if (xPatient == null)
            {
                MessageBox.Show("Не выбран пациент", "Предупреждение", MessageBoxButton.OK);
                return false;
            }
            if (!dtpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату", "Предупреждение", MessageBoxButton.OK);
                return false;
            }


            return true;
        }


        #region GetDefaultVParams
        private tblPatientInfo GetPatient()
        {
            return FViewDataContext.CurrentPatient;
        }
        private DateTime GetDateRealisation()
        {
            Debug.Assert(dtpDate.SelectedDate.HasValue);
            return dtpDate.SelectedDate.Value;
        }
        private Eye GetEye()
        {
            switch (cbbEye.SelectedIndex)
            {
                case 0: return Eye.OS;
                case 1: return Eye.OD;
                case 2: return Eye.OU;
                default:
                    Debug.Fail("");
                    return Eye.No;
            }
        }
        private tblOffer GetOfferService()
        {
            return (tblOffer)cbbOperationType.SelectedItem;
        }
        private tblOffer GetOfferProduct()
        {
            return (tblOffer)cbbProductName.SelectedItem;
        }
        private decimal GetPrice(string value)
        {
            decimal result;
            if (!decimal.TryParse(value.Replace(".", ","), out result))
                return 0;
            return result;
        }

        #endregion

        private ManipulationConfig GetManipulationConfig(bool isService)
        {
            return new ManipulationConfig()
            {
                Patient = GetPatient(),
                Offer = isService ? GetOfferService() : GetOfferProduct(),
                Eye = GetEye(),
                DateRealisation = GetDateRealisation(),
                Price = isService ? GetPrice(edtServicePrice.Text) : GetPrice(edtProductPrice.Text),
            };
        }


        //private void AddService(tblManipulation manipulation)
        //{
        //    tblDocumentService xDoc = new tblDocumentService();
        //    ViewDataContext.PrepareDoc(xDoc, manipulation);
        //    FViewDataContext.Documents.Add(xDoc);
        //}

        //private void AddProuctDoc(tblManipulation manipulation)
        //{
        //    tblDocumentProduct xDoc = new tblDocumentProduct();
        //    ViewDataContext.PrepareDoc(xDoc, manipulation);
        //    FViewDataContext.Documents.Add(xDoc);
        //}

        //private void AddProuctSalesReceipt(tblManipulation manipulation)
        //{
        //    tblDocumentSalesReceipt xDoc = new tblDocumentSalesReceipt();
        //    ViewDataContext.PrepareDoc(xDoc, manipulation);
        //    FViewDataContext.Documents.Add(xDoc);
        //}


        private void btnAddServiceClick(object sender, RoutedEventArgs e)
        {
            if (!CheckBeforeAddDoc())
                return;
            ViewDataContext.AddDoc(GetManipulationConfig(true), () => new tblDocumentCommon[] { new tblDocumentService() });
        }

        private void btnAddProtuctClick(object sender, RoutedEventArgs e)
        {
            if (!CheckBeforeAddDoc())
                return;
            ViewDataContext.AddDoc(GetManipulationConfig(false), () => new tblDocumentCommon[] { new tblDocumentProduct(), new tblDocumentSalesReceipt() });
        }


        private void btnAddServProdClick(object sender, RoutedEventArgs e)
        {
            if (!CheckBeforeAddDoc())
                return;

            btnAddServiceClick(sender, e);
            btnAddProtuctClick(sender, e);

        }

        private async void actgrdPrint(object sender, RoutedEventArgs e)
        {
            tblDocumentCommon xDoc = FViewDataContext.CurrentDocument; // (tblDocumentCommon)((Button)sender).Tag;
            await FViewDataContext.SaveAllCangesAsync();
            PrintContext.Instance.PrintDocument(FViewDataContext.CurrentDocument);
            await FViewDataContext.SaveAllCangesAsync();
        }

        private async void actgrdPreview(object sender, RoutedEventArgs e)
        {

            tblDocumentCommon xDoc = FViewDataContext.CurrentDocument; //(tblDocumentCommon)((Button)sender).Tag;
            await FViewDataContext.SaveAllCangesAsync();
            ReportPreview xReportPreview = new ReportPreview();
            //xReportPreview.Owner = this;
            ReportClass xReport = PrintContext.Instance.GetReport(xDoc);
            try
            {
                xReportPreview.SetReport(xReport);
                xReportPreview.Show();
            }
            finally
            {
                // xReport.Dispose();
            }
        }

        private async void actgrdPrintSelected(object sender, RoutedEventArgs e)
        {
            await FViewDataContext.SaveAllCangesAsync();
            PrintContext.Instance.PrintDocuments(FViewDataContext.SelectesDocument);
            await FViewDataContext.SaveAllCangesAsync();
        }
    }



    //internal class DocumentConfig
    //{
    //    public tblManipulation Manipulation;
    //    public tblPatient Patient;
    //    public bool IsSelected;
    //}
}



