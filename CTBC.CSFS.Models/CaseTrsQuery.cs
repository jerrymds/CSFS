using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseTrsQuery : Entity
    {
        public string DocNo { get; set; }//	案件編號
        public string CustId { get; set; }//	身分證統一編號
        public string CustAccount { get; set; }//	身分證統一編號
        public string ForCDateS { get; set; }//	查詢期間起日
        public string ForCDateE { get; set; }//	查詢期間訖日
        public string FileName { get; set; }//來文文檔名稱
        public string FilePath { get; set; }//來文文檔名稱
        public string UploadStatus { get; set; } // 上傳狀態
        public int Version { get; set; } // 版本號
        public Guid VersionKey { get; set; }
        public Guid NewID { get; set; }
        public string CreatedUser { get; set; } 
        public string CreatedDate { get; set; }
        public string ModifyUser { get; set; }
        public string ModifyDate { get; set; }

        public Boolean Option1 { get; set; }

        public Boolean Option2 { get; set; }

        public Boolean Option3 { get; set; }
    }

    // 查詢條件
    public class CaseTrsCondition : Entity
    {
        public string No { get; set; }
        public string NewID { get; set; }
        public string DocNo { get; set; }
        public string CustAccount { get; set; }
        public string Currency { get; set; }
        public string ForCDateS { get; set; }
        public string ForCDateE { get; set; }
        public string SendDate { get; set; }
        public string RecvDate { get; set; }
        public string QFileName { get; set; }
        public string QFileName2 { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }

        public string CustId { get; set; }
        public string Status { get; set; }
        public string TrnNum { get; set; }
        public string ESBStatus { get; set; }
        public string FileName { get; set; }
        public string AcctDesc { get; set; }
        public string Curr { get; set; }
        public string Channel { get; set; }

        public string Option1 { get; set; }

        public string Option2 { get; set; }

        public string Option3 { get; set; }

        // 案件狀態 
        public string CaseStatus { get; set; }

        // 處理方式   
        public string ProcessingMethod { get; set; }

        // 統一編號   
        public string CustIdNo { get; set; }

        // 回文字號
        public string GoFileNo { get; set; }

 
        /// <summary>
        /// 回文檔名稱（存款帳戶開戶資料）
        /// </summary>
        public string ROpenFileName { get; set; }

        /// <summary>
        /// 回文檔名稱（存款往來明細資料）
        /// </summary>
        public string RFileTransactionFileName { get; set; }

        /// <summary>
        /// 回文檔PDF首頁
        /// </summary>
        public string ReturnFileTitle { get; set; }

        public string ReturnFilePDF { get; set; }

        public int Version { get; set; } // 版本號


        // 頁面來源 1:主管放行;2:歷史記錄查詢與重送
        public string PageSource { get; set; }
        public string AuditStatus { get; set; }

        public string IsEnable { get; set; }


        // 拋查結果 
        public string Result { get; set; }

        // 建檔日期 S
        public string DateStart { get; set; }

        // 建檔日期 E
        public string DateEnd { get; set; }

        // 來文日期S 
        public string FileDateStart { get; set; }

        // 來文日期E
        public string FileDateEnd { get; set; }

        // 查詢項目 
        public string SearchProgram { get; set; }

        public string DataBase { get; set; }
        public string TrsDetails { get; set; }
        public List<CaseTrsCondition> CaseTrsConditionList { get; set; }
        public List<CaseTrsQuery> CaseTrsQueryList { get; set; }

    }


    public class CaseHisCondition : Entity
    {
        public string NewID { get; set; }
        public string DocNo { get; set; }
        public string CustAccount { get; set; }
        public string Currency { get; set; }
        public string ForCDateS { get; set; }
        public string ForCDateE { get; set; }
        public string SendDate { get; set; }
        public string RecvDate { get; set; }
        public string QFileName { get; set; }
        public string QFileName2 { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }
        public string PageFrom { get; set; }
        public string CustId { get; set; }
        public string Status { get; set; }
        public string TrnNum { get; set; }
        public string ESBStatus { get; set; }
        public string FileName { get; set; }
        public string AcctDesc { get; set; }
        public string Curr { get; set; }
        public string Channel { get; set; }


        // 案件狀態 
        public string CaseStatus { get; set; }

        // 處理方式   
        public string ProcessingMethod { get; set; }

        // 統一編號   
        public string CustIdNo { get; set; }

        // 回文字號
        public string GoFileNo { get; set; }


        /// <summary>
        /// 回文檔名稱（存款帳戶開戶資料）
        /// </summary>
        public string ROpenFileName { get; set; }

        /// <summary>
        /// 回文檔名稱（存款往來明細資料）
        /// </summary>
        public string RFileTransactionFileName { get; set; }

        /// <summary>
        /// 回文檔PDF首頁
        /// </summary>
        public string ReturnFileTitle { get; set; }

        public string ReturnFilePDF { get; set; }

        public int Version { get; set; } // 版本號


        // 頁面來源 1:主管放行;2:歷史記錄查詢與重送
        public string PageSource { get; set; }
        public string AuditStatus { get; set; }

        public string IsEnable { get; set; }


        // 拋查結果 
        public string Result { get; set; }

        // 建檔日期 S
        public string DateStart { get; set; }

        // 建檔日期 E
        public string DateEnd { get; set; }

        // 來文日期S 
        public string FileDateStart { get; set; }

        // 來文日期E
        public string FileDateEnd { get; set; }

        // 查詢項目 
        public string SearchProgram { get; set; }
        public string AgentDepartment { get; set; }
        public string AgentDepartment2 { get; set; }
        public string AgentDepartmentUser { get; set; }

        public string CaseId { get; set; }

    }
 

    public class CaseTrsQueryVersion : Entity
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public Guid NewID { get; set; }

        public string IdNo { get; set; }

        /// <summary>
        /// 來文檔案查詢條件檔主鍵
        /// </summary>
        public Guid CaseTrsNewID { get; set; }

        public string DocNo { get; set; }

        public string RowNum { get; set; }

        public string Seq { get; set; }

        public string chkbox { get; set; }

        /// <summary>
        /// 身分證統一編號
        /// </summary>
        public string CustID { get; set; }
        public string CustAccount { get; set; }
        public string Currency { get; set; }

        /// <summary>
        /// 開戶個人資料標記
        /// </summary>
        public string OpenFlag { get; set; }

        /// <summary>
        /// 特定期間之交易明細往來（限新臺幣）標記
        /// </summary>
        public string TransactionFlag { get; set; }

        /// <summary>
        /// 查詢期間起日
        /// </summary>
        public string QDateS { get; set; }

        /// <summary>
        /// 查詢期間訖日
        /// </summary>
        public string QDateE { get; set; }

        /// <summary>
        /// 案件狀態
        /// </summary>
        public string Status { get; set; }

        public string StatusName { get; set; }

        /// <summary>
        /// HTG查詢狀態
        /// </summary>
        public string HTGSendStatus { get; set; }

        /// <summary>
        /// HTG狀態原因
        /// </summary>
        public string HTGQryMessage { get; set; }

        /// <summary>
        /// HTG發查時間
        /// </summary>
        public DateTime? HTGModifiedDate { get; set; }

        /// <summary>
        /// RFDM查詢狀態
        /// </summary>
        public string RFDMSendStatus { get; set; }

        /// <summary>
        /// RFDM狀態原因
        /// </summary>
        public string RFDMQryMessage { get; set; }

        /// <summary>
        /// RFDM發查時間
        /// </summary>
        public DateTime? RFDModifiedDate { get; set; }

        /// <summary>
        /// 建立日
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string CreatedUser { get; set; }

        /// <summary>
        /// 修改日
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// 修改者
        /// </summary>
        public string ModifiedUser { get; set; }
        public string OpenDate { get; set; }
        public string LastDate { get; set; }
        public string CaseStatusName { get; set; }
        public string GoFileNo { get; set; }

    }

    public class CaseTrsQueryDetails : Entity
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        /// 
        public string chkbox { get; set; }
        public string RowNum { get; set; }
        public string CustID { get; set; }
        public string CustAccount { get; set; }

        /// <summary>
        /// 開戶個人資料標記
        /// </summary>
        public string CaseStatusName { get; set; }

       /// <summary>
        /// 查詢期間起日
        /// </summary>
        public string QDateS { get; set; }

        /// <summary>
        /// 查詢期間訖日
        /// </summary>
        public string QDateE { get; set; }

        /// RFDM狀態原因
        /// </summary>
        public string RFDMQryMessage { get; set; }

         public string OpenDate { get; set; }
        public string LastDate { get; set; }

    }
}
