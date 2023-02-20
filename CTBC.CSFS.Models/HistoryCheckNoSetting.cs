using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryCheckNoSetting : Entity
    {
        //public int CheckNoId { get; set; }
        //public string CheckNoYear { get; set; }
        //public string CheckNoPerfix { get; set; }

        //public Int64 CheckNoNow { get; set; }
        public int maxnum { get; set; }

        /// <summary>
        /// 支票本編號
        /// </summary>
        public Int64 CheckIntervalID { get; set; }

        /// <summary>
        /// 開始號碼
        /// </summary>
        public Int64 CheckNoStart { get; set; }

        /// <summary>
        /// 結束號碼
        /// </summary>
        public Int64 CheckNoEnd { get; set; }

        /// <summary>
        /// 每週二支票預留數量
        /// </summary>
        public int WeekTempAmount { get; set; }

        /// <summary>
        /// 使用狀態(未使用,已使用)
        /// </summary>
        public string UseStatus { get; set; }

        /// <summary>
        /// 使用種類(支付,作廢,其它)
        /// </summary>
        public string Kind { get; set; }

        public int IsUsed { get; set; }

        public Int64 CheckNo { get; set; }

        public Int64 CheckNoS { get; set; }
        public Int64 CheckNoE { get; set; }


        public int IsPreserve { get; set; }
    }
}
