using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningFraudAttach
    {
        public int AttachmentId { get; set; }
        public int WarningFraudNo { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentServerPath { get; set; }
        public string AttachmentServerName { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
    }
}
