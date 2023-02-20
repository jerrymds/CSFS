using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
     public class WarningToApproveViewModel
    {
        // 介面顯示
        public WarningToApprove WarningToApprove { get; set; }

        // 清單顯示
        public IList<WarningToApprove> WarningToApproveList { get; set; }
    }
}
