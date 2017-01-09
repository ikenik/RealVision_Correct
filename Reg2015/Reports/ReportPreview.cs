using CrystalDecisions.CrystalReports.Engine;
using System.Windows.Forms;

namespace Reg2015.Reports
{


    public partial class ReportPreview : Form
    {
        public ReportPreview()
        {
            InitializeComponent();
        }

        ReportDocument FReport;

        public void SetReport(ReportDocument report)
        {
            if (FReport != null)
            {
                FReport.Dispose();
                FReport = null;
            }
            FReport = report;
            crystalReportViewer1.ReportSource = report;
            //crystalReportViewer1.RefreshReport();
            //crystalReportViewer1.pa
        }

        private void ReportPreview_FormClosing(object sender, FormClosingEventArgs e)
        {
            FReport.Dispose(); // вообще не правильно
        }
    }

    //public class CrystalReportViewerEx : CrystalReportViewer
    //{

    //}
}
