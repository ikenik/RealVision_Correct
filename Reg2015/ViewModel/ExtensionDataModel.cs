using System;
using System.Linq;

using NK.Collections.ObjectViewModel;
using Reg2015.ViewModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Reg2015.RVDataModel
{
    public partial class RealVisionDataContext
    {
        //public int GetDocumentServiceMask()
        //{
        //    return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("GetDocumentServiceMask");
        //}

    }


    internal static class ModelFunctions
    {
        internal static string GetShortName(IPatientFIO fio)
        {
            string result = fio.FirstName;
            string xInit = "";
            if ((fio.LastName != null) && (fio.LastName.Length > 0))
                xInit += fio.LastName[0] + ".";
            if ((fio.FatherName != null) && (fio.FatherName.Length > 0))
                xInit += fio.FatherName[0] + ".";
            if (xInit.Length > 0)
                result += " " + xInit;
            return result;
        }
    }

    public partial class tblManipulation : IIndestructibleObject, IDocOwner
    {
        #region IIndestructibleObject
        public bool ForceRemove { get; set; }
        #endregion

        // Если tblDocumentCommons абстрактный то БД уходят страшные зспросы
        public int DocumentsCount
        {
            get
            {
                int xCount;
                if (FDocumentCommonsObs == null)
                {
                    RealVisionDataContext xDataContext = ViewDataContext.Instance.RVDataContext;
                    xCount = (from doc in xDataContext.tblDocumentCommons.AsNoTracking()
                              where (doc.ManipulationID == ID) && (doc.DateDelete == null)
                              select doc).Count();
                }
                else
                {
                    xCount = tblDocumentCommonsObs.Count;
                }
                return xCount;
            }
        }

        private ObsCollectionWrap<tblDocumentCommon, tblDocumentCommon> FDocumentCommonsObs;
        public ObservableCollection<tblDocumentCommon> tblDocumentCommonsObs
        {
            get
            {
                if (FDocumentCommonsObs == null)
                {
                    RealVisionDataContext xDataContext = ViewDataContext.Instance.RVDataContext;
                    FDocumentCommonsObs = ViewDataContext.PopulateCollection(xDataContext.tblDocumentCommons.Where(doc => (doc.ManipulationID == ID) && (doc.DateDelete == null)), xDataContext.tblDocumentCommons);
                }
                return FDocumentCommonsObs;
            }
        }



        #region Синхронизация всех договоров связанных пациентом
        partial void OnOfferCustomTextChanged()
        {
            var xOfferCustomText = OfferCustomText;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc =>
            {
                if (doc is tblDocumentService)
                    ((tblDocumentService)doc).Name = xOfferCustomText;
                else if (doc is tblDocumentProduct)
                    ((tblDocumentProduct)doc).ProductName = xOfferCustomText;
                else if (doc is tblDocumentSalesReceipt)
                    ((tblDocumentSalesReceipt)doc).ProductName = xOfferCustomText;
            });
        }
        partial void OnEyeChanged()
        {
            var xEye = Eye;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.Eye = xEye);
        }
        partial void OnPriceChanged()
        {
            var xPrice = Price;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.Price = xPrice);
        }
        partial void OnDateRealizationChanged()
        {
            var xDateRealization = DateRealization;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.DateRealization = xDateRealization);
        }
        #endregion
    }
    public partial class tblPatient : IIndestructibleObject, IDocOwner
    {
        #region IIndestructibleObject
        public bool ForceRemove { get; set; }
        #endregion


        #region Синхронизация всех договоров связанных пациентом
        partial void OnFirstNameChanged()
        {
            OnPropertyChanged("ShortName");
            var xFirstName = FirstName;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.FirstName = xFirstName);
        }
        partial void OnLastNameChanged()
        {
            OnPropertyChanged("ShortName");
            var xLastName = LastName;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.LastName = xLastName);
        }
        partial void OnFatherNameChanged()
        {
            OnPropertyChanged("ShortName");
            var xFatherName = FatherName;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.FatherName = xFatherName);
        }
        partial void OnBirthDayChanged()
        {
            var xBirthDay = BirthDay;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.BirthDay = xBirthDay);
        }
        #endregion

        public string ShortName
        {
            get { return ModelFunctions.GetShortName(this as IPatientFIO); }
        }

        private ObsCollectionWrap<tblDocumentCommon, tblDocumentCommon> FDocumentCommonsObs;
        public ObservableCollection<tblDocumentCommon> tblDocumentCommonsObs
        {
            get
            {
                if (FDocumentCommonsObs == null)
                {
                    RealVisionDataContext xDataContext = ViewDataContext.Instance.RVDataContext;
                    FDocumentCommonsObs = ViewDataContext.PopulateCollection(xDataContext.tblDocumentCommons.Where(doc => (doc.PatientID == ID) && (doc.DateDelete == null)), xDataContext.tblDocumentCommons);
                }
                return FDocumentCommonsObs;
            }
        }

        private ObsCollectionWrap<tblManipulation, tblManipulation> FManipulationsObs;
        public ObservableCollection<tblManipulation> tblManipulationsObs
        {
            get
            {
                if (FManipulationsObs == null)
                {
                    RealVisionDataContext xDataContext = ViewDataContext.Instance.RVDataContext;
                    var query = from manip in xDataContext.tblManipulations
                                where (manip.PatientID == this.ID) && (manip.DateDelete == null)
                                select manip;
                    FManipulationsObs = ViewDataContext.PopulateCollection(query, xDataContext.tblManipulations);
                    FManipulationsObs.CollectionChanged += FManipulationsObs_CollectionChanged;
                }
                return FManipulationsObs;
            }
        }

        private void FManipulationsObs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Удалить все не распечатанные договора
                // var xDocuments = ((ObservableCollection<tblManipulation>)sender).tb;
                foreach (tblManipulation OldItem in e.OldItems)
                {
                    var xDocuments = OldItem.tblDocumentCommonsObs;
                    var xOldDocuments = xDocuments.Where(doc => !doc.Printed).ToList();
                    foreach (tblDocumentCommon item in xOldDocuments)
                        xDocuments.Remove(item);
                }
            }
        }
    }


    public interface IPatientFIO
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string FatherName { get; set; }
    }

    /// <summary>
    /// Общий интерфейс для сущности договор и информации о пациенте
    /// </summary>
    public interface ICommonPatientInfo : IPatientFIO
    {
        DateTime? BirthDay { get; set; }
        string Address { get; set; }
        string PaspSeriya { get; set; }
        string PaspNumber { get; set; }
        string PaspIssuing { get; set; }
        string Phon { get; set; }
    }

    public partial class tblPatientInfo : ICommonPatientInfo
    {
        #region Синхронизация всех договоров пациентом
        partial void OnAddressChanged()
        {
            var xAddress = Address;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.Address = xAddress);
        }
        partial void OnPaspSeriyaChanged()
        {
            var xPaspSeriya = PaspSeriya;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.PaspSeriya = xPaspSeriya);
        }
        partial void OnPaspNumberChanged()
        {
            var xPaspNumber = PaspNumber;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.PaspNumber = xPaspNumber);
        }
        partial void OnPaspIssuingChanged()
        {
            var xPaspIssuing = PaspIssuing;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.PaspIssuing = xPaspIssuing);
        }
        partial void OnPhonChanged()
        {
            var xPhon = Phon;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(this, doc => doc.Phon = xPhon);
        }

        #endregion
    }

    ///// <summary>
    ///// Скопировал из сгенерированного файла
    ///// </summary>
    ///// <remarks>
    ///// Если договор распечатан, то берем данные из договора, иначе если нет данных договора берем данные пациентьа
    ///// </remarks>
    //internal class DocPatientInfo : INotifyPropertyChanged, ICommonPatientInfo
    //{
    //    #region Implement INotifyPropertyChanged
    //    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    //    protected virtual void OnPropertyChanged(string propertyName)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    //        }
    //    }
    //    #endregion

    //    public DocPatientInfo(tblDocumentCommon owner)
    //    {
    //        FDocOwner = owner;
    //        FPatientInfo = (tblPatientInfo)FDocOwner.tblPatient; // Debug.Assert(FPatientInfo != null); TODO : срабатывает assert

    //        FDocOwner.PropertyChanged += Owner_PropertyChanged;
    //        FPatientInfo.PropertyChanged += Owner_PropertyChanged;
    //    }

    //    /// <summary>
    //    /// Переадресация изменения свойств
    //    /// </summary>
    //    private void Owner_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //    {
    //        OnPropertyChanged(e.PropertyName);
    //    }

    //    internal void NotifyPatientOwnerChange()
    //    {
    //        OnPropertyChanged("FirstName");
    //        OnPropertyChanged("LastName");
    //        OnPropertyChanged("FatherName");
    //        OnPropertyChanged("BirthDay");
    //        OnPropertyChanged("Address");
    //        OnPropertyChanged("PaspSeriya");
    //        OnPropertyChanged("PaspNumber");
    //        OnPropertyChanged("PaspIssuing");
    //        OnPropertyChanged("Phon");
    //    }

    //    private tblDocumentCommon FDocOwner;
    //    private tblPatientInfo FPatientInfo;

    //    public string FirstName
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.FirstName : FDocOwner.FirstName ?? FPatientInfo.FirstName; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.FirstName = value; }
    //    }

    //    public string LastName
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.LastName : FDocOwner.LastName ?? FPatientInfo.LastName; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.LastName = value; }
    //    }

    //    public string FatherName
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.FatherName : FDocOwner.FatherName ?? FPatientInfo.FatherName; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.FatherName = value; }
    //    }

    //    public DateTime? BirthDay
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.BirthDay : FDocOwner.BirthDay ?? FPatientInfo.BirthDay; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.BirthDay = value; }
    //    }

    //    public string Address
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.Address : FDocOwner.Address ?? FPatientInfo.Address; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.Address = value; }
    //    }

    //    public string PaspSeriya
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.PaspSeriya : FDocOwner.PaspSeriya ?? FPatientInfo.PaspSeriya; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.PaspSeriya = value; }
    //    }

    //    public string PaspNumber
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.PaspNumber : FDocOwner.PaspNumber ?? FPatientInfo.PaspNumber; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.PaspNumber = value; }
    //    }

    //    public string PaspIssuing
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.PaspIssuing : FDocOwner.PaspIssuing ?? FPatientInfo.PaspIssuing; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.PaspIssuing = value; }
    //    }

    //    public string Phon
    //    {
    //        get { return FDocOwner.Printed ? FDocOwner.Phon : FDocOwner.Phon ?? FPatientInfo.Phon; }
    //        set { Debug.Assert(!FDocOwner.Printed); FDocOwner.Phon = value; }
    //    }
    //}


    public partial class tblDocumentCommon : IIndestructibleObject, ICommonPatientInfo, IDocumentCommon
    {
        #region IIndestructibleObject
        public bool ForceRemove { get; set; }
        #endregion

        #region Синхронизация всех договоров связанных однй манипуляцией или пациентом

        partial void OnEyeChanged()
        {
            if (tblManipulation == null)
                return;
            var xEye = Eye;
            tblManipulation.Eye = xEye.HasValue ? xEye.Value : RVDataModel.Eye.No;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblManipulation, doc => doc.Eye = xEye);
        }
        partial void OnDateRealizationChanged()
        {
            if (tblManipulation == null)
                return;
            var xDateRealization = DateRealization;
            if (xDateRealization.HasValue)
                tblManipulation.DateRealization = xDateRealization.Value;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblManipulation, doc => doc.DateRealization = xDateRealization);
        }
        partial void OnPriceChanged()
        {
            if (tblManipulation == null)
                return;
            var xPrice = Price;
            if (xPrice.HasValue)
                tblManipulation.Price = xPrice.Value;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblManipulation, doc => doc.Price = xPrice);
        }
        partial void OnFirstNameChanged()
        {
            OnPropertyChanged("ShortName");
            if (tblPatient == null)
                return;
            var xFirstName = FirstName;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.FirstName = xFirstName);

        }
        partial void OnLastNameChanged()
        {
            OnPropertyChanged("ShortName");
            if (tblPatient == null)
                return;
            var xLastName = LastName;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.LastName = xLastName);

        }
        partial void OnFatherNameChanged()
        {
            OnPropertyChanged("ShortName");
            if (tblPatient == null)
                return;
            var xFatherName = FatherName;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.FatherName = xFatherName);

        }
        partial void OnBirthDayChanged()
        {
            if (tblPatient == null)
                return;
            var xBirthDay = BirthDay;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.BirthDay = xBirthDay);
        }
        partial void OnAddressChanged()
        {
            if (tblPatient == null)
                return;
            var xAddress = Address;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.Address = xAddress);
        }
        partial void OnPhonChanged()
        {
            if (tblPatient == null)
                return;
            var xPhon = Phon;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.Phon = xPhon);
        }
        partial void OnPaspSeriyaChanged()
        {
            if (tblPatient == null)
                return;
            var xPaspSeriya = PaspSeriya;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.PaspSeriya = xPaspSeriya);
        }
        partial void OnPaspNumberChanged()
        {
            if (tblPatient == null)
                return;
            var xPaspNumber = PaspNumber;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.PaspNumber = xPaspNumber);
        }
        partial void OnPaspIssuingChanged()
        {
            if (tblPatient == null)
                return;
            var xPaspIssuing = PaspIssuing;
            ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblPatient, doc => doc.PaspIssuing = xPaspIssuing);
        }

        #endregion

        public string ShortName
        {
            get { return ModelFunctions.GetShortName(this as IPatientFIO); }
        }

        private bool? FIsSelected;
        public bool IsSelected
        {
            get
            {
                return FIsSelected.HasValue ? FIsSelected.Value : !Printed;
            }
            set
            {
                if (FIsSelected != value)
                {
                    FIsSelected = value;
                    //if (tblManipulation != null) // пока синхронизируем
                    //    ViewDataContext.UpdateJoinDocs<tblDocumentCommon>(tblManipulation, doc => doc.IsSelected = value);
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        private short FCopuesCount;
        public short CopiesCount
        {
            get { return FCopuesCount; }
            set
            {
                if (FCopuesCount != value)
                {
                    FCopuesCount = value;
                    OnPropertyChanged("CopuesCount");
                }
            }
        }

        //partial void OnPrintedChanged()
        //{
        //    if (Printed)
        //        IsSelected = false;
        //}

        protected virtual void DoSetDefaultParams()
        {
            IsSelected = true;
        }
        public void SetDefaultParams()
        {
            DoSetDefaultParams();
        }

        protected virtual void DoPatientInfoToDocument(tblPatientInfo patientInfo)
        {
            FirstName = patientInfo.FirstName;
            LastName = patientInfo.LastName;
            FatherName = patientInfo.FatherName;
            BirthDay = patientInfo.BirthDay;
            Address = patientInfo.Address;
            Phon = patientInfo.Phon;
            PaspSeriya = patientInfo.PaspSeriya;
            PaspNumber = patientInfo.PaspNumber;
            PaspIssuing = patientInfo.PaspIssuing;
        }
        public void CopyPatientInfoToDocument()
        {
            tblPatientInfo xPatientInfo = tblPatient as tblPatientInfo;
            DoPatientInfoToDocument(xPatientInfo);
        }

        protected virtual void DoManipulationToDocument(tblManipulation manipulation)
        {
            Price = manipulation.Price;
            Eye = manipulation.Eye;
            DateRealization = manipulation.DateRealization;
            var xOffer = manipulation.tblOffer;
        }
        public void CopyManipulationToDocument()
        {
            DoManipulationToDocument(tblManipulation);
        }

        protected abstract string GetNativeDocType();
        public string NativeDocType
        {
            get { return GetNativeDocType(); }
        }
    }

    public partial class tblDocumentProduct : IDocProduct
    {
        partial void OnProductNameChanged()
        {
            if (tblManipulation == null)
                return;
            var xProductName = ProductName;
            tblManipulation.OfferCustomText = xProductName;
            ViewDataContext.UpdateJoinDocs<IDocProduct>(tblManipulation, doc => doc.ProductName = xProductName);
        }
        partial void OnProductModelChanged()
        {
            if (tblManipulation == null)
                return;
            var xProductModel = ProductModel;
            ViewDataContext.UpdateJoinDocs<IDocProduct>(tblManipulation, doc => doc.ProductModel = xProductModel);
        }

        public tblDocumentProduct() :
            base()
        {
            CopiesCount = 2;
        }

        //protected override void DoSetDefaultParams()
        //{
        //    base.DoSetDefaultParams();
        //    CopuesCount = 2;
        //}
        protected override void DoManipulationToDocument(tblManipulation manipulation)
        {
            base.DoManipulationToDocument(manipulation);
            ProductName = manipulation.OfferCustomText;
            ProductModel = manipulation.tblOffer.ProductModel;
        }
        protected override string GetNativeDocType()
        {
            return "Договор продажи";
        }
    }
    public partial class tblDocumentSalesReceipt : IDocProduct
    {
        partial void OnProductNameChanged()
        {
            if (tblManipulation == null)
                return;
            var xProductName = ProductName;
            tblManipulation.OfferCustomText = xProductName;
            ViewDataContext.UpdateJoinDocs<IDocProduct>(tblManipulation, doc => doc.ProductName = xProductName);
        }
        partial void OnProductModelChanged()
        {
            if (tblManipulation == null)
                return;
            var xProductModel = ProductModel;
            ViewDataContext.UpdateJoinDocs<IDocProduct>(tblManipulation, doc => doc.ProductModel = xProductModel);
        }

        public tblDocumentSalesReceipt() :
            base()
        {
            CopiesCount = 1;
        }

        //protected override void DoSetDefaultParams()
        //{
        //    base.DoSetDefaultParams();
        //    CopuesCount = 1;
        //}
        protected override void DoManipulationToDocument(tblManipulation manipulation)
        {
            base.DoManipulationToDocument(manipulation);
            ProductName = manipulation.OfferCustomText;
            ProductModel = manipulation.tblOffer.ProductModel;
        }
        protected override string GetNativeDocType()
        {
            return "Товарный чек";
        }
    }
    public partial class tblDocumentService : IDocService
    {
        partial void OnNameChanged()
        {
            if (tblManipulation == null)
                return;
            var xName = Name;
            ViewDataContext.UpdateJoinDocs<IDocService>(tblManipulation, doc => doc.Name = xName);
        }
        public tblDocumentService() :
            base()
        {
            CopiesCount = 2;
        }
        //protected override void DoSetDefaultParams()
        //{
        //    base.DoSetDefaultParams();
        //    CopuesCount = 2;
        //}
        protected override void DoManipulationToDocument(tblManipulation manipulation)
        {
            base.DoManipulationToDocument(manipulation);
            Name = manipulation.OfferCustomText;
        }
        protected override string GetNativeDocType()
        {
            return "Услуга";
        }
    }

    public partial class tblReferralVendor : IIndestructibleObject
    {
        #region IIndestructibleObject
        public bool ForceRemove { get; set; }
        #endregion
    }
    public partial class tblOffer : IIndestructibleObject
    {
        #region IIndestructibleObject
        public bool ForceRemove { get; set; }
        #endregion

        private bool? FIsReadOnly;
        public bool IsReadOnly
        {
            get { return FIsReadOnly.HasValue ? FIsReadOnly.Value : ID != Guid.Empty; }
            set { FIsReadOnly = value; }
        }

    }
    public partial class tblEmployee : IIndestructibleObject
    {
        #region IIndestructibleObject
        public bool ForceRemove { get; set; }
        #endregion
    }

    //public partial class          : IIndestructibleObject { }
    //public partial class          : IIndestructibleObject { }
    //public partial class          : IIndestructibleObject { }
    //public partial class          : IIndestructibleObject { }

}
