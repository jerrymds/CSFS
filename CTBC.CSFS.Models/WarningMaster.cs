using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class WarningMaster : Entity
    {
        public string DocNo { get; set; }
        public string CustId { get; set; }
        public string CustName { get; set; }
        public string CustAccount { get; set; }

        public string CustIdOld { get; set; }
        public string CustId_Old { get; set; }
        public string CustIdNew { get; set; }

        public string ForeignId { get; set; }

        public string ForeignId_Old { get; set; }
        public string AccountStatus { get; set; }
        public string BankID { get; set; }
        public string BankName { get; set; }
        public int IsRelease { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }

        public string ClosedDate { get; set; }
        public string Currency { get; set; }
        public string NotifyBal { get; set; }
        public string CurBal { get; set; }
        public string ReleaseBal { get; set; }
        public string VD { get; set; }
        public string MD { get; set; }

        public string BirthDay { get; set; }
        public string Tel { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }

        public Guid CaseId { get; set; }

    }
}
