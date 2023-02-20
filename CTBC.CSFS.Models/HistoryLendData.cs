using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryLendData : Entity
    {
        public int LendID { get; set; }
        public Guid CaseId { get; set; }
        public string DocNo { get; set; }
        public string ClientID { get; set; }
        public string Name { get; set; }
        public string BankID { get; set; }
        public string Bank { get; set; }
        public string Account { get; set; }
        public string Memo { get; set; }
        public string Phone { get; set; }
        public string ReturnDate { get; set; }
        public string ReturnBankDate { get; set; }
        public string RetrunBankID { get; set; }
        public string ReturnBank { get; set; }
        public string ReturnPostNo { get; set; }
        public string BankReceiver { get; set; }
        public string ReturnMemo { get; set; }
        public string CaseNo { get; set; }
        public string returnCaseNo { get; set; }
        public string LendStatus { get; set; }
        public Guid? ReturnCaseId { get; set; }
        public string AttachNames { get; set; }//附件
        public int Sno { get; set; }//序號
        public string GovNo { get; set; }//來文字號
        public int maxnum { get; set; }
        public string CaseKind { get; set; }
        public string CaseKind2 { get; set; }
        public string GovKind { get; set; }
        public string GovUnit { get; set; }
        public string GovDate { get; set; }
        public string Speed { get; set; }
        public string ReceiveKind { get; set; }
        public string AgentUser { get; set; }
        public string GovDateStart { get; set; }
        public string GovDateEnd { get; set; }

        public string SendDate { get; set; }
        public string SendNo { get; set; }

        public string MailDate { get; set; }
        public string MailNo { get; set; }

        //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
        public string AgentDepartment { get; set; }
        public string AgentDepartment2 { get; set; }
        public string AgentDepartmentUser { get; set; }
       //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end
    }
}
