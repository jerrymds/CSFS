using System;

namespace CTBC.CSFS.Models
{
    public class CaseMeetDetails
    {
        public int MeetDetailId { get; set; }
        public Guid CaseId { get; set; }
        public string MeetKind { get; set; }
        public string MeetUnit { get; set; }
        public int SortOrder { get; set; }
        public bool IsSelected { get; set; }
        public string Result { get; set; }
    }
}