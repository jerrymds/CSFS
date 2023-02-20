using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseAttachment : Entity
    {
        public int AttachmentId { get; set; }
        public Guid CaseId { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentServerPath { get; set; }
        public string AttachmentServerName { get; set; }
        public int isDelete { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
