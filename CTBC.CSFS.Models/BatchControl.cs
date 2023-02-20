using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class BatchControl
    {
        public string SerialID { get; set; }
        public Guid CaseId { get; set; }
        public string EXCEPTION { get; set; }
        public string STATUS_Create { get; set; }
        public string STATUS_Transfer { get; set; }
        public string CREATE_TIME { get; set; }
        public string UPDATE_TIME { get; set; }
        public string SendNo { get; set; }
    }
}
