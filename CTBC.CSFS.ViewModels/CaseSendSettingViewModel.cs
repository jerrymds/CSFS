using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseSendSettingViewModel
    {
        // 介面顯示
        public CaseSendSetting CaseSendSetting { get; set; }

        //// 清單顯示
        public IList<CaseSendSetting> CaseSendSettingList { get; set; }

        //public IList<CaseSendSetting> SendSettingList { get; set; }


        public IList<CaseSendSettingDetails> ReceiveList { get; set; }
        public IList<CaseSendSettingDetails> CCList { get; set; }

        
    }


    /// <summary>
    /// 查詢
    /// </summary>
    public class CaseSendSettingQueryResultViewModel : CaseSendSetting
    {
        public string CaseNo { get; set; }
        public string Receiver { get; set; }
        public string Cc { get; set; }
        public string ApproveDate { get; set; }
        public DirectorToApprove directorToApprove { get; set; }
    }

    public class CaseSendSettingCreateViewModel : CaseSendSetting
    {
        public IList<CaseSendSettingDetails> ReceiveList { get; set; }
        public IList<CaseSendSettingDetails> CcList { get; set; }
        public string ReceiveKind { get; set; }
        public string CaseKind2 { get; set; }
        public string flag { get; set; }//標記發文資訊來的呈核還是帳務資訊來的呈核
    }
}
