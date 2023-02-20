using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseMasterViewModel
    {
        public CaseMaster CaseMaster { get; set; }

        public IList<CaseMaster> CaseMasterList { get; set; }


        public int DataCount { get; set; } // 案件總筆數
    }
}
