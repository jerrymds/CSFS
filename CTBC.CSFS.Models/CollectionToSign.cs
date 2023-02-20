using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CollectionToSign : Entity
    {
        public Guid CaseId { get; set; }
        public string GovKind { get; set; }
        public string GovUnit { get; set; }
        public string GovDate { get; set; }
        public string Speed { get; set; }
        public string ReceiveKind { get; set; }
        public string GovNo { get; set; }
        public string CaseKind { get; set; }
        public string CaseKind2 { get; set; }
        public string Unit { get; set; }
        public string Person { get; set; }
        public string ReceiverNo { get; set; }
        public string CaseNo { get; set; }
        public string LimitDate { get; set; }
        public string Status { get; set; }
        public int maxnum { get; set; }
        public string GovDateS { get; set; }
        public string GovDateE { get; set; }
        public string CreatedDateS { get; set; }
        public string CreatedDateE { get; set; }
        public string AgentUser { get; set; }
        public string StatusShow { get; set; }
        public string ReturnReason { get; set; }
        public string Department { get; set; }
        public string ObligorNo { get; set; }
    }
}
