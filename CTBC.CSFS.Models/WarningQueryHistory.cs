using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public  class WarningQueryHistory : Entity
    {
        public string NewID { get; set; }
        public string DocNo { get; set; }
        public string CustAccount { get; set; }
        public string ForCDateS { get; set; }
        public string ForCDateE { get; set; }
        public string SendDate { get; set; }
        public string RecvDate { get; set; }
        public string QFileName { get; set; }
        public string QFileName2 { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }

        public string CustId { get; set; }
        public string Status { get; set; }
        public string TrnNum { get; set; }
        public string ESBStatus { get; set; }
        public string FileName { get; set; }
        public string AcctDesc { get; set; }
        public string Curr { get; set; }
        public string Channel { get; set; }

    }
}
