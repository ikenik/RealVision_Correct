using System;
using System.Collections.Generic;
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

using Reg2015.RVDataModel;
using Reg2015.ViewModel;

using Xceed.Wpf.Toolkit;
using NK.Collections.ObjectViewModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Reg2015.Reports;
using Reg2015.View.DictWindow;


namespace Reg2015
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ViewDataContext FViewDataContext;
        private DataHelper FDataHelper;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var RVDataContext = new RealVisionDataContext();
            FDataHelper = new DataHelper();
            //var xPtntQuery = RVDataContext.tblPatients.Include("tblDocumentCommons").Where(ptnt => ptnt is tblPatientInfo).Take(10);
            //var xDocQuery = RVDataContext.tblDocumentCommons.Where(doc => ((doc.DateDelete == null) && (!doc.Printed))).OrderByDescending(doc => doc.DateCreate);
            //xPtntQuery.ToList();
            //xDocQuery.ToList();


            // TODO : загрузить последние карточки, в фоновом потоке
            FViewDataContext = ViewDataContext.Instance;

            //await FViewDataContext.SetDefaultDocsViewSource();
            await FViewDataContext.SetDefaultCardsViewSource();

            CollectionViewSource tblDefaultOfferViewSource = ((CollectionViewSource)(this.FindResource("tblDefaultOfferViewSource")));
            tblDefaultOfferViewSource.Source = FViewDataContext.DictDefaultOffers;

            CollectionViewSource tblEmployeeViewSource = ((CollectionViewSource)(FindResource("tblEmployeeViewSource")));
            tblEmployeeViewSource.Source = FViewDataContext.DictEmployees;

            pnlMainPanient.DateTimeLostEvent += pnlMainPanient_DateTimeLostEvent;
        }

        private async void pnlMainPanient_DateTimeLostEvent(string f, string i, string o, DateTime birthDate)
        {
            tblPatientInfo xCurrent = FViewDataContext.CurrentPatient;
            if (!Guid.Empty.Equals(xCurrent.ID))
                return;


            List<tblPatientInfo> xPatients = await FDataHelper.FindPatientIDByFIOData(f, i, o, birthDate);
            if (xPatients.Count == 0)
                return;

            StringBuilder xMsg = new StringBuilder();
            xMsg.AppendFormat("Такие данные уже есть.     {0} {1} {2}, дата рождения {3: dd MMMM yyyy}", f, i, o, birthDate); xMsg.AppendLine();
            xMsg.AppendLine();
            xMsg.Append(xPatients.Count > 1 ? "№ карточек:\r\n" : "№ карточки: ");
            xMsg.Append(string.Join(";\r\n", xPatients.Select(ptnt => string.Format("{0}, зарегистрирована {1: dd MMMM yyyy}", ptnt.Number, ptnt.DateCreate)).ToArray()));
            xMsg.AppendLine(".");
            xMsg.AppendLine();
            xMsg.AppendLine("Да  - Все равно ввести новые данные");
            xMsg.AppendLine("Нет - Отменить ввод и просмотреть существующие данные");

            MessageBoxResult xRes = System.Windows.MessageBox.Show(xMsg.ToString(), "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Stop);
            if (xRes == MessageBoxResult.Yes)
                return;

            xCurrent.ForceRemove = true;
            FViewDataContext.PatientInfos.Remove(xCurrent);
            await SaveAllChanges();
            await FViewDataContext.SetCardsByFIOViewSource(f, i, o, birthDate);
        }

        protected async Task SaveAllChanges()
        {
            await FViewDataContext.SaveAllCangesAsync();
        }

        private void AddManipulation(tblOffer offer, tblEmployee employee = null)
        {

            tblPatientInfo xPatientInfo = FViewDataContext.CurrentPatient;
            if (xPatientInfo == null)
            {
                System.Windows.MessageBox.Show("Не выбран пациент");
                return;
            }

            tblManipulation xManipulation = new tblManipulation()
            {
                tblPatient = xPatientInfo,
                tblOffer = offer,
                Price = offer.DefaultPrice,
                DateRealization = DateTime.Now,
                Eye = Eye.OU,
                tblEmployee = employee
            };

            xPatientInfo.tblManipulationsObs.Add(xManipulation);
        }

        #region Манипуляции
        private void MenuItem_AddManipolation(object sender, RoutedEventArgs e)
        {
            MenuItem xMenuItem = e.OriginalSource as MenuItem;
            if ((xMenuItem == null) || (xMenuItem.Tag == null))
            {
                return;
            }
            AddManipulation(xMenuItem.Tag as tblOffer);
        }

        private void btnAddManipulation_Click(object sender, RoutedEventArgs e)
        {
            tblOffer xOffer = (tblOffer)((Button)sender).Tag;
            AddManipulation(xOffer);
        }
        #endregion

        #region Печать карточек
        private async Task PrintaCard(tblPatientInfo currentPatient)
        {
            await FViewDataContext.SaveAllCangesAsync(); // получить все номер аи ID

            // печатаем
            currentPatient.Printed = true;
            await FViewDataContext.SaveAllCangesAsync();
        }

        private async void btnPrintDefaultCard(object sender, RoutedEventArgs e)
        {
            tblPatientInfo xCurrentPatient = FViewDataContext.CurrentPatient;
            await PrintaCard(xCurrentPatient);
        }

        #endregion

        #region Комманды
        private async void cmdSaveAll(object sender, ExecutedRoutedEventArgs e)
        {
            await FViewDataContext.SaveAllCangesAsync();
        }

        private void cmdAddPatientAndCadr(object sender, ExecutedRoutedEventArgs e)
        {
            tblPatientInfo xNewPatient = new tblPatientInfo() { Location = CardLocation.Receptopn };

            ObservableCollection<tblPatientInfo> xPatientsSource = FViewDataContext.PatientInfos;
            xPatientsSource.Add(xNewPatient);
            FViewDataContext.PatientInfoViewSource.View.MoveCurrentTo(xNewPatient);
        }

        private void cmdDeleteCurrentPatient(object sender, ExecutedRoutedEventArgs e)
        {
            tblPatientInfo xCurrent = FViewDataContext.CurrentPatient;
            if (xCurrent == null)
                return;

            MessageBoxResult xRes = System.Windows.MessageBox.Show(string.Format("Удалить карточку № {0}", xCurrent.Number), "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.No);
            if (xRes != MessageBoxResult.Yes)
                return;

            FViewDataContext.PatientInfos.Remove(xCurrent);
        }
        #endregion

        #region Договора
        // Сделать виртуальные договора сохранять только те которые нужно напечатать

        #endregion


        private void btnShowContextMenu(object sender, RoutedEventArgs e)
        {
            Button xButon = (sender as Button);
            if (xButon == null)
                return;
            xButon.ContextMenu.IsOpen = true;
        }


        private tblOffer FDefMonitorReception;
        private tblOffer FDefPrimaryReception;
        private tblOffer FDefPaidReception;
        private bool FDefOfferPrepared = false;
        private void PrepareDefaultOffers()
        {

            foreach (var xOffer in FViewDataContext.DictOffers)
            {
                switch (xOffer.Classification)
                {
                    case OfferClassification.Monitor: FDefMonitorReception = xOffer; break;
                    case OfferClassification.PrimaryReception: FDefPrimaryReception = xOffer; break;
                    case OfferClassification.PaidReception: FDefPaidReception = xOffer; break;
                }
            }
            FDefOfferPrepared = true;
        }

        protected tblOffer DefMonitorReception
        {
            get
            {
                if (!FDefOfferPrepared) PrepareDefaultOffers();
                return FDefMonitorReception;
            }
        }
        protected tblOffer DefPrimaryReception
        {
            get
            {
                if (!FDefOfferPrepared) PrepareDefaultOffers();
                return FDefPrimaryReception;
            }
        }
        protected tblOffer DefPaidReception
        {
            get
            {
                if (!FDefOfferPrepared) PrepareDefaultOffers();
                return FDefPaidReception;
            }
        }

        private void menuItenAddPrimaryRecept(object sender, RoutedEventArgs e)
        {
            MenuItem xMenuItem = e.OriginalSource as MenuItem;
            if ((xMenuItem == null) || (xMenuItem.Tag == null))
                return;
            AddManipulation(DefPrimaryReception, xMenuItem.Tag as tblEmployee);
        }

        private void menuItenAddMonitorReception(object sender, RoutedEventArgs e)
        {
            MenuItem xMenuItem = e.OriginalSource as MenuItem;
            if ((xMenuItem == null) || (xMenuItem.Tag == null))
                return;
            AddManipulation(DefMonitorReception, xMenuItem.Tag as tblEmployee);
        }
        private void menuItenAddPaidReception(object sender, RoutedEventArgs e)
        {
            MenuItem xMenuItem = e.OriginalSource as MenuItem;
            if ((xMenuItem == null) || (xMenuItem.Tag == null))
                return;
            AddManipulation(DefPaidReception, xMenuItem.Tag as tblEmployee);
        }

        private async void btnPrintCardClick(object sender, RoutedEventArgs e)
        {
            await SaveAllChanges();
            PrintContext.Instance.PrintCard(FViewDataContext.CurrentPatient);
        }

        private async void actPrintSelectedDocuments(object sender, RoutedEventArgs e)
        {
            await SaveAllChanges();
            PrintContext.Instance.PrintDocuments(FViewDataContext.SelectesDocument);
            await SaveAllChanges();
        }

        private async void actPrintCurrentDocuments(object sender, RoutedEventArgs e)
        {
            await SaveAllChanges();
            PrintContext.Instance.PrintDocument(FViewDataContext.CurrentDocument);
            await SaveAllChanges();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FViewDataContext.SaveAllCangesSync();
        }

        private void btnOfferChange(object sender, RoutedEventArgs e)
        {
            Offers xOffers = new Offers();
            xOffers.Owner = this;
            xOffers.Show();
        }

        private async void pnlManipulations_RequareDocuments(tblManipulation manipulation)
        {
            tabDocuments.IsSelected = true;
            await FViewDataContext.SaveAllCangesAsync();
            await FViewDataContext.SetDocsByManipulation(manipulation);
        }

        private async void pnlRequareDocumentsByPatient(tblPatient patient)
        {
            tabDocuments.IsSelected = true;
            await FViewDataContext.SaveAllCangesAsync();
            await FViewDataContext.SetDocsByPatient(patient);
        }

    }
}
