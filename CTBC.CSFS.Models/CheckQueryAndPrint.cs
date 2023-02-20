using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CheckQueryAndPrint
    {
        public string CheckNo { get; set; }
        public string CaseNo { get; set; }
        public string CheckDate { get; set; }
        public string PayDate { get; set; }
        public string ReceivePerson { get; set; }
        public string Money { get; set; }
        public string Fee { get; set; }
        public string GovNo { get; set; }
        public string CaseKind2 { get; set; }
        public string AgentUser { get; set; }
        public Guid CaseId { get; set; }
        public string SendNo { get; set; }
        public string Type { get; set; }

        public int PageSize{get;set;}
        public int CurrentPage{get;set;}
        public int TotalItemCount{get;set;}
        public string SortDirection{get;set;}
        public string SortExpression{get;set;}
        public int maxnum { get; set; }
        public int num { get; set; }

        public string CreatedDate { get; set; }
        public string Account { get; set; }
        public string CustId { get; set; }
        public string CustName { get; set; }
        public string PayAmount { get; set; }
        public string sum1 { get; set; }

        public int PayeeId { get; set; }

        public string  Status { get; set; }

        //Add by zhangwei 20180315 start
        public string AmtConsistentType { get; set; }
        public string CheckNoStart { get; set; }
        public string CheckNoEnd { get; set; }
        public string SeizureAmountSUB { get; set; }

        public string ProdCode { get; set; }

        public string Currency { get; set; }

        public string TotalID { get; set; }
        public string TotalPayment { get; set; }
        public string TotalSeizureAmount { get; set; }
        public string TotalFee { get; set; }
        //Add by zhangwei 20180315 end
    }
}
