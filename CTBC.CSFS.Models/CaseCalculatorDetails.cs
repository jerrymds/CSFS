using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseCalculatorDetails : Entity
    {
        public int CalcDId { get; set; }
        public Guid CaseId { get; set; }
        public Decimal Amount { get; set; }
        public string InterestRateType { get; set; }
        public Decimal InterestRate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int InterestDays { get; set; }
        public Decimal Interest { get; set; }
        public int InterestReal { get; set; }

        public int InterestTotal { get; set; }

        public int CaseTotal { get; set; }//扣押總計
    }
}
