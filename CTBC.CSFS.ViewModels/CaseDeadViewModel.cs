using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseDeadViewModel
    {
        public CaseDeadVersion CaseDeadVersion { get; set; }

        public IList<CaseDeadVersion> CaseDeadVersionList { get; set; }

        public CaseDeadCondition CaseDeadCondition { get; set; }
        public List<CaseDeadCondition> CaseDeadConditionList { get; set; }
        public CaseRecordCondition CaseRecordCondition { get; set; }

        public List<CaseRecordCondition> CaseRecordConditionList { get; set; }
    }
}
