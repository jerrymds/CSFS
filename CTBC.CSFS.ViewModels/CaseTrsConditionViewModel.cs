using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CaseTrsConditionViewModel
    {
        public CaseTrsCondition CaseTrsCondition { get; set; }

        public List<CaseTrsQuery> CaseTrsQueryList { get; set; }
    }
}
