using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class ImportEdocData
    {
        public string SerialID { get; set; }
        public DateTime ExecutedDate { get; set; }
        public string Timesection { get; set; }
        public string DocNo { get; set; }
        public string CaseNo { get; set; }
        public string GovUnit { get; set; }
        public string GovNo { get; set; }
        public string Added { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string GovDateS { get; set; }
        public string GovDateE { get; set; }
        public DateTime GovDate { get; set; }
        public string ExecutedDateS { get; set; }
        public string ExecutedDateE { get; set; }
        public int maxnum { get; set; }
        public string SortExpression { get; set; }
        public string SortDirection { get; set; }
        public int TotalItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
