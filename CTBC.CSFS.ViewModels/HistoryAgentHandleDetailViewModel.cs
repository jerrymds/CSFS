using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class HistoryAgentHandleDetailViewModel
    {
        // 介面顯示
        public HistoryAgentHandleDetail AgentHandleDetail { get; set; }

        // 清單顯示
        public IList<HistoryAgentHandleDetail> AgentHandleDetailList { get; set; }
    }
}
