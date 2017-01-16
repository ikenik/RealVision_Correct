using Reg2015.Lib;
using Reg2015.RVDataModel;
using Reg2015.ViewModel;
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

namespace Reg2015.View.Panels
{

    public delegate void OnDateTimeLost(string f, string i, string o, DateTime birthDate);

    /// <summary>
    /// Interaction logic for pnlPatientInfo.xaml
    /// </summary>
    public partial class pnlPatientInfo : UserControl
    {
        public pnlPatientInfo()
        {
            InitializeComponent();
        }

        private string _RadioButtonSexGroup;
        public string RadioButtonSexGroup
        {
            get
            {
                return String.IsNullOrEmpty(_RadioButtonSexGroup) ? _RadioButtonSexGroup = Guid.NewGuid().ToString("N") : _RadioButtonSexGroup;
            }
        }


        public event RequareDocumentsByPatient RequareDocumentsEvent;

        public Visibility JobVisibility
        {
            get { return (Visibility)GetValue(JobVisibilityProperty); }
            set { SetValue(JobVisibilityProperty, value); }
        }
        // Using a DependencyProperty as the backing store for JobVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JobVisibilityProperty =
            DependencyProperty.Register("JobVisibility", typeof(Visibility), typeof(pnlPatientInfo), new PropertyMetadata(Visibility.Collapsed));

        public Visibility RarVisibility
        {
            get { return (Visibility)GetValue(RarVisibilityProperty); }
            set { SetValue(RarVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RarVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RarVisibilityProperty =
            DependencyProperty.Register("RarVisibility", typeof(Visibility), typeof(pnlPatientInfo), new PropertyMetadata(Visibility.Visible));

        public Visibility SummaryVisibility
        {
            get { return (Visibility)GetValue(SummaryVisibilityProperty); }
            set { SetValue(SummaryVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SummaryVisibilityProperty =
            DependencyProperty.Register("SummaryVisibility", typeof(Visibility), typeof(pnlPatientInfo), new PropertyMetadata(Visibility.Visible));


        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            //for (int i = 1, xLen = a.Length; i < xLen; i++)
            //    a[i] = char.ToLower(a[i]);
            return new string(a);
        }

        private void FIOTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Text = UppercaseFirst(((TextBox)sender).Text);
        }

        private TraversalRequest FTraversalRequest = new TraversalRequest(FocusNavigationDirection.Next);
        private void EditorBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox).MoveFocus(FTraversalRequest);
            }
        }

        private void btnRequareDocumentsClick(object sender, RoutedEventArgs e)
        {
            if (RequareDocumentsEvent != null)
                RequareDocumentsEvent((tblPatient)((Button)sender).Tag);
        }

        private DataHelper FDataHelper = new DataHelper();
        private ViewDataContext FViewDataContext = ViewDataContext.Instance;

        public event OnDateTimeLost DateTimeLostEvent;

        private void birthDayDatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DateTimeLostEvent == null)
                return;
            tblPatient xPatient = (tblPatientInfo)DataContext;
            if ((xPatient == null) || (!xPatient.BirthDay.HasValue)/* || Guid.Empty.Equals(xPatient.ID)*/)
                return;

            DateTimeLostEvent(xPatient.FirstName, xPatient.LastName, xPatient.FatherName, xPatient.BirthDay.Value);

            //this.DataContext;
            // FDataHelper.FindFIOData()
        }

    }
}
