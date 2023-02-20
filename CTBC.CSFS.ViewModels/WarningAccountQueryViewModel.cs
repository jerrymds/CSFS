using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class WarningAccountQueryViewModel
    {
        // 介面顯示
        public WarningAccountQuery WarningAccountQuery { get; set; }

        // 清單顯示
        public IList<WarningAccountQuery> WarningAccountQueryList { get; set; }
    }
}
