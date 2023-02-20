using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class HistoryQueryViewModel
    {
        public HistoryQuery HistoryQuery { get; set; }
        public IList<HistoryQuery> HistoryQueryList { get; set; }
    }
}
