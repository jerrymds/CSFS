/// <summary>
/// 程式說明：匯出資料View Object
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.FrameWork.Util;
using System.ComponentModel.DataAnnotations;

namespace CTBC.CSFS.Models
{
    public class CSFSExportDataVO:Entity
    {
       

        /// <summary>
        /// 序號
        /// </summary>
        public string RowNum { get; set; }

        /// <summary>
        /// 收件編號
        /// </summary>
        public string ApplNo { get; set; }

        /// <summary>
        /// 身分證號
        /// </summary>
        public string CusId { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        public string CusName { get; set; }

        /// <summary>
        /// 案件類型
        /// </summary>
        public string ApplTypeCode { get; set; }

        /// <summary>
        /// 業務別
        /// </summary>
        public string BusClassType { get; set; }

        /// <summary>
        /// 是否急件
        /// </summary>
        public string RushFlag { get; set; }

        /// <summary>
        /// 變簽種類
        /// </summary>
        public string DISPType { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public string ApplyDate { get; set; }

        /// <summary>
        /// LFA人員
        /// </summary>
        public string LFAUserName { get; set; }

        /// <summary>
        /// LFA主管人員
        /// </summary>
        public string LFABossName { get; set; }

        /// <summary>
        /// LFA區域中心
        /// </summary>
        public string BUDept { get; set; }

        /// <summary>
        /// 鍵檔處理人員
        /// </summary>
        public string KeyInEmpName { get; set; }

        /// <summary>
        /// 鍵檔完成時間
        /// </summary>
        public string KeyInCompleteTime { get; set; }

        /// <summary>
        /// 鑑價處理人員
        /// </summary>
        public string APRLOwnerName { get; set; }

        /// <summary>
        /// 鑑價完成日期
        /// </summary>
        public string APRLProcDateTime { get; set; }

        /// <summary>
        /// 徵信處理人員
        /// </summary>
        public string LatestApproveApplEmpName { get; set; }

        /// <summary>
        /// 徵信人員完成時間
        /// </summary>
        public string LatestApproveApplDateTime { get; set; }

        /// <summary>
        /// 徵信核決主管人員
        /// </summary>
        public string LatestApproveSupervisorEmpName { get; set; }

        /// <summary>
        /// 徵信核決完成時間
        /// </summary>
        public string CloseDate { get; set; }

        /// <summary>
        /// 核決結果
        /// </summary>
        public string ApproveAuditResult { get; set; }

        /// <summary>
        /// LFA科別
        /// </summary>
        public string BUName { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int maxnum { get; set; }

        public CSFSExportDataVO QryVO { get; set; }

        //是否分頁
        public bool isPaging { get; set; }

        #region -- 以下為查詢區
        /// <summary>
        /// 起始申請日期(起)
        /// </summary>
        [DateExpression()]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? ApplDateS { get; set; }

        /// <summary>
        /// 起始申請日期(迄)
        /// </summary>
        [DateExpression()]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? ApplDateE { get; set; }

     

        #endregion

    }
}
