using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public  class CaseCalculatorViewModel
    {
        public CaseCalculatorMain CaseCalculatorMain { get; set; }

        public List<CaseCalculatorMain> CaseCalculatorMainList { get; set; }

        public CaseCalculatorDetails CaseCalculatorDetails { get; set; }

        public List<CaseCalculatorDetails> CaseCalculatorDetailsList { get; set; }
    }
}
