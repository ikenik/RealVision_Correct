using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Reg2015.ViewModel;
using System.Threading.Tasks;

// TODO : Проверять возмлжно пациент уже есть при добавлении карточки

namespace Reg2015.View.Panels
{
    /// <summary>
    /// Логика взаимодействия для pnlCardNavigation.xaml
    /// </summary>
    public partial class pnlCardNavigation : UserControl
    {

        public pnlCardNavigation()
        {
            InitializeComponent();
        }

        private ViewDataContext FViewDataContext;

        private void CardNavigation_Loaded(object sender, RoutedEventArgs e)
        {
            //Не загружайте свои данные во время разработки.
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
            FViewDataContext = ViewDataContext.Instance;
        }

        private void ClearFIOFiltr()
        {
            fTextBox.Text = "";
            iTextBox.Text = "";
            oTextBox.Text = "";
            dateBirthDayDateePicker.SelectedDate = null;
        }


        private bool FLockFiltrChange = false;

        private async void btnFlashFiltrClick(object sender, RoutedEventArgs e)
        {
            FLockFiltrChange = true;
            try
            {
                ClearFIOFiltr();
            }
            finally
            {
                FLockFiltrChange = false;
            }
            // dateBirthDayDateePicker.Text = "";
            await FViewDataContext.SetDefaultCardsViewSource();
        }

        private async Task ApplyFiltr()
        {
            //if (!(fTextBox.IsFocused || iTextBox.IsFocused || oTextBox.IsFocused || dateBirthDayDateePicker.IsFocused))
            //    return;
            await FViewDataContext.SetCardsByFIOViewSource(fTextBox.Text, iTextBox.Text, oTextBox.Text, dateBirthDayDateePicker.SelectedDate);
        }

        private async void fioTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FLockFiltrChange) return;
            await ApplyFiltr();
        }



        //private async void fioTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (!fioTextBox.IsFocused)
        //        return;
        //    string xText = fioTextBox.Text;
        //    if (xText == "")
        //    {
        //        await FViewDataContext.SetDefaultCardsViewSource();
        //        return;
        //    }

        //    string[] xSep = { " " };
        //    string[] fio = xText.Split(xSep, StringSplitOptions.RemoveEmptyEntries);
        //    string xFrst = "";
        //    string xLast = "";
        //    string xFather = "";
        //    for (int i = 0; i < fio.Length; i++)
        //    {
        //        string item = fio[i];
        //        if (item == "*")
        //            continue;
        //        switch (i)
        //        {
        //            case 0: xFrst = item; break;
        //            case 1: xLast = item; break;
        //            case 2: xFather = item; break;
        //            default:
        //                break;
        //        }
        //        if (i == 2)
        //            break;
        //    }

        //    await FViewDataContext.SetCardsByFIOViewSource(xFrst, xLast, xFather);
        //}

        private async void numberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!numberTextBox.IsFocused)
            {
                return;
            }
            int xNumber;
            if ((!int.TryParse(numberTextBox.Text, out xNumber)) && (xNumber <= 0))
                return;

            await FViewDataContext.SetCardsByNumberViewSource(xNumber);
        }

        private void numberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // отвязываемся от привязки
            BindingOperations.ClearBinding(numberTextBox, TextBox.TextProperty);
        }

        private void numberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // привязываемся к свойству
            Binding bind = new Binding();
            bind.Path = new PropertyPath("Number");
            bind.Mode = BindingMode.OneWay;
            numberTextBox.SetBinding(TextBox.TextProperty, bind);
        }

        private async void dateBirthDayDateePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FLockFiltrChange) return;
            await ApplyFiltr();

            ////if (!dateBirthDayDateePicker.IsFocused)
            ////    return;
            //ClearFIOFiltr();

            //DateTime? xDate = dateBirthDayDateePicker.SelectedDate;
            //if (!xDate.HasValue)
            //    await FViewDataContext.SetDefaultCardsViewSource();

            //DateTime xLow = xDate.Value.Date;
            //DateTime xUpp = xLow.AddDays(1);

            //await FViewDataContext.SetCardsByDateBetweenViewSource(xLow, xUpp);
        }
    }
}
