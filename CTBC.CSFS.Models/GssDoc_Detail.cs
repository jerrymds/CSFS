using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
   public class GssDoc_Detail
    {
        public int id { get; set; }
        public string BatchNo { get; set; }
        public string DocNo { get; set; }
        public string Metadata { get; set; }
        public string FileNames { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ParserStatus { get; set; }
        public string ParserMessage { get; set; }
        public string CaseKind { get; set; }
        public Guid CaseId { get; set; }
        public string SendBatchNo { get; set; }
        public DateTime SendDate { get; set; }
        public int SendStatus { get; set; }
        public string SendMessage { get; set; }
        public string Sendmetadata { get; set; }
    }
}
