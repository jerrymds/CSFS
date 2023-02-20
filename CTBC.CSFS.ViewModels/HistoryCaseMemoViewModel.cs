using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class HistoryCaseMemoViewModel
    {
        // 介面顯示
        public HistoryCaseMemo HistoryCaseMemo { get; set; }

        // 清單顯示
        public IList<HistoryCaseMemo> HistoryCaseMemoList { get; set; }
    }
}
