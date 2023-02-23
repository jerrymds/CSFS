using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningFraud : Entity
    {
        /// <summary>
        /// PK
        /// </summary>
        public long No { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string COL_ID { get; set; }
        /// <summary>
        /// 聯徵案號
        /// </summary>
        public string CASE_NO { get; set; }
        /// <summary>
        /// 工單編號
        /// </summary>
        public string COL_C1003CASE { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public string Milestone { get; set; }
        /// <summary>
        /// 發單人
        /// </summary>
        public string CaseCreator { get; set; }
        /// <summary>
        /// 發單員編
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// 通報單位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 發單類別 EX:警示通報/聯防/165預警
        /// </summary>
        public string C1003_BUSTYPE { get; set; }
        /// <summary>
        /// 案件時效 EX:一般件/急件/抱怨件
        /// </summary>
        public string Case_Priority { get; set; }
        /// <summary>
        /// 身分證字號
        /// </summary>
        public string COL_PID { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string COL_Name { get; set; }
        /// <summary>
        /// 二層通報
        /// </summary>
        public string COL_2NDNOTICE { get; set; }
        /// <summary>
        /// 案發日期
        /// </summary>
        public string COL_EVENTD { get; set; }
        /// <summary>
        /// 案發時間
        /// </summary>
        public string COL_EVENTT { get; set; }
        /// <summary>
        /// 通報警局
        /// </summary>
        public string COL_POLICE { get; set; }
        /// <summary>
        /// 案發地點
        /// </summary>
        public string COL_EVENTP { get; set; }
        /// <summary>
        /// 被害人
        /// </summary>
        public string COL_VICTIM { get; set; }
        /// <summary>
        /// 通報來源 EX:他行/165
        /// </summary>
        public string COL_SOURCE { get; set; }
        /// <summary>
        /// 銀行別名稱
        /// </summary>
        public string COL_OTHERBANKID { get; set; }
        /// <summary>
        /// 165案號
        /// </summary>
        public string COL_165CASE { get; set; }
        /// <summary>
        /// 修改模式儲存修改前的COL_165CASE
        /// </summary>
        public string COL_165CASE_OLD { get; set; }
        /// <summary>
        /// 聯防_帳號類型
        /// </summary>
        public string COL_CDM_C1003ACCTYPE2 { get; set; }
        /// <summary>
        /// 聯防_通報帳號
        /// </summary>
        public string COL_ACCOUNT2 { get; set; }
        /// <summary>
        /// 聯防_幣別
        /// </summary>
        public string COL_CCY2 { get; set; }
        /// <summary>
        /// 聯防_ CSI設定事故/圈存日期
        /// </summary>
        public string COL_CSIBLOCKDATE2 { get; set; }
        /// <summary>
        /// 聯防_ CSI設定事故/圈存時間
        /// </summary>
        public string COL_CSIBLOCKTIME2 { get; set; }
        /// <summary>
        /// 聯防_止扣/圈存金額
        /// </summary>
        public string COL_CSIBLOCKAMT2 { get; set; }
        /// <summary>
        /// 客服覆審人員
        /// </summary>
        public string COL_AGENTNAME { get; set; }
        /// <summary>
        /// 覆審人員員編
        /// </summary>
        public string COL_AGENTID { get; set; }
        /// <summary>
        /// 開單時是否有附件
        /// </summary>
        public string COL_WITHATTACH { get; set; }
        public Guid CaseId { get; set; }
        public string DocNo { get; set; }
        /// <summary>
        /// 分機
        /// </summary>
        public string EXT { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 鍵檔日期
        /// </summary>
        public string CreatedDate { get; set; }
        /// <summary>
        /// 鍵檔人員
        /// </summary>
        public string CreatedUser { get; set; }
        /// <summary>
        /// 修改日期
        /// </summary>
        public string ModifiedDate { get; set; }
        /// <summary>
        /// 修改人員
        /// </summary>
        public string ModifiedUser { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 鍵檔日期起
        /// </summary>
        public string CreateDateS { get; set; }
        /// <summary>
        /// 鍵檔日期迄
        /// </summary>
        public string CreateDateE { get; set; }
        /// <summary>
        /// 資料序號 ROW_NUMBER()
        /// </summary>
        public int RowNum { get; set; }
        /// <summary>
        /// 附件 PK
        /// </summary>
        public int AttachmentId { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public WarningFraudAttach WarningFraudAttach { get; set; }
    }
}
