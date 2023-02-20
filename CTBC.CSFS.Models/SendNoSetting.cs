using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class SendNoSetting : Entity
    {
        public int SendNoId { get; set; }
        public string SendNoYear { get; set; }
        public Int64 SendNoStart { get; set; }
        public Int64 SendNoEnd { get; set; }
        public Int64 SendNoNow { get; set; }
        public int maxnum { get; set; }
    }
}
