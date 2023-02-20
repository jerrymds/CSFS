using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningState : Entity
    {
        public string DocNo { get; set; }
        public string NotificationSource { get; set; }
        public string RelieveDate { get; set; }
        public string RelieveDateTimeForHour { get; set; }
        public string RelieveReason { get; set; }
        public string OtherReason { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }

        public int maxnum { get; set; }
        //Add by zhangwei 20180315 start
        /// <summary>
        /// 外來文編號
        /// </summary>
        public string EtabsNo { get; set; }
        /// <summary>
        /// 電文編號
        /// </summary>
        public string EtabsTrnNum { get; set; }
        //Add by zhangwei 20180315 end
        public string StateCode { get; set; }// adam20190218
        public string Flag_Release { get; set; }

        public string Status { get; set; }

        public string Kind { get; set; }

        public string No_165 { get; set; }

        public bool bool_Release { get; set; }
        public string text_Release { get; set; }
    }
}
