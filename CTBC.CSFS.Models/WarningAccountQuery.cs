using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningAccountQuery : Entity
    {
        public string CustId { get; set; }
        public string CustName { get; set; }
        public string CustAccount { get; set; }
        public string AccountStatus { get; set; }
        public string BankID { get; set; }
        public string BankName { get; set; }
        public int IsRelease { get; set; }
        public int SerialID { get; set; }
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
        public int maxnum { get; set; }

        public int NoClosed { get; set; }
        //Add by zhangwei 20180315 start
        /// <summary>
        /// 類別
        /// </summary>
        public string StateType { get; set; }
        public string ItemType { get; set; }
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
        public string TRAN_Date { get; set; }
        public string TX9091 { get; set; }
        public string TX9092 { get; set; }
        /// <summary>
        /// 帳戶狀態中文解釋
        /// </summary>
        public string Other { get; set; }
        public string AccountStatusName { get; set; }
        /// <summary>
        /// 目前餘額
        /// </summary>
        public string HangAmount { get; set; }
        public string Amount { get; set; }
        public string CurBal { get; set; }
        public string Balance { get; set; }
        public string Balance450 { get; set; }
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
        public string HangAmountlist { get; set; }
        public string NotificationSourcelist { get; set; }
        public string AccountStatuslist { get; set; }
        public string Otherlist { get; set; }
        public string VDMD { get; set; }
        public string ACCT_NO { get; set; }
        public string ACCOUNT { get; set; }
        public string ACCT_DATE { get; set; }
        public string POST_DATE { get; set; }
    }
}
