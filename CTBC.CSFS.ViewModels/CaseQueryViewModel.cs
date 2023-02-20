using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class CaseQueryViewModel
    {
        public CaseQuery CaseQuery { get; set; }
        public IList<CaseQuery> CaseQueryList { get; set; }
    }
}
