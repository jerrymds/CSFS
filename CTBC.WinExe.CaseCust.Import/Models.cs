using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.CaseCust.Import
{
    public class batchmetadata
    {
        public string CompanyId { get; set; }
        public string BatchNo { get; set; }
        public DateTime BatchDate { get; set; }
        public int TransferType { get; set; }
        public int TotalNumber { get; set; }
        public string[] CnoNoInfo { get; set; }
    }
    public class metadata
    {
        public string CnoNo { get; set; }
        public File[] Files { get; set; }
        public string CaseKind { get; set; }
    }

    public class File
    {
        public string FileType { get; set; }
        public string FileName { get; set; }
    }

    public class GssDoc
    {
        public int id { get; set; }
        public Nullable<int> DocType { get; set; }
        public string BatchNo { get; set; }
        public string CompanyID { get; set; }
        public string Batchmetadata { get; set; }
        public Nullable<System.DateTime> BatchDate { get; set; }
        public string TransferType { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> ParserStatus { get; set; }
        public string ParserMessage { get; set; }
    }

    public class GssDoc_Detail
    {
        public int id { get; set; }
        public string BatchNo { get; set; }
        public string DocNo { get; set; }
        public string Metadata { get; set; }
        public string FileNames { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> ParserStatus { get; set; }
        public string ParserMessage { get; set; }
        public string CaseKind { get; set; }
        public Nullable<System.Guid> CaseId { get; set; }
        public string SendBatchNo { get; set; }
        public Nullable<System.DateTime> SendDate { get; set; }
        public Nullable<int> SendStatus { get; set; }
        public string SendMessage { get; set; }
        public string Sendmetadata { get; set; }
    }

    public class CaseCustImportModel
	{
		public string type { get; set; }
		public string id { get; set; }
		public string accno { get; set; }
		public string sdate { get; set; }
		public string edate { get; set; }
		public Guid caseid { get; set; }
        public bool isValid { get; set; }
        public string ErrorMessage { get; set; }
	}
}
