using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseDeathInvestment
    {
        public int Sno { get; set; }
        public string ID_NO { get; set; }
        public string UTIDNO { get; set; }
        public string BANK_NAME { get; set; }
        public string PROVIDE_DATE { get; set; }
        public string DESCRIPTION { get; set; }
        public string UNIT { get; set; }
        public string CURRENCY { get; set; }
        public string BAL { get; set; }
        public string MEMO { get; set; }
        public Guid CaseDeadVersionNewID { get; set; }
        public string CaseNo { get; set; }
        public int Seq { get; set; }
        public DateTime CreatedDate { get; set; } 
    }
}
