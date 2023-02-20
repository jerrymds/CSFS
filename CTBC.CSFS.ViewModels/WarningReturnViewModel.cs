using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
     public class WarningReturnViewModel
    {
        // 介面顯示
        public WarningReturn WarningReturn { get; set; }

        // 清單顯示
        public IList<WarningReturn> WarningReturnList { get; set; }
    }
}
