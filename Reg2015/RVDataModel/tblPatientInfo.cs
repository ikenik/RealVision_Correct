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
    
    public partial class tblPatientInfo : tblPatient
    {
        
    
        partial void OnAddressChanging(string value);
        partial void OnAddressChanged();
        private string _Address;
        public string Address 
        { 
            get { return _Address; } 
            set
            {
                if(_Address != value)
                {
                    OnAddressChanging(_Address);
                    _Address = value;
                    OnAddressChanged();
                    OnPropertyChanged("Address");
                }
            }
        }
        
    
        partial void OnPaspSeriyaChanging(string value);
        partial void OnPaspSeriyaChanged();
        private string _PaspSeriya;
        public string PaspSeriya 
        { 
            get { return _PaspSeriya; } 
            set
            {
                if(_PaspSeriya != value)
                {
                    OnPaspSeriyaChanging(_PaspSeriya);
                    _PaspSeriya = value;
                    OnPaspSeriyaChanged();
                    OnPropertyChanged("PaspSeriya");
                }
            }
        }
        
    
        partial void OnPaspNumberChanging(string value);
        partial void OnPaspNumberChanged();
        private string _PaspNumber;
        public string PaspNumber 
        { 
            get { return _PaspNumber; } 
            set
            {
                if(_PaspNumber != value)
                {
                    OnPaspNumberChanging(_PaspNumber);
                    _PaspNumber = value;
                    OnPaspNumberChanged();
                    OnPropertyChanged("PaspNumber");
                }
            }
        }
        
    
        partial void OnPaspIssuingChanging(string value);
        partial void OnPaspIssuingChanged();
        private string _PaspIssuing;
        public string PaspIssuing 
        { 
            get { return _PaspIssuing; } 
            set
            {
                if(_PaspIssuing != value)
                {
                    OnPaspIssuingChanging(_PaspIssuing);
                    _PaspIssuing = value;
                    OnPaspIssuingChanged();
                    OnPropertyChanged("PaspIssuing");
                }
            }
        }
        
    
        partial void OnJobChanging(string value);
        partial void OnJobChanged();
        private string _Job;
        public string Job 
        { 
            get { return _Job; } 
            set
            {
                if(_Job != value)
                {
                    OnJobChanging(_Job);
                    _Job = value;
                    OnJobChanged();
                    OnPropertyChanged("Job");
                }
            }
        }
        
    
        partial void OnPostChanging(string value);
        partial void OnPostChanged();
        private string _Post;
        public string Post 
        { 
            get { return _Post; } 
            set
            {
                if(_Post != value)
                {
                    OnPostChanging(_Post);
                    _Post = value;
                    OnPostChanged();
                    OnPropertyChanged("Post");
                }
            }
        }
        
    
        partial void OnPhonChanging(string value);
        partial void OnPhonChanged();
        private string _Phon;
        public string Phon 
        { 
            get { return _Phon; } 
            set
            {
                if(_Phon != value)
                {
                    OnPhonChanging(_Phon);
                    _Phon = value;
                    OnPhonChanged();
                    OnPropertyChanged("Phon");
                }
            }
        }
        
    
        partial void OnNumberChanging(int value);
        partial void OnNumberChanged();
        private int _Number;
        public int Number 
        { 
            get { return _Number; } 
            set
            {
                if(_Number != value)
                {
                    OnNumberChanging(_Number);
                    _Number = value;
                    OnNumberChanged();
                    OnPropertyChanged("Number");
                }
            }
        }
        
    
        partial void OnKindChanging(CardKind value);
        partial void OnKindChanged();
        private CardKind _Kind;
        public CardKind Kind 
        { 
            get { return _Kind; } 
            set
            {
                if(_Kind != value)
                {
                    OnKindChanging(_Kind);
                    _Kind = value;
                    OnKindChanged();
                    OnPropertyChanged("Kind");
                }
            }
        }
        
    
        partial void OnLocationChanging(CardLocation value);
        partial void OnLocationChanged();
        private CardLocation _Location;
        public CardLocation Location 
        { 
            get { return _Location; } 
            set
            {
                if(_Location != value)
                {
                    OnLocationChanging(_Location);
                    _Location = value;
                    OnLocationChanged();
                    OnPropertyChanged("Location");
                }
            }
        }
        
    
        partial void OnPrintedChanging(bool value);
        partial void OnPrintedChanged();
        private bool _Printed;
        public bool Printed 
        { 
            get { return _Printed; } 
            set
            {
                if(_Printed != value)
                {
                    OnPrintedChanging(_Printed);
                    _Printed = value;
                    OnPrintedChanged();
                    OnPropertyChanged("Printed");
                }
            }
        }
    }
}
