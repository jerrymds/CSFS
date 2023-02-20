using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.WarningReturnFile.Model
{
    public class wm
    {
        public string   DocNo{get;set;}
        public string   CustId{get;set;}
        public string   CustName{get;set;}
        public string   CustAccount{get;set;}
        public string   AccountStatus{get;set;}
        public string   BankID{get;set;}
        public string   BankName{get;set;}
        public int   IsRelease{get;set;}
        public string   CreatedUser{get;set;}
        public DateTime   CreatedDate{get;set;}
        public string   ModifiedUser{get;set;}
        public DateTime ModifiedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public string   Currency{get;set;}
        public string   NotifyBal{get;set;}
        public string   CurBal{get;set;}
        public string   ReleaseBal{get;set;}
        public string   VD{get;set;}
        public string   MD{get;set;}
        public string   VDMD{get;set;}
        public string   Status{get;set;}
        public string   ForeignId{get;set;}
        public string   CustId_Old{get;set;}
        public Guid? CaseId { get; set; }
    }
}
