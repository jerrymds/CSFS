using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseDeadQuery  : Entity
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
    public class CaseDeadCondition  : Entity
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
        public List<CaseDeadCondition > CaseDeadConditionList { get; set; }
        public List<CaseDeadQuery > CaseDeadQueryList { get; set; }

    }

    public class CaseRecordCondition : Entity
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
        public string HeirId { get; set; }
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


    public class CaseDeadVersion : Entity
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public Guid NewID { get; set; }
        public Guid CaseTrsNewID { get; set; }
        public string DocNo { get; set; }
        public string RowNum { get; set; }
        public int Seq { get; set; }
        public string chkbox { get; set; }
        public string QueryUnit    { get; set; }
        public string CityNo       { get; set; }
        public string BrigeNo      { get; set; }
        public string AppDate      { get; set; }
        public string SNo          { get; set; }
        public string HeirId       { get; set; }  
        public string HeirName     { get; set; }
        public string HeirBirthDay { get; set; }
        public string HeirDeadDate { get; set; }
        public string AppId        { get; set; }
        public string AppName      { get; set; }
        public string AppTel       { get; set; }  
        public string Relation     { get; set; }
        public string AgentId      { get; set; }
        public string AgentName    { get; set; }
        public string AgentTel     { get; set; }
        public string SendCity     { get; set; }
        public string SendTown     { get; set; }
        public string SendLe { get; set; }
        public string SendLin      { get; set; }
        public string SendStreet   { get; set; }
        public string MergeQU      { get; set; }
        public string deposit      { get; set; }
        public string Loan         { get; set; }
        public string LoanMgr      { get; set; }  
        public string Box          { get; set; }
        public string CashCard     { get; set; }
        public string CreditCard   { get; set; }
        public string InvestMgr    { get; set; }
        public DateTime CreatedDate  { get; set; }
        public string CreatedUser  { get; set; }  
        public DateTime ModifiedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string IdNo         { get; set; }
        public string EXCEL_SETUP  { get; set; }
        public string EXCEL_RESULT { get; set; }  
        public string PDF_FILE     { get; set; }
        public string Currency     { get; set; }
        public string Status       { get; set; }
        public string StatusName { get; set; }
        public string StatusName1 { get; set; }
        public string StatusName2 { get; set; }
        public string SetStatus    { get; set; }
        public string SetMessage   { get; set; }
        public string SendStatus   { get; set; }  
        public string SendMessage  { get; set; }

    }

  
}
