using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseTrsViewModel
    {
        public CaseTrsQueryVersion CaseTrsQueryVersion { get; set; }

        public IList<CaseTrsQueryVersion> CaseTrsQueryVersionList { get; set; }

        public CaseTrsCondition CaseTrsCondition { get; set; }
        public List<CaseTrsCondition> CaseTrsConditionList { get; set; }
        public CaseHisCondition CaseHisCondition { get; set; }

        public List<CaseHisCondition> CaseHisConditionList { get; set; }
    }
}
