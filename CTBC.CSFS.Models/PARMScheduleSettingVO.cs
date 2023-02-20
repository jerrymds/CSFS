/// <summary>
/// 程式說明：PARMScheduleSetting View Object
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTBC.CSFS.Models
{
    public class PARMScheduleSettingVO : Entity
    {
        /// <summary>
        /// 序號
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// 排程ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 排程名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 排程路徑
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// 排程參數
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public string Enabled { get; set; }

        /// <summary>
        /// 排程狀態
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// OneTime啟動時間
        /// </summary>
        public DateTime? OneTime { get; set; }

        /// <summary>
        /// 定時啟動-時
        /// </summary>
        public int? RegularHour { get; set; }

        /// <summary>
        /// 定時啟動-分
        /// </summary>
        public int? RegularMinute { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int maxnum { get; set; }
    }
}
