using System;
using System.Collections.Generic;

namespace CTBC.CSFS.Models
{
    public class CaseMeetMaster
    {
        public int MeetId { get; set; }
        public Guid CaseId { get; set; }
        public string StandardDateS { get; set; }
        public string StandardDateE { get; set; }
        public string BranchPaySave { get; set; }
        public bool BranchVip { get; set; }
        public string BranchViptext { get; set; }
        public string RmNotice { get; set; }
        public string RmNoticeAndConfirm { get; set; }
        public string MeetMemo { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }

        public List<CaseMeetDetails> ListDetails { get; set; } 
    }
}