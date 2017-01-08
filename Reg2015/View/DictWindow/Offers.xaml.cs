using Reg2015.ViewModel;
using System.Windows;
using System.Windows.Data;

namespace Reg2015.View.DictWindow
{
    /// <summary>
    /// Interaction logic for Offers.xaml
    /// </summary>
    public partial class Offers : Window
    {
        public Offers()
        {
            InitializeComponent();
        }


        private ViewDataContext FViewDataContext;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            FViewDataContext = ViewDataContext.Instance;
            CollectionViewSource tblProductOfferVS = ((CollectionViewSource)(this.FindResource("tblProductOfferVS")));
            tblProductOfferVS.Source = FViewDataContext.DictProductOffers;

            CollectionViewSource tblOperationOfferVS = ((CollectionViewSource)(this.FindResource("tblOperationOfferVS")));
            tblOperationOfferVS.Source = FViewDataContext.DictOperationOffers;


            CollectionViewSource MonitorTextSource = ((CollectionViewSource)(this.FindResource("MonitorTextSource")));
            CollectionViewSource PriceDependenceSource = ((CollectionViewSource)(this.FindResource("PriceDependenceSource")));
            CollectionViewSource AnaliseTextSource = ((CollectionViewSource)(this.FindResource("AnaliseTextSource")));
            CollectionViewSource DontDriveTextSource = ((CollectionViewSource)(this.FindResource("DontDriveTextSource")));
            CollectionViewSource PatientRequiredSource = ((CollectionViewSource)(this.FindResource("PatientRequiredSource")));

            FViewDataContext.FlashTexts();

            MonitorTextSource.Source =  FViewDataContext.MonitorTextList;
            PriceDependenceSource.Source =  FViewDataContext.PriceDependenceList;
            AnaliseTextSource.Source =  FViewDataContext.AnaliseTextList;
            DontDriveTextSource.Source = FViewDataContext.DontDriveTextList;
            PatientRequiredSource.Source = FViewDataContext.PatientRequiredTextList;
        }
    }
}
