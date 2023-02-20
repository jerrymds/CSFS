/// <summary>
/// 程式說明：CSFSLog View Object
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using CTBC.FrameWork.Util;
using System.ComponentModel.DataAnnotations;

namespace CTBC.CSFS.Models
{
    public class CSFSLogVO : Entity
    {
        /// <summary>
        /// 序號
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// Log編號
        /// </summary>
        public int LogID { get; set; }

        /// <summary>
        /// Log發生時間
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Log分類
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Log訊息與內容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Log優先程度
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 事件編號
        /// </summary>
        public int EventID { get; set; }

        /// <summary>
        /// 事件的緊急程度
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// 產生此事件的使用者員編
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 事件的處理結果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 事件的功能編號
        /// </summary>
        public string FunctionId { get; set; }

        /// <summary>
        /// 事件的Session編號
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// 事件的URL
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 事件的IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 事件的來源Host
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// 查詢Log開始時間
        /// </summary>
        [DateExpression()]
        //[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]/// 
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 查詢Log結束時間
        /// </summary>
        [DateExpression()]
        //[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int maxnum { get; set; }
    }
}