using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Reg2015.RVDataModel
{
    public interface IDocumentCommon
    {
        string Address { get; set; }
        DateTime? BirthDay { get; set; }
        DateTime DateCreate { get; set; }
        DateTime? DateDelete { get; set; }
        DateTime? DateRealization { get; set; }
        Eye? Eye { get; set; }
        string FatherName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        Guid ManipulationID { get; set; }
        int Number { get; set; }
        string PaspIssuing { get; set; }
        string PaspNumber { get; set; }
        string PaspSeriya { get; set; }
        Guid PatientID { get; set; }
        string Phon { get; set; }
        decimal? Price { get; set; }
        bool Printed { get; set; }
        Guid? ReplaceToID { get; set; }
    }

    public interface IDocProduct : IDocumentCommon
    {
        string ProductModel { get; set; }
        string ProductName { get; set; }
    }

    public interface IDocService : IDocumentCommon
    {
        string Name { get; set; }
    }

    interface IDocOwner {
        ICollection<tblDocumentCommon> tblDocumentCommons { get; }
        ObservableCollection<tblDocumentCommon> tblDocumentCommonsObs { get; }
    }

}