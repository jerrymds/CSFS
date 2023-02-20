using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class HistoryCaseSendSettingViewModel
    {
        // 介面顯示
        public HistoryCaseSendSetting CaseSendSetting { get; set; }

        //// 清單顯示
        public IList<HistoryCaseSendSetting> CaseSendSettingList { get; set; }

        //public IList<CaseSendSetting> SendSettingList { get; set; }


        public IList<HistoryCaseSendSettingDetails> ReceiveList { get; set; }
        public IList<HistoryCaseSendSettingDetails> CCList { get; set; }

        
    }


    /// <summary>
    /// 查詢
    /// </summary>
    public class HistoryCaseSendSettingQueryResultViewModel : HistoryCaseSendSetting
    {
        public string CaseNo { get; set; }
        public string Receiver { get; set; }
        public string Cc { get; set; }
        public string ApproveDate { get; set; }
        public DirectorToApprove directorToApprove { get; set; }
    }

    public class HistoryCaseSendSettingCreateViewModel : HistoryCaseSendSetting
    {
        public IList<HistoryCaseSendSettingDetails> ReceiveList { get; set; }
        public IList<HistoryCaseSendSettingDetails> CcList { get; set; }
        public string ReceiveKind { get; set; }
        public string flag { get; set; }//標記發文資訊來的呈核還是帳務資訊來的呈核
    }
}
