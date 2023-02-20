using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseCustOutputF2
    {
        public int Id { get; set; }
        public string DocNo { get; set; }
        public Guid MasterId { get; set; }
        public Guid DetailsId { get; set; }
        public string CUST_ID_NO { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public String ACCT_NO { get; set; }
        public string JRNL_NO { get; set; }
        public string TRAN_DATE { get; set; }
        public string JNRST_TIME { get; set; }
        public string TRAN_BRANCH { get; set; }
        public string TXN_DESC { get; set; }
        public string CURRENCY { get; set; }
        public string TRAN_AMT { get; set; }
        public string SAVEAMT { get; set; }
        public string BALANCE { get; set; }
        public string ATM_NO { get; set; }
        public string TELLER { get; set; }
        public string TRF_BANK { get; set; }
        public string NARRATIVE { get; set; }
        public string PD_TYPE_DESC { get; set; }
    }
}
