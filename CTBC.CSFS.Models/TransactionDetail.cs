using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class TransactionDetail
    {
        public string TrnNum { get; set; }
        public string DATA_DATE { get; set; }
        public string ACCT_NO { get; set; }
        public string JNRST_DATE { get; set; }
        public string JNRST_TIME { get; set; }
        public string JNRST_TIME_SEQ { get; set; }
        public string TRAN_DATE { get; set; }
        public string POST_DATE { get; set; }
        public string TRANS_CODE { get; set; }
        public string JRNL_NO { get; set; }
        public string REVERSE { get; set; }
        public string PROMO_CODE { get; set; }
        public string REMARK { get; set; }
        public string TRAN_AMT { get; set; }
        public string BALANCE { get; set; }
        public string TRF_BANK { get; set; }
        public string TRF_ACCT { get; set; }
        public string NARRATIVE { get; set; }
        public string FISC_BANK { get; set; }
        public string FISC_SEQNO { get; set; }
        public string CHQ_NO { get; set; }
        public string ATM_NO { get; set; }
        public string TRAN_BRANCH { get; set; }
        public string TELLER { get; set; }
        public string FILLER { get; set; }
        public string TXN_DESC { get; set; }
        public string ACCT_P2 { get; set; }
        public string FILE_NAME { get; set; }
        public string TYPE { get; set; }
    }
}
