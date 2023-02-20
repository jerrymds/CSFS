using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseCustQuery : Entity
    {
        public string Repeat { get; set; }//	是否重複
        public string DocNo { get; set; }//	案件編號
        public string CustIdNo { get; set; }//	身分證統一編號
        public string OpenFlag { get; set; }//	開戶個人資料標記
        public string TransactionFlag { get; set; }//	特定期間之交易明細往來（限新臺幣）標記
        public string QDateS { get; set; }//	查詢期間起日
        public string QDateE { get; set; }//	查詢期間訖日
        public string RecvDate { get; set; }//	資料接收日期
        public string HTGSendStatus { get; set; }//	HTG查詢狀態
        public string HTGQryMessage { get; set; }//	HTG狀態原因
        public string HTGModifiedDate { get; set; }//	HTG發查時間
        public string RFDMSendStatus { get; set; }//	RFDM查詢狀態
        public string RFDMQryMessage { get; set; }//	RFDM狀態原因
        public string RFDModifiedDate { get; set; }//	RFDM發查時間
        public string QFileName { get; set; }//來文文檔名稱
        public string QFileName2 { get; set; }//來文文檔名稱

        public string ROpenFileName { get; set; }//回文檔名稱（存款帳戶開戶資料）
        public string RFileTransactionFileName { get; set; }//回文檔名稱（存款往來明細資料）

        public string PDFFileName { get; set; }//回文檔名稱（存款帳戶開戶資料）


        public string FileNo { get; set; }//來文字號
        public string Govement { get; set; }//來文機關 
        public string LimitDate { get; set; }//	限辦日期(T+7)
        public string RecvDate5 { get; set; }//	(T+5)
        public string SearchProgram { get; set; }//查詢項目 
        public string CaseStatus { get; set; }//案件狀態 
        public string StatusReason { get; set; }//狀態原因
        public string GoFileNo { get; set; } // 回文字號
        public string Result { get; set; } // 拋查結果 
        public string FinishDate { get; set; }//結案日期
        public string RowNum { get; set; }

        public string CaseStatusName { get; set; }

        public string UploadStatus { get; set; } // 上傳狀態
        public int Version { get; set; } // 版本號
        public string AuditStatus { get; set; } // 上傳狀態
        public Guid VersionKey { get; set; }
        public Guid NewID { get; set; }
        public string CountDocNo { get; set; }// 資料筆數
        public string ShowDocNo { get; set; }// 顯示在頁面上的案件編號


        public string IsEnable { get; set; }

        public string ImportFormFlag { get; set; }
    }

    // 查詢條件
    public class CaseCustCondition : Entity
    {
        // 類別 1 
        public string TypeFirst { get; set; }

        // 類別 2 
        public string TypeSecond { get; set; }

        // 發文方式   
        public string Method { get; set; }

        // 來文機關 
        public string FileGovenment { get; set; }

        // 案件編號
        public string DocNo { get; set; }

        // 來文日期S 
        public string FileDateStart { get; set; }

        // 來文日期E
        public string FileDateEnd { get; set; }

        // 查詢項目 
        public string SearchProgram { get; set; }

        // 來文字號   
        public string FileNo { get; set; }

        // 拋查結果 
        public string Result { get; set; }

        // 建檔日期 S
        public string DateStart { get; set; }

        // 建檔日期 E
        public string DateEnd { get; set; }

        // 審核狀態 
        public string Status { get; set; }

        public string PageFrom { get; set; }

        // 勾選的資料
        public string CheckedData { get; set; }
        public string CheckedDatas { get; set; }
        // 結案日期 S
        public string FinishDateStart { get; set; }

        // 結案日期 E
        public string FinishDateEnd { get; set; }

        // 案件狀態 
        public string CaseStatus { get; set; }

        // 處理方式   
        public string ProcessingMethod { get; set; }

        // 統一編號   
        public string CustIdNo { get; set; }

        // 回文字號
        public string GoFileNo { get; set; }

        // 來文文檔名稱txt
        public string QFileName { get; set; }

        /// <summary>
        /// 來文文檔名稱PDF
        /// </summary>
        public string QFileName2 { get; set; }

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

        public Guid NewID { get; set; }

        // 頁面來源 1:主管放行;2:歷史記錄查詢與重送
        public string PageSource { get; set; }
        public string AuditStatus { get; set; }

        public string IsEnable { get; set; }
    }

    public class CheckedData_P
    {
        public string[] DocNo { get; set; }
        public string[] DocNo1 { get; set; }
    }

    public class ApprMsgKeyVO : Entity
    {
        public string MsgKeyLU { get; set; }
        public string MsgKeyLP { get; set; }
        public string MsgKeyRU { get; set; }
        public string MsgKeyRP { get; set; }
        public string MsgKeyRB { get; set; }
        public string MsgUID { get; set; }
        public Guid VersionNewID { get; set; }
    }

    public class CaseCustQueryVersion : Entity
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public Guid NewID { get; set; }

        public Guid IdNo { get; set; }

        /// <summary>
        /// 來文檔案查詢條件檔主鍵
        /// </summary>
        public Guid CaseCustNewID { get; set; }

        /// <summary>
        /// 身分證統一編號
        /// </summary>
        public string CustIdNo { get; set; }

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

    }
}
