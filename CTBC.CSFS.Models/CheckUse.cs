using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CheckUse
    {
        public Int64 CheckIntervalId { get; set; }
        public Int64 CheckNo { get; set; }
        public string Kind { get; set; }
        public bool IsUsed { get; set; }
        public bool IsPreserve { get; set; }

    }
}
