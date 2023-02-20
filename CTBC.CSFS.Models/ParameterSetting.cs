/// <summary>
/// 程式說明:DB Entity--參數@Lang.CSFS_detail檔
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CTBC.FrameWork.Util;


namespace CTBC.CSFS.Models
{
    /// <summary>
    /// DB 實體類
    /// </summary>
    public class ParameterSetting : Entity
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SNO { get; set; }

        /// <summary>
        /// 參數類別碼
        /// </summary>
        [NumericletterCodeType()]
        [Required]
        [Display(Name = "csfs_sys_param_id", ResourceType = typeof(Resource.Lang))]
        public string param_id { get; set; }

        /// <summary>
        /// 參數描述
        /// </summary>
        [Required]
        [Display(Name = "csfs_sys_param_desc", ResourceType = typeof(Resource.Lang))]
        public string param_desc { get; set; }

        /// <summary>
        /// 類別
        /// </summary>
        [Display(Name = "csfs_sys_param_type", ResourceType = typeof(Resource.Lang))]
        public string param_type { get; set; }

        /// <summary>
        /// 類別ID
        /// </summary>
        public string param_typeid { get; set; }

        /// <summary>
        /// 參數代碼
        /// </summary>
        public string prop_id { get; set; }

        /// <summary>
        /// 參數代碼
        /// </summary>
        public string prop_name { get; set; }

        /// <summary>
        /// 代碼描述
        /// </summary>
        public string prop_desc { get; set; }

        /// <summary>
        /// 語言
        /// </summary>
        public string language { get; set; }

        /// <summary>
        /// 顯示次序
        /// </summary>
        public string sort { get; set; }

        /// <summary>
        /// 下拉顯示
        /// </summary>
        public string is_show { get; set; }

        /// <summary>
        /// 快速查詢條件
        /// </summary>
        public string QuickSearchCon { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int maxnum { get; set; }
        public string pop_id { get; set; }
        public string parent_param_id { get; set; }
        public string parent_prop_id { get; set; }
        public string detlLanguage { get; set; }
    }
}