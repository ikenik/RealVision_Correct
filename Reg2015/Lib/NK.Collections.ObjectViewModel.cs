using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Data.Entity;

namespace NK.Collections.ObjectViewModel
{
    /// <summary>
    /// Не удалаемы йобъект из БД
    /// </summary>
    public interface IIndestructibleObject
    {
        DateTime? DateDelete { get; set; }
        bool ForceRemove { get; set; }
    }

    /// <summary>
    /// Обертка для коллекции не удяляемых из БД объектов
    /// </summary>
    public class ObsCollectionWrap<T, TCommon> : ObservableCollection<T>
        where T : TCommon // , IIndestructibleObject //, INotifyPropertyChanged
        where TCommon : class
    {
        private DbSet<TCommon> FSetContext;

        public ObsCollectionWrap(DbSet<TCommon> setContext, List<T> list)
            : base(list)
        {
            if (setContext == null)
                throw new ArgumentNullException("EntityContext");
            FSetContext = setContext;
        }


        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T Newitem in e.NewItems)
                        FSetContext.Add(Newitem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T OldItem in e.OldItems)
                    {
                        IIndestructibleObject xObj = OldItem as IIndestructibleObject;
                        if ((xObj == null) || (xObj.ForceRemove))
                            FSetContext.Remove(OldItem);
                        else
                            xObj.DateDelete = DateTime.Now;
                    }
                    break;
                //case NotifyCollectionChangedAction.Replace:
                //    break;
                //case NotifyCollectionChangedAction.Move:
                //    break;
                //case NotifyCollectionChangedAction.Reset:
                //    break;
                //default:
                //    break;
            }
        }
    }
}
