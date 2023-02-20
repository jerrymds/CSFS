using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseCustRFDMSend : Entity
    {
        public string TrnNum { get; set; }
        public System.Guid VersionNewID { get; set; }
        public string ID_No { get; set; }
        public string Acct_No { get; set; }
        public Nullable<System.DateTime> Start_Jnrst_Date { get; set; }
        public Nullable<System.DateTime> End_Jnrst_Date { get; set; }
        public string Type { get; set; }
        public string RFDMSendStatus { get; set; }
        public string RspCode { get; set; }
        public string RspMsg { get; set; }
        public string FileName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string AcctDesc { get; set; }
        public string Curr { get; set; }
        public string Channel { get; set; } 
    }
}
