using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class HistoryAgentOriginalInfoViewModel
    {
        public HistoryLendData LendDataInfo { get; set; }

        public IList<HistoryLendData> LendDataInfoList { get; set; }

        public HistoryLendAttachment LendAttachmentInfo { get; set; }

        public List<HistoryLendAttachment> LendAttachmentInfoList { get; set; }

        public HistoryCaseObligor CaseObligorInfo { get; set; }

        public HistoryCaseMaster CaseMaster { get; set; }
    }
}
