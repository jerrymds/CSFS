using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseTrsDetailViewModel
    {
        public CaseTrsQueryVersion CaseTrsQueryVersion { get; set; }

        public IList<CaseTrsQueryVersion> CaseTrsQueryVersionList { get; set; }

        public CaseMaster CaseMaster { get; set; }
        public List<CaseMaster> CaseMasterList { get; set; }
        public CaseHisCondition CaseHisCondition { get; set; }

        public List<CaseHisCondition> CaseHisConditionList { get; set; }
    }
}
