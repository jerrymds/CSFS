using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningUnionQuery : Entity
    {
        public long No { get; set; }
        public string COL_ID { get; set; }
        public string CASE_NO { get; set; }
        public string COL_C1003CASE { get; set; }
        public string Milestone { get; set; }
        public string CaseCreator { get; set; }
        public string EmployeeId { get; set; }
        public string Unit { get; set; }
        public string C1003_BUSTYPE { get; set; }
        public string Case_Priority { get; set; }
        public string COL_PID { get; set; }
        public string COL_Name { get; set; }
        public string COL_2NDNOTICE { get; set; }
        public string COL_EVENTD { get; set; }
        public string COL_EVENTT { get; set; }
        public string COL_POLICE { get; set; }
        public string COL_EVENTP { get; set; }
        public string COL_VICTIM { get; set; }
        public string COL_SOURCE { get; set; }
        public string COL_OTHERBANKID { get; set; }
        public string COL_165CASE { get; set; }
        public string COL_CDM_C1003ACCTYPE2 { get; set; }
        public string COL_ACCOUNT2 { get; set; }
        public string COL_CCY2 { get; set; }
        public string COL_CSIBLOCKDATE2 { get; set; }
        public string COL_CSIBLOCKTIME2 { get; set; }
        public string COL_CSIBLOCKAMT2 { get; set; }
        public string COL_AGENTNAME { get; set; }
        public string COL_AGENTID { get; set; }
        public string COL_WITHATTACH { get; set; }
        public Guid  CaseId { get; set; }
        public string DocNo { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string Status { get; set; }

        public string Retry { get; set; }
        public int maxnum { get; set; }

        public int NoClosed { get; set; }
        //Add by zhangwei 20180315 start
        /// <summary>
        /// 類別
        /// </summary>
        public string StateType { get; set; }
        /// <summary>
        /// 解除日期起
        /// </summary>
        public string RelieveDateS { get; set; }
        /// <summary>
        /// 解除日期訖
        /// </summary>
        public string RelieveDateE { get; set; }

        /// <summary>
        /// 修改日期起
        /// </summary>
        public string ModifyDateS { get; set; }
        /// <summary>
        /// 修改日期訖
        /// </summary>
        public string ModifyDateE { get; set; }
        /// <summary>
        /// 正本
        /// </summary>
        public string Original { get; set; }
        /// <summary>
        /// 解除日期
        /// </summary>
        public string RelieveDate { get; set; }

        public string TX9091 { get; set; }
        public string TX9092 { get; set; }
        /// <summary>
        /// 帳戶狀態中文解釋
        /// </summary>
        public string AccountStatusName { get; set; }
        /// <summary>
        /// 目前餘額
        /// </summary>
        public string CurBal { get; set; }
        /// <summary>
        /// 通報餘額
        /// </summary>
        public string NotifyBal { get; set; }
        /// <summary>
        /// 解除餘額
        /// </summary>
        public string ReleaseBal { get; set; }
        //Add by zhangwei 20180315 end
        public string currency { get; set; }
        public string DocAddress { get; set; }
        //Add by adam 20181214 end
        public string NoClosedName { get; set; }

        public string CloseReason { get; set; }//* 退件原因
        /// <summary>
        /// 發文方式
        /// </summary>
        public string SendKind { get; set; }
        /// <summary>
        /// 放行主管
        /// </summary>
        public string ApproveManager { get; set; }
        /// <summary>
        /// 放行日期起
        /// </summary>
        public string ApproveDateS { get; set; }
        /// <summary>
        /// 放行日期訖
        /// </summary>
        public string ApproveDateE { get; set; }
        /// <summary>
        /// 客戶ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 電子發文上傳日
        /// </summary>
        public string SendUpDate { get; set; }
        public string ReceiveDate { get; set; }

        public string MailNo { get; set; }
        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        public string OverDueMemo { get; set; }
        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
        /// <summary>
        /// 案件狀態
        /// </summary>
        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
        public string AgentDepartment { get; set; }
        public string AgentDepartment2 { get; set; }
        public string AgentDepartmentUser { get; set; }
        public string IsBranchDirector { get; set; }
        public string Kind { get; set; }
        public string Extend { get; set; }
        public string UniteNo { get; set; }
        public string UniteDate { get; set; }
        public string UniteNo_Old { get; set; }
        public string UniteDate_Old { get; set; }
        public string Flag_909113 { get; set; }
        public string Release { get; set; }
        public string ReleaseDate { get; set; }
        public string CustId_Old { get; set; }

        public string Set { get; set; }
        public string SetDate { get; set; }
    }
}
