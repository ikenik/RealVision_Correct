using Reg2015.RVDataModel;
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

using Reg2015.ViewModel;
using Reg2015.Lib;

namespace Reg2015.View.Panels
{
   
    /// <summary>
    /// Логика взаимодействия для pnlManipulationsObs.xaml
    /// </summary>
    public partial class pnlManipulationsObs : UserControl
    {
        public pnlManipulationsObs()
        {
            InitializeComponent();
        }

        public event RequareDocumentsByManipulation RequareDocuments;

        private ViewDataContext FViewDataContext;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Не загружайте свои данные во время разработки.
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
            FViewDataContext = ViewDataContext.Instance;

            CollectionViewSource tblOfferViewSource = ((CollectionViewSource)(this.FindResource("tblOfferViewSource")));
            tblOfferViewSource.Source = FViewDataContext.DictDefaultOffers;

            CollectionViewSource tblEmployeeViewSource = ((CollectionViewSource)(this.FindResource("tblEmployeeViewSource")));
            tblEmployeeViewSource.Source = FViewDataContext.DictEmployees;

            //System.Windows.Data.CollectionViewSource tblReferralVendorViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("tblReferralVendorViewSource")));
            //tblReferralVendorViewSource.Source = FViewDataContext. ;

        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var xSender = (ComboBox)sender;

            switch (((tblManipulation)xSender.Tag).tblOffer.Classification)
            {
                case OfferClassification.Monitor:
                case OfferClassification.PrimaryReception:
                case OfferClassification.PaidReception:
                    xSender.Items.Filter = null;
                    break;
                default:
                    xSender.Items.Filter = item => ((ComboBoxItem)item).Content.ToString() != "OU";
                    break;
            }
        }

        private void btnShowDocumentsClick(object sender, RoutedEventArgs e)
        {
            if (RequareDocuments != null)
                RequareDocuments((tblManipulation)((Button)sender).Tag);
        }
    }
}
