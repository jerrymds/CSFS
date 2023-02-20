using System;
using System.Collections.Generic;
using CTBC.CSFS.Models;
namespace CTBC.CSFS.ViewModels
{
    public class PaySettingViewModel
    {
        /// <summary>
        /// 來文字號
        /// </summary>
        public string GovNo { get; set; }
        /// <summary>
        /// 解扣日
        /// </summary>
        public string BreakDay { get; set; }
        public Guid CaseId { get; set; }
        /// <summary>
        /// 案件類型, 扣押/外來文 (預留,支付設定這裡只可能是扣押啊)
        /// </summary>
        public string CaseKind { get; set; }
        /// <summary>
        /// 案件類型2,
        /// </summary>
        public string CaseKind2 { get; set; }
        /// <summary>
        /// 是否已經儲存過
        /// </summary>
        public bool AlreadySaved { get; set; }

        /// <summary>
        /// 已經設定的 支付設定
        /// </summary>
        public IList<CaseSeizure> ListPay { get; set; }

        /// <summary>
        /// 搜索出來同ID 沒有結案的扣押設定
        /// </summary>
        public IList<CaseSeizure> ListSeizure { get; set; } 

    }
}