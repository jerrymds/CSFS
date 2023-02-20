using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class AgentOriginalInfoViewModel
    {
        public LendData LendDataInfo { get; set; }

        public IList<LendData> LendDataInfoList { get; set; }

        public LendAttachment LendAttachmentInfo { get; set; }

        public List<LendAttachment> LendAttachmentInfoList { get; set; }

        public CaseObligor CaseObligorInfo { get; set; }

        public CaseMaster CaseMaster { get; set; }
    }
}
