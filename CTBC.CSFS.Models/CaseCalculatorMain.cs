using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseCalculatorMain : Entity
    {
        public int CalcMId { get; set; }
        public Guid CaseId { get; set; }
        public Decimal Amount1 { get; set; }
        public Decimal Amount2 { get; set; }
        public Decimal Amount3 { get; set; }
        public Decimal Amount4 { get; set; }
        public Decimal Amount5 { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
