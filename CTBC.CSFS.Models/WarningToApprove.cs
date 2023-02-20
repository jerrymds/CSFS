using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningToApprove : Entity
    {
        public Guid CaseId { get; set; }

        public Guid NewId { get; set; }
        public string CustId { get; set; }
        public string CustName { get; set; }
        public string CustAccount { get; set; }
        public string AccountStatus { get; set; }
        public string BankID { get; set; }
        public string BankName { get; set; }
        public int IsRelease { get; set; }
        public int SerialID { get; set; }
        public int SerialNo { get; set; }
        public int RowNum { get; set; }
        public string DocNo { get; set; }
        public string HappenDateTime { get; set; }
        public string HappenDateTimeForHour { get; set; }
        public string No_165 { get; set; }
        public string No_e { get; set; }
        public string NotificationContent { get; set; }
        public string NotificationSource { get; set; }
        public string ForCDate { get; set; }
        public string ForCDateS { get; set; }
        public string ForCDateE { get; set; }
        public string EtabsDatetime { get; set; }
        public string EtabsDatetimeHour { get; set; }
        public string NotificationUnit { get; set; }
        public string NotificationName { get; set; }
        public string ExtPhone { get; set; }
        public string PoliceStation { get; set; }
        public string VictimName { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }

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
        public string Status { get; set; }
        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
        public string AgentDepartment { get; set; }
        public string AgentDepartment2 { get; set; }
        public string AgentDepartmentUser { get; set; }
        public string IsBranchDirector { get; set; }

        public string Kind { get; set; }
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
