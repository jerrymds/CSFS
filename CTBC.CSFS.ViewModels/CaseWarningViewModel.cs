using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public  class CaseWarningViewModel
    {
        public WarningMaster WarningMaster { get; set; }

        public IList<WarningMaster> WarningMasterList { get; set; }

        public WarningDetails WarningDetails { get; set; }

        public List<WarningDetails> WarningDetailsList { get; set; }
        public List<WarningGenAcct> WarningGenAcctList { get; set; }
        public WarningState WarningState { get; set; }

        public List<WarningState> WarningStateList { get; set; }

        public WarningAttachment WarningAttachment { get; set; }

        public WarningQuery WarningQuery { get; set; }

        public List<WarningAttachment> WarningAttachmentList { get; set; }
        public IEnumerable <CaseCustRFDMRecv> CaseCustRFDMRecv { get; set; }
        public List<CaseCustRFDMRecv> CaseCustRFDMRecvList { get; set; }

        public List<WarningQueryHistory> WarningHistoryList { get; set; }
    }
}
