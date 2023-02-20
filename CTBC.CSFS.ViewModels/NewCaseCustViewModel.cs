using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class NewCaseCustViewModel
    {
        public CaseCustMaster CaseCustMaster { get; set; }

        public IList<CaseCustMaster> NewCaseCustQueryList { get; set; }

        public NewCaseCustCondition NewCaseCustCondition { get; set; }

        public int DataCount { get; set; } // 案件總筆數
    }
}
