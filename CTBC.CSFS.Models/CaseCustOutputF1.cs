using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseCustOutputF1
    {
        public int Id {get;set;} 
        public string DocNo {get;set;} 
        public Guid MasterId {get;set;} 
        public Guid DetailsId {get;set;} 
        public string CUST_ID_NO {get;set;} 
        public string BRANCH_NO {get;set;} 
        public string PD_TYPE_DESC {get;set;} 
        public string CURRENCY {get;set;} 
        public string CUSTOMER_NAME {get;set;} 
        public string NIGTEL_NO {get;set;} 
        public string MOBIL_NO {get;set;} 
        public string COMM_ADDR {get;set;} 
        public string CUST_ADD {get;set;} 
        public string ACCT_NO {get;set;} 
        public DateTime HTGModifiedDate {get;set;} 
        public string OPEN_DATE {get;set;} 
        public string CLOSE_DATE {get;set;} 
        public string CUR_BAL {get;set;} 
        public string Memo  {get;set;}
        public string LAST_DATE { get; set; }
    }
}
