/// <summary>
/// 程式說明:DB Entity--參數@Lang.CSFS_detail檔
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>
/// 

using System;
using System.ComponentModel.DataAnnotations;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Models
{
    public class PARMCode : Entity
    {
        /// <summary>
        /// 唯一識別欄位
        /// </summary>
        public string CodeUid { get; set; }

        /// <summary>
        /// 參數類別碼
        /// </summary>
        [NumericletterCodeType()]
        [Required]
        [Display(Name = "csfs_pm_codetype", ResourceType = typeof(Lang))]
        public string CodeType { get; set; }

        public string CodeType1 { get; set; }

        /// <summary>
        /// 參數類別說明
        /// </summary>
        [Required]
        [Display(Name = "csfs_pm_codetypedesc", ResourceType = typeof(Lang))]
        public string CodeTypeDesc { get; set; }

        /// <summary>
        /// 參數細項代碼
        /// </summary>
        [Required]
        [Display(Name = "csfs_pm_detail_code", ResourceType = typeof(Lang))]
        public string CodeNo { get; set; }

        /// <summary>
        /// 參數細項代碼說明
        /// </summary>
        [Required]
        [Display(Name = "csfs_pm_detail_name", ResourceType = typeof(Lang))]
        public string CodeDesc { get; set; }

        /// <summary>
        /// 參數細項順序
        /// </summary>
        /// ---------------------
        /// 修改日期：2014/3/19
        /// 修改人員：莊筱婷
        /// 修改內容：(1) 將int? --> int，此欄位為必輸 (2) 加上[Required] (3) 加上[Display(Name = "parm_detail_order")]
        [Required]
        [Display(Name = "csfs_pm_detail_order", ResourceType = typeof(Lang))]
        public int SortOrder { get; set; }

        /// <summary>
        /// 參數細項標籤
        /// </summary>
        public string CodeTag { get; set; }

        /// <summary>
        /// 啟用狀態
        /// </summary>
        public bool? Enable { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string CodeMemo { get; set; }

        /// <summary>
        /// 參數代碼說明
        /// </summary>
        public string DescNO { get; set; }

        /// <summary>
        /// 優先序
        /// </summary>
        public decimal? Priority { get; set; }

        /// <summary>
        /// 策略比對條件優先序
        /// </summary>
        public string PolicyCondition { get; set; }

        /// <summary>
        /// 策略主鍵
        /// </summary>
        public Guid? NewId { get; set; }

        /// <summary>
        /// 業務別
        /// </summary>
        public string BusType { get; set; }
        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        public string QueryCodeType { get; set; }
        public string QueryCodeNo { get; set; }
        public string QueryEnable { get; set; }

        /// <summary>
        /// CodeNo+CodeTag 
        /// added by smallzhi
        /// </summary>
        public string CodeMix { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int maxnum { get; set; }

        //LDAP/RACF 檢核帳號 Added By NianhuaXiao 2018/03/07
        public string status { get; set; }
        public string msg { get; set; }
    }
}
