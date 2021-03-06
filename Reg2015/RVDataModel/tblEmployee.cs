//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Reg2015.RVDataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblEmployee : System.ComponentModel.INotifyPropertyChanged
    {
        #region Implement INotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblEmployee()
        {
            this.tblOffers = new HashSet<tblOffer>();
            // this.tblOffers = new System.Collections.ObjectModel.ObservableCollection<tblOffer>(); // Н.З. тоже работает
            this.tblManipulations = new HashSet<tblManipulation>();
            // this.tblManipulations = new System.Collections.ObjectModel.ObservableCollection<tblManipulation>(); // Н.З. тоже работает
        }
    
        
    
        partial void OnIDChanging(System.Guid value);
        partial void OnIDChanged();
        private System.Guid _ID;
        public System.Guid ID 
        { 
            get { return _ID; } 
            set
            {
                if(_ID != value)
                {
                    OnIDChanging(_ID);
                    _ID = value;
                    OnIDChanged();
                    OnPropertyChanged("ID");
                }
            }
        }
        
    
        partial void OnFirstNameChanging(string value);
        partial void OnFirstNameChanged();
        private string _FirstName;
        public string FirstName 
        { 
            get { return _FirstName; } 
            set
            {
                if(_FirstName != value)
                {
                    OnFirstNameChanging(_FirstName);
                    _FirstName = value;
                    OnFirstNameChanged();
                    OnPropertyChanged("FirstName");
                }
            }
        }
        
    
        partial void OnLastNameChanging(string value);
        partial void OnLastNameChanged();
        private string _LastName;
        public string LastName 
        { 
            get { return _LastName; } 
            set
            {
                if(_LastName != value)
                {
                    OnLastNameChanging(_LastName);
                    _LastName = value;
                    OnLastNameChanged();
                    OnPropertyChanged("LastName");
                }
            }
        }
        
    
        partial void OnFatherNameChanging(string value);
        partial void OnFatherNameChanged();
        private string _FatherName;
        public string FatherName 
        { 
            get { return _FatherName; } 
            set
            {
                if(_FatherName != value)
                {
                    OnFatherNameChanging(_FatherName);
                    _FatherName = value;
                    OnFatherNameChanged();
                    OnPropertyChanged("FatherName");
                }
            }
        }
        
    
        partial void OnBirthDayChanging(Nullable<System.DateTime> value);
        partial void OnBirthDayChanged();
        private Nullable<System.DateTime> _BirthDay;
        public Nullable<System.DateTime> BirthDay 
        { 
            get { return _BirthDay; } 
            set
            {
                if(_BirthDay != value)
                {
                    OnBirthDayChanging(_BirthDay);
                    _BirthDay = value;
                    OnBirthDayChanged();
                    OnPropertyChanged("BirthDay");
                }
            }
        }
        
    
        partial void OnShortNameChanging(string value);
        partial void OnShortNameChanged();
        private string _ShortName;
        public string ShortName 
        { 
            get { return _ShortName; } 
            set
            {
                if(_ShortName != value)
                {
                    OnShortNameChanging(_ShortName);
                    _ShortName = value;
                    OnShortNameChanged();
                    OnPropertyChanged("ShortName");
                }
            }
        }
        
    
        partial void OnDateDeleteChanging(Nullable<System.DateTime> value);
        partial void OnDateDeleteChanged();
        private Nullable<System.DateTime> _DateDelete;
        public Nullable<System.DateTime> DateDelete 
        { 
            get { return _DateDelete; } 
            set
            {
                if(_DateDelete != value)
                {
                    OnDateDeleteChanging(_DateDelete);
                    _DateDelete = value;
                    OnDateDeleteChanged();
                    OnPropertyChanged("DateDelete");
                }
            }
        }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        private ICollection<tblOffer> _tblOffers;
        public virtual ICollection<tblOffer> tblOffers 
        { 
            get { return _tblOffers; } 
            set
            {
                if(_tblOffers != value)
                {
                    _tblOffers = value;
                    OnPropertyChanged("tblOffers");
                }
            }
        }
    	
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        private ICollection<tblManipulation> _tblManipulations;
        public virtual ICollection<tblManipulation> tblManipulations 
        { 
            get { return _tblManipulations; } 
            set
            {
                if(_tblManipulations != value)
                {
                    _tblManipulations = value;
                    OnPropertyChanged("tblManipulations");
                }
            }
        }
    	
    }
}
