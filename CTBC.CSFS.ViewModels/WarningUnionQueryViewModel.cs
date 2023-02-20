using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
     public class WarningUnionQueryViewModel
    {
        // 介面顯示
        public WarningUnionQuery WarningUnionQuery { get; set; }

        // 清單顯示
        public IList<WarningUnionQuery> WarningUnionQueryList { get; set; }
    }
}
