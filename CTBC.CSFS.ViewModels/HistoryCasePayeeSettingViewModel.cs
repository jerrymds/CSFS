using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class HistoryCasePayeeSettingViewModel
    {
        // 介面顯示
        public HistoryCasePayeeSetting CasePayeeSetting { get; set; }

        // 清單顯示
        public IList<HistoryCasePayeeSetting> CasePayeeSettingList { get; set; }

        public Guid CaseId { get; set; }
        public string ReceivePerson { get; set; }
        public string Address { get; set; }
        public string Receiver { get; set; }
        public string CCReceiver { get; set; }
        public string CheckNo { get; set; }
        public string CheckNo2 { get; set; }
        public string CaseKind { get; set; }
        public string BankID { get; set; }
        public string Bank { get; set; }
        public string Money { get; set; }
        public string Fee { get; set; }
        public string Memo { get; set; }
        public string Currency { get; set; }
        public int DocNo { get; set; }
        public int PayeeId { get; set; }
        public string PayAmountSum { get; set; }
        public string MoneySum { get; set; }

        /// <summary>
        /// 受款人動作(1新增,2取號新增,3清除支票新增)
        /// </summary>
        public int PayeeAction { get; set; }
    }
}
