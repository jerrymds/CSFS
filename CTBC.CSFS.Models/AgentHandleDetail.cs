using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class AgentHandleDetail : Entity
    {
        public Guid CaseId { get; set; }
        public string CaseNo { get; set; }
        public string FromRole { get; set; }
        public string FromUser { get; set; }
        public string FromFolder { get; set; }
        public string EventTime { get; set; }
        public string Event { get; set; }
        public string ToRole { get; set; }
        public string ToUser { get; set; }
        public string ToFolder { get; set; }
        public int maxnum { get; set; }

        public string Memo { get; set; }

        //待改
        public int HistoryId { get; set; }
    }
}
