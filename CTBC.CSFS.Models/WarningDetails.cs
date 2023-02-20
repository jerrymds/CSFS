using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningDetails : Entity
    {
        public int SerialID { get; set; }

        public int SerialNo { get; set; }
        public string DocNo { get; set; }
        public string HappenDateTime { get; set; }
        public string HappenDateTimeForHour { get; set; }
        public string No_165 { get; set; }
        public string No_e { get; set; }
        public string NotificationContent { get; set; }
        public string NotificationSource { get; set; }
        public string ForCDate { get; set; }
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
        //Add by zhangwei 20180315 start
        public int maxnum { get; set; }
        /// <summary>
        /// 正本
        /// </summary>
        public string Original { get; set; }
        /// <summary>
        /// 案發地址
        /// </summary>
        public string DocAddress { get; set; }
        /// <summary>
        /// 電文ID
        /// </summary>
        public string EtabsTrnNum { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public string StateType { get; set; }
        //Add by zhangwei 20180315 end
        public string ReturnResult9091 { get; set; }//發查9091電文的回傳值 20180808 肖年華
        public string Currency { get; set; }// adam
                                            //public string CustAccount { get; set; }// adam

        public string StateCode { get; set; }// adam20190218

        public string Retry { get; set; }// adam20220329
        public int RowNum { get; set; } // adam20220329
        public string Extend { get; set; }
        public string ExtendDate { get; set; }
        public string ExtendDateHour { get; set; }

        public string Status { get; set; }
        public string UniteNo { get; set; }
        public string UniteDate { get; set; }
        public string Kind { get; set; }
        public string UniteNo_Old { get; set; }
        public string UniteDate_Old { get; set; }
        public string Flag_909113 { get; set; }
        public string Release { get; set; }
        public string ReleaseDate { get; set; }
        public string ReleaseDateForHour { get; set; }
        public string CustId_Old { get; set; }
        public string ExtendNo { get; set; }
        public string FIX { get; set; }
        public string FIXSEND { get; set; }

        public bool bool_909113 { get; set; }
        public bool bool_Retry { get; set; }
        public bool bool_Extend { get; set; }
        public bool bool_Release { get; set; }
        public bool bool_Fix { get; set; }
        public bool bool_Set { get; set; }
        public bool bool_FIXSEND { get; set; }

        public string Set { get; set; }
        public string SetDate { get; set; }
        public Guid NewId { get; set; }
        public Guid CaseId { get; set; }

    }
}
