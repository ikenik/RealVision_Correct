using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;

using System.Threading.Tasks;
using Reg2015.RVDataModel;
using System.Windows.Data;
using System.Windows;
using NK.Collections.ObjectViewModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Diagnostics;

namespace Reg2015.ViewModel
{

    /// <summary>
    /// Контекст данных приложения, единый для всех связаных представлений
    /// </summary>
    public class ViewDataContext : FrameworkElement
    {
        private static ViewDataContext _instance;

        // Реализация паттерна singleton

        public static ViewDataContext Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ViewDataContext();
                return _instance;
            }
        }

        public static ObsCollectionWrap<TEntity, TCommon> PopulateCollection<TEntity, TCommon>(IEnumerable<TEntity> query, DbSet<TCommon> dataContext)
            where TEntity : class, TCommon
            where TCommon : class
        {
            return new ObsCollectionWrap<TEntity, TCommon>(dataContext, query.ToList());
        }

        public static tblManipulation CreateManipulation(ManipulationConfig config)
        {
            tblManipulation result = new tblManipulation()
            {
                tblPatient = config.Patient,
                tblOffer = config.Offer,
                OfferCustomText = config.Offer.DocName,
                Eye = config.Eye,
                Price = config.Price,
                DateRealization = config.DateRealisation,
            };
            config.Patient.tblManipulationsObs.Add(result);
            return result;
        }

        internal static void PrepareDoc<TDoc>(TDoc document, tblManipulation manipulation) where TDoc : tblDocumentCommon
        {
            document.tblPatient = manipulation.tblPatient;
            document.tblManipulation = manipulation;
            document.SetDefaultParams(); // selected = true и количестко коппий
            document.CopyPatientInfoToDocument();
            document.CopyManipulationToDocument();
        }

        public static void AddDoc(ManipulationConfig config, Func<tblDocumentCommon[]> DocFactory)
        {
            Eye xEye = config.Eye; Debug.Assert(xEye != Eye.No);
            tblManipulation xManipulation;
            Eye[] xEyes = (xEye == Eye.OU) ? new Eye[] { Eye.OS, Eye.OD } : new Eye[] { xEye };
            foreach (Eye item in xEyes)
            {
                config.Eye = item;
                xManipulation = CreateManipulation(config);
                tblDocumentCommon[] xNewDocs = DocFactory();
                foreach (tblDocumentCommon xNewDoc in xNewDocs)
                {
                    ViewDataContext.PrepareDoc(xNewDoc, xManipulation);
                    xManipulation.tblDocumentCommonsObs.Add(xNewDoc);
                    Instance.Documents.Add(xNewDoc);
                }
            }
        }

        /// <summary>
        ///  Обновить все связанные договора
        /// </summary>
        /// <param name="updater"></param>
        internal static void UpdateJoinDocs<T>(IDocOwner docOwner, Action<T> updater) where T : class, IDocumentCommon
        {
            var xDocs = docOwner.tblDocumentCommons;
            foreach (var item in xDocs)
                if (!item.Printed && (item is T))
                    updater(item as T);
        }


        private RealVisionDataContext FRVDataContext;
        public RealVisionDataContext RVDataContext
        {
            get
            {
                if (FRVDataContext == null)
                    FRVDataContext = new RealVisionDataContext();
                return FRVDataContext;
            }
        }

        #region PatientInfo
        private CollectionViewSource FPatientInfoViewSource;

        private IQueryable<tblPatient> GetPatientInfoContext()
        {
            return RVDataContext.tblPatients.Where(ptnt => ptnt is tblPatientInfo).
                Include(ptn => ptn.tblManipulations);

            //.Include("tblPatient.tblCards")
            //.Include("tblPatient.tblManipulations")
            //.Include("tblPatient.tblManipulations.tblEmployee")
            //.Include("tblPatient.tblManipulations.tblOffer");
        }
        private List<Guid> GetLastGUIDs()
        {
            StringCollection xCollection = Properties.Settings.Default.LastGUIDs;
            if (xCollection == null)
                return null;
            List<Guid> Result = new List<Guid>(xCollection.Count);
            foreach (string item in xCollection)
                Result.Add(new Guid(item.ToLower()));
            return Result;
        }
        private IQueryable<tblPatientInfo> DefaultPatientQuery()
        {
            // TODO : реализовать xLastPatient
            //List<Guid> xLastPatient = GetLastGUIDs();
            IQueryable<tblPatientInfo> result;
            //if ((xLastPatient != null) && (xLastPatient.Count > 0))
            //{
            //    result = (from card in GetPatientContext()
            //              join last in xLastPatient on card.ID equals last
            //              where card.DateDelete == null
            //              orderby card.DateCreate descending
            //              select card).Take(Properties.Settings.Default.ViewCardLimit);
            //}
            //else
            {
                result = (from ptnt in GetPatientInfoContext()
                          where (ptnt.DateDelete == null)
                          orderby ptnt.DateCreate descending
                          select ptnt as tblPatientInfo).Take(Properties.Settings.Default.ViewCardLimit);
            }
            return result;
        }
        private IQueryable<tblPatientInfo> PatientInfoByFIOQuery(string F, string I, string O, DateTime? BirthDay = null)
        {
            if (BirthDay == null)
            {
                return (from ptnt in GetPatientInfoContext()
                        where (ptnt.DateDelete == null) &
                              ptnt.FirstName.StartsWith(F) &
                              ptnt.LastName.StartsWith(I) &
                              ptnt.FatherName.StartsWith(O)
                        orderby ptnt.FirstName, ptnt.LastName, ptnt.FatherName
                        select ptnt as tblPatientInfo).Take(Properties.Settings.Default.ViewCardLimit);
            }
            else
            {
                DateTime xLow = BirthDay.Value.Date;
                DateTime xUpp = xLow.AddDays(1);
                return (from ptnt in GetPatientInfoContext()
                        where (ptnt.DateDelete == null) &&
                              ptnt.FirstName.StartsWith(F) && ptnt.LastName.StartsWith(I) && ptnt.FatherName.StartsWith(O) &&
                              ptnt.BirthDay >= xLow && ptnt.BirthDay < xUpp
                        orderby ptnt.FirstName, ptnt.LastName, ptnt.FatherName
                        select ptnt as tblPatientInfo).Take(Properties.Settings.Default.ViewCardLimit);
            }

        }
        private IQueryable<tblPatientInfo> PatientInfoByNumberQuery(int number)
        {
            return (from ptnt in GetPatientInfoContext()
                    where (ptnt.DateDelete == null) && ((ptnt as tblPatientInfo).Number == number)
                    orderby ptnt.DateCreate descending
                    select ptnt as tblPatientInfo).Take(Properties.Settings.Default.ViewCardLimit);

        }
        private IQueryable<tblPatientInfo> PatientInfoByDateBetweenQuery(DateTime low, DateTime upp)
        {
            return (from ptnt in GetPatientInfoContext()
                    where (ptnt.DateDelete == null) &&
                          ((ptnt.DateCreate >= low) && (ptnt.DateCreate < upp))
                    orderby ptnt.FirstName, ptnt.LastName, ptnt.FatherName
                    select ptnt as tblPatientInfo).Take(Properties.Settings.Default.ViewCardLimit);
        }

        private async Task SetPatientInfo(ObservableCollection<tblPatientInfo> patientInfos)
        {
            await SaveAllCangesAsync();
            PatientInfoViewSource.Source = patientInfos;
        }

        /// <summary>
        /// Скорректировать карточки и связанные с ними данные
        /// </summary>
        private void BeforeSavePatientInfo()
        {
            // TODO : проерить на сушествующие карточки
        }

        private async Task AfterSaveDocuments()
        {
            RVDataContext.spPrepareDocNumbers();

            foreach (var item in RVDataContext.tblDocumentCommons.Local.Where(d => d.Number == 0))
                await RVDataContext.Entry(item).ReloadAsync();
        }

        public Task SetDefaultCardsViewSource()
        {
            var xPatients = PopulateCollection(DefaultPatientQuery(), RVDataContext.tblPatients);
            return SetPatientInfo(xPatients);
        }
        public Task SetCardsByFIOViewSource(string F, string I, string O, DateTime? BirthDay = null)
        {
            var xPatients = PopulateCollection(PatientInfoByFIOQuery(F, I, O, BirthDay), RVDataContext.tblPatients);
            return SetPatientInfo(xPatients);
        }
        public Task SetCardsByNumberViewSource(int number)
        {
            var xPatients = PopulateCollection(PatientInfoByNumberQuery(number), RVDataContext.tblPatients);
            return SetPatientInfo(xPatients);
        }
        public Task SetCardsByDateBetweenViewSource(DateTime low, DateTime upp)
        {
            var xPatients = PopulateCollection(PatientInfoByDateBetweenQuery(low, upp), RVDataContext.tblPatients);
            return SetPatientInfo(xPatients);
        }

        public ObservableCollection<tblPatientInfo> PatientInfos
        {
            get
            {
                if (PatientInfoViewSource.Source == null)
                {
                    PatientInfoViewSource.Source = PopulateCollection(DefaultPatientQuery(), RVDataContext.tblPatients);
                }
                return (ObservableCollection<tblPatientInfo>)PatientInfoViewSource.Source;
            }
        }


        public CollectionViewSource PatientInfoViewSource
        {
            get
            {
                if (FPatientInfoViewSource == null)
                    FPatientInfoViewSource = (CollectionViewSource)FindResource("tblPatientInfoViewSource");
                Debug.Assert(FPatientInfoViewSource != null);
                return FPatientInfoViewSource;
            }
        }
        public tblPatientInfo CurrentPatient
        {
            get
            {
                return PatientInfoViewSource.View.CurrentItem as tblPatientInfo;
            }
        }
        #endregion

        #region Documents
        private CollectionViewSource FDocumentViewSource;

        private DbQuery<tblDocumentCommon> GetDocContext()
        {
            return RVDataContext.tblDocumentCommons;
            //.Include("tblPatient");
            //.Include("tblPatient.tblCards")
            //.Include("tblPatient.tblManipulations")
            //.Include("tblPatient.tblManipulations.tblEmployee")
            //.Include("tblPatient.tblManipulations.tblOffer");
        }


        private IQueryable<tblDocumentCommon> DefaultDocumentsQuery()
        {
            var xIDs = RVDataContext.tblDocumentCommons.AsNoTracking().Where(d => d.Printed == false).Select(d => d.ID);

            IQueryable<tblDocumentCommon> result;

            result = from doc in GetDocContext()
                     join id in xIDs on doc.ID equals id
                     orderby doc.DateCreate descending
                     select doc;

            // result = GetDocContext().Where(doc => ((doc.DateDelete == null) && (!doc.Printed))).OrderByDescending(doc => doc.DateCreate);//.Take(Properties.Settings.Default.ViewDocLimit);
            return result;
        }

        private IQueryable<tblDocumentCommon> DocumentsByDateQuery(DateTime value)
        {
            DateTime xLow = value.Date;
            DateTime xUpp = xLow.AddDays(1);
            return from doc in GetDocContext()
                   where (doc.DateDelete == null) &&
                   (doc.DateRealization >= xLow) && (doc.DateRealization < xUpp)
                   select doc;
        }

        private IQueryable<tblDocumentCommon> DocsByManipulation(tblManipulation manipulation)
        {
            return from doc in GetDocContext()
                   where (doc.DateDelete == null) && (doc.ManipulationID == manipulation.ID)
                   select doc;
        }

        private IQueryable<tblDocumentCommon> DocsByPatient(tblPatient patient)
        {
            return from doc in GetDocContext()
                   where (doc.DateDelete == null) && (doc.PatientID == patient.ID)
                   select doc;
        }

        private async Task SetDocs(ObservableCollection<tblDocumentCommon> docs)
        {
            await SaveAllCangesAsync();
            DocumentViewSource.Source = docs;
        }

        public Task SetDefaultDocsViewSource()
        {
            ObservableCollection<tblDocumentCommon> xDocs = PopulateCollection(DefaultDocumentsQuery(), RVDataContext.tblDocumentCommons);
            return SetDocs(xDocs);
        }

        public Task SetDocsByManipulation(tblManipulation manipulation)
        {
            ObservableCollection<tblDocumentCommon> xDocs = PopulateCollection(DocsByManipulation(manipulation), RVDataContext.tblDocumentCommons);
            return SetDocs(xDocs);
        }

        public Task SetDocsByPatient(tblPatient patient)
        {
            ObservableCollection<tblDocumentCommon> xDocs = PopulateCollection(DocsByPatient(patient), RVDataContext.tblDocumentCommons);
            return SetDocs(xDocs);
        }

        public Task SetDocsViewSourceByDate(DateTime value)
        {
            ObservableCollection<tblDocumentCommon> xDocs = PopulateCollection(DocumentsByDateQuery(value), RVDataContext.tblDocumentCommons);
            return SetDocs(xDocs);
        }

        public CollectionViewSource DocumentViewSource
        {
            get
            {
                if (FDocumentViewSource == null)
                    FDocumentViewSource = (CollectionViewSource)FindResource("tblDocumentViewSource");
                Debug.Assert(FDocumentViewSource != null);
                return FDocumentViewSource;
            }
        }
        public ObservableCollection<tblDocumentCommon> Documents
        {
            get
            {
                if (DocumentViewSource.Source == null)
                {
                    DocumentViewSource.Source = PopulateCollection(DefaultDocumentsQuery(), RVDataContext.tblDocumentCommons);
                }
                return (ObservableCollection<tblDocumentCommon>)DocumentViewSource.Source;
            }
        }
        public tblDocumentCommon CurrentDocument
        {
            get
            {
                return DocumentViewSource.View.CurrentItem as tblDocumentCommon;
            }
        }
        public IEnumerable<tblDocumentCommon> SelectesDocument
        {
            get
            {
                return Documents.Where(doc => doc.IsSelected);
            }
        }
        #endregion

        #region Коллекции словарей дляя подстановок

        private ObservableCollection<tblEmployee> FDictEmployees;
        public ObservableCollection<tblEmployee> DictEmployees
        {
            get
            {
                if (FDictEmployees == null)
                {
                    var query = from emp in RVDataContext.tblEmployees
                                where emp.DateDelete == null
                                orderby emp.ShortName
                                select emp;

                    FDictEmployees = PopulateCollection(query, RVDataContext.tblEmployees);
                }
                return FDictEmployees;
            }
        }

        private ObservableCollection<tblOffer> FDictDefaultOffers;
        /** Все предложения для которых не нужны договора */
        public ObservableCollection<tblOffer> DictDefaultOffers
        {
            get
            {
                if (FDictDefaultOffers == null)
                {
                    var query = from offer in RVDataContext.tblOffers
                                where (offer.DateDelete == null) &&
                                ((offer.Classification == OfferClassification.PrimaryReception) ||
                                 (offer.Classification == OfferClassification.Monitor) ||
                                 (offer.Classification == OfferClassification.PaidReception))
                                orderby offer.Sort, offer.Rank descending
                                select offer;
                    FDictDefaultOffers = PopulateCollection(query, RVDataContext.tblOffers);
                }
                return FDictDefaultOffers;
            }
        }


        private ObservableCollection<tblOffer> FDictProductOffers;
        private void FDictProductOffers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (tblOffer Newitem in e.NewItems)
                    Newitem.Classification = OfferClassification.Product;
        }
        /** Все продукты предложения */
        public ObservableCollection<tblOffer> DictProductOffers
        {
            get
            {
                if (FDictProductOffers == null)
                {
                    var query = from offer in RVDataContext.tblOffers
                                where (offer.DateDelete == null) && (offer.Classification == OfferClassification.Product)
                                orderby offer.Rank descending
                                select offer;
                    FDictProductOffers = PopulateCollection(query, RVDataContext.tblOffers);
                    FDictProductOffers.CollectionChanged += FDictProductOffers_CollectionChanged;
                }
                return FDictProductOffers;
            }
        }

        private ObservableCollection<tblOffer> FDictOperationOffers;
        private void FDictOperationOffers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (tblOffer Newitem in e.NewItems)
                    Newitem.Classification = OfferClassification.Operation;
        }
        /** Все продукты предложения */
        public ObservableCollection<tblOffer> DictOperationOffers
        {
            get
            {
                if (FDictOperationOffers == null)
                {
                    var query = from offer in RVDataContext.tblOffers
                                where (offer.DateDelete == null) && (offer.Classification == OfferClassification.Operation)
                                orderby offer.Rank descending
                                select offer;
                    FDictOperationOffers = PopulateCollection(query, RVDataContext.tblOffers);
                    FDictOperationOffers.CollectionChanged += FDictOperationOffers_CollectionChanged;
                }
                return FDictOperationOffers;
            }
        }


        private ObservableCollection<tblOffer> FDictOffers;
        /** Все доступные предложения */
        public ObservableCollection<tblOffer> DictOffers
        {
            get
            {
                if (FDictOffers == null)
                {
                    var query = from offer in RVDataContext.tblOffers
                                where (offer.DateDelete == null) && (offer.Classification != OfferClassification.Uncnown)
                                select offer;
                    FDictOffers = PopulateCollection(query, RVDataContext.tblOffers);
                }
                return FDictOffers;
            }
        }

        #endregion

        #region Не контролируемые коллекции

        private ObservableCollection<string> FMonitorText;
        private ObservableCollection<string> FPriceDependence;
        private ObservableCollection<string> FAnaliseText;
        private ObservableCollection<string> FDontDriveText;
        private ObservableCollection<string> FPatientRequiredTextList;
        public ObservableCollection<string> MonitorTextList
        {
            get
            {
                if (FMonitorText == null)
                {
                    var xGgroupQuery = (from offer in DictOperationOffers
                                        group offer by offer.MonitorText into text
                                        orderby text.Key
                                        select text.Key);
                    FMonitorText = new ObservableCollection<string>(xGgroupQuery);
                }
                return FMonitorText;
            }
        }
        public ObservableCollection<string> PriceDependenceList
        {
            get
            {
                if (FPriceDependence == null)
                {
                    var xGgroupQuery = (from offer in DictOperationOffers
                                        group offer by offer.PriceDependence into text
                                        orderby text.Key
                                        select text.Key);
                    FPriceDependence = new ObservableCollection<string>(xGgroupQuery);
                }
                return FPriceDependence;
            }
        }
        public ObservableCollection<string> AnaliseTextList
        {
            get
            {
                if (FAnaliseText == null)
                {
                    var xGgroupQuery = (from offer in DictOperationOffers
                                        group offer by offer.AnaliseText into text
                                        orderby text.Key
                                        select text.Key);
                    FAnaliseText = new ObservableCollection<string>(xGgroupQuery);
                }
                return FAnaliseText;
            }
        }
        public ObservableCollection<string> DontDriveTextList
        {
            get
            {
                if (FDontDriveText == null)
                {
                    var xGgroupQuery = (from offer in DictOperationOffers
                                        group offer by offer.DontDriveText into text
                                        orderby text.Key
                                        select text.Key);
                    FDontDriveText = new ObservableCollection<string>(xGgroupQuery);
                }
                return FDontDriveText;
            }
        }
        public ObservableCollection<string> PatientRequiredTextList
        {
            get
            {
                if (FPatientRequiredTextList == null)
                {
                    var xGgroupQuery = (from offer in DictOperationOffers
                                        group offer by offer.PatientRequired into text
                                        orderby text.Key
                                        select text.Key);
                    FPatientRequiredTextList = new ObservableCollection<string>(xGgroupQuery);
                }
                return FPatientRequiredTextList;
            }
        }
        public void FlashTexts()
        {
            FMonitorText = null;
            FPriceDependence = null;
            FAnaliseText = null;
            FDontDriveText = null;
            FPatientRequiredTextList = null;
        }

        #endregion


        private void DoSaveChange()
        {
            BeforeSavePatientInfo();
        }

        private Task AfterSaveChange()
        {
            return AfterSaveDocuments();
        }

        public async Task<int> SaveAllCangesAsync()
        {
            DoSaveChange();
            int result = await RVDataContext.SaveChangesAsync();
            await AfterSaveChange();
            // context.Entry<tblContract>(new tblContract()).ReloadAsync();

            // RVDataContext.tblDocumentCommons.Load()

            //RVDataContext.tblDocumentCommons.ToList();

            //foreach (var item in Documents)
            //{
            //    await RVDataContext.Entry(item).ReloadAsync();
            //}

            return result;
        }

        public int SaveAllCangesSync()
        {
            return RVDataContext.SaveChanges();
        }
    }

    /// <summary>Вспомогательный класс данных.</summary>
    /// <remarks>Класс не получает экземпляры и не сохраняет данные на сервер</remarks>
    public class DataHelper
    {
        public DataHelper()
        {
            FRVDataContext = new RealVisionDataContext();
        }
        private RealVisionDataContext FRVDataContext;
        public RealVisionDataContext RVDataContext { get { return FRVDataContext; } }

        public Task<List<tblPatientInfo>> FindPatientIDByFIOData(string f, string i, string o, DateTime birthDate)
        {
            return FRVDataContext.tblPatients.AsNoTracking().Where(ptnt => (ptnt is tblPatientInfo))
                .Where(ptnt => (ptnt.DateDelete == null) &
                               (ptnt.FirstName == f) & (ptnt.LastName == i) & (ptnt.FatherName == o) & (ptnt.BirthDay == birthDate))
                .Select(ptnt => ptnt as tblPatientInfo)
                .ToListAsync();
        }
    }

    public class ManipulationConfig
    {
        public tblPatientInfo Patient;
        public tblOffer Offer;
        public Eye Eye;
        public DateTime DateRealisation;
        public decimal Price;
    }
}
