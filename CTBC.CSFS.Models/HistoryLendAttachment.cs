using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryLendAttachment : Entity
    {
        public int LendAttachId { get; set; }
        public int LendId { get; set; }
        public Guid CaseId { get; set; }
        public string LendAttachName { get; set; }
        public string LendAttachServerPath { get; set; }
        public string LendAttachServerName { get; set; }
        public int isDelete { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }

    }
}
