using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseClosedQuery
    {
        public string CaseKind { get; set; }
        public string CaseKind2 { get; set; }
        public string ReceiveDateStart { get; set; }
        public string ReceiveDateEnd { get; set; }
        public string SendDateStart { get; set; }
        public string SendDateEnd { get; set; }
        public string CloseDateStart { get; set; }
        public string CloseDateEnd { get; set; }
        public string AccountKind { get; set; }
        public Guid CaseId { get; set; }
        public string Depart { get; set; }
        public string Case { get; set; }
        public string ReturnCase { get; set; }
        public string ReturnCaseRate { get; set; }
        public string OutCase { get; set; }
        public string OutCaseRate { get; set; }
        public string ApproveDateStart { get; set; }
        public string ApproveDateEnd { get; set; }
        public string SendKind { get; set; }
        public string SendUpDateStart { get; set; }
        public string SendUpDateEnd { get; set; }
    }
}
