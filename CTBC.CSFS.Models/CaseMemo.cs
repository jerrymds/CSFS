using System;
using System.Collections.Generic;

namespace CTBC.CSFS.Models
{
    public class CaseMemo : Entity
    {
        public int MemoId { get; set; }
        public Guid CaseId { get; set; }
        public string MemoType { get; set; }
        public string Memo { get; set; }
        public string MemoDate { get; set; }
        public string MemoUser { get; set; }
        public int maxnum { get; set; }

    }
}