using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseHistory : Entity
    {
        public int HistoryId { get; set; }
        public Guid CaseId { get; set; }
        public string FromRole { get; set; }
        public string FromUser { get; set; }
        public string FromFolder { get; set; }
        public string Event { get; set; }
        public DateTime EventTime { get; set; }
        public string ToRole { get; set; }
        public string ToUser { get; set; }
        public string ToFolder { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
