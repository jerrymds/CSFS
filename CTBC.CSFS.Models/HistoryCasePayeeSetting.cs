using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryCasePayeeSetting : Entity
    {
        public Guid CaseId { get; set; }
        public string ReceivePerson { get; set; }
        public string GovName { get; set; }
        public string GovKind { get; set; }
        public string Address { get; set; }
        public string Receiver { get; set; }
        public string CCReceiver { get; set; }
        public string CheckNo { get; set; }
        public string CaseKind { get; set; }
        public string BankID { get; set; }
        public string Bank { get; set; }
        public string Money { get; set; }
        public string Fee { get; set; }
        public string Memo { get; set; }
        public string Currency { get; set; }
        public int PayeeId { get; set; }
        public int maxnum { get; set; }

        public string GovUnit { get; set; }
        public string CaseKind2 { get; set; }
        public string CaseNo { get; set; }
        public DateTime PayDate { get; set; }

        public string SendId { get; set; }
        public string PayAmountSum { get; set; }
        public string MoneySum { get; set; }
        /// <summary>
        /// 發票本號碼
        /// </summary>
        public string CheckIntervalId { get; set; }

        /// <summary>
        /// 受款人動作(1新增,2取號新增,3清除支票新增 4全部取號新增)
        /// </summary>
        public int PayeeAction { get; set; }
    }
}
