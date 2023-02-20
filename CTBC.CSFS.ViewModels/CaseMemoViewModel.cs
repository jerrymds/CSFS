using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseMemoViewModel
    {
        // 介面顯示
        public CaseMemo CaseMemo { get; set; }

        // 清單顯示
        public IList<CaseMemo> CaseMemoList { get; set; }
    }
}
