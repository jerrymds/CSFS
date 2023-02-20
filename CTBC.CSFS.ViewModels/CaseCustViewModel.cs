using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseCustViewModel
    {
        public CaseCustQuery CaseCustQuery { get; set; }

        public IList<CaseCustQuery> CaseCustQueryList { get; set; }

        public CaseCustCondition CaseCustCondition { get; set; }

        public int DataCount { get; set; } // 案件總筆數
    }
}
