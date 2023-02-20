using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningGenAcct : Entity
    {
        public int id { get; set; }
        public string DocNo { get; set; }
        public string TransDateTime { get; set; }
        public string HangAmount { get; set; }
        public string Balance { get; set; }
        public string eTabs { get; set; }
        public string Memo { get; set; }
        public string TimeLog { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }
        //Add by zhangwei 20180315 start
        public string CHQ_PAYEE { get; set; }
        public string INST_NO { get; set; }
        public string HOME_BRCH { get; set; }
        public string ACCT_NO { get; set; }
        public string ACT_DATE_TIME { get; set; }
        public string ACT_DATE { get; set; }
        public string ACT_CCYY { get; set; }
        public string ACT_MM { get; set; }
        public string ACT_DD { get; set; }
        public string ACT_TIME { get; set; }
        public string TRAN_TYPE { get; set; }
        public string TRAN_STATUS { get; set; }
        public string TRAN_DATE { get; set; }
        public string BRANCH { get; set; }
        public string BRANCH_TERM { get; set; }
        public string TELLER { get; set; }
        public string TRAN_CODE { get; set; }
        public string POST_DATE { get; set; }
        public string JRNL_NO { get; set; }
        public string AMOUNT { get; set; }
        public string BTCH_NO_U { get; set; }
        public string CORRECTION { get; set; }
        public string DEFER_DAYS { get; set; }
        public string FOREIGN_FLAG { get; set; }
        public string FILLER { get; set; }
        public string CreatedId { get; set; }
        public string CreatedTime { get; set; }
        public string ACCOUNT_NO { get; set; }
        public string  SYSTEM { get; set; }
        public string  DESCR { get; set; }
        //master
        public string CustAccount { get; set; }
        public string AccountStatus { get; set; }
        public string BankID { get; set; }
        public string BankName { get; set; }
        public int IsRelease { get; set; }
        public string ClosedDate { get; set; }
        public string Currency { get; set; }
        public string NotifyBal { get; set; }
        public string CurBal { get; set; }
        public string ReleaseBal { get; set; }
        public string VD { get; set; }
        public string MD { get; set; }
    }
}
