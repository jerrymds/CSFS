using System;

namespace CTBC.CSFS.Models
{
    public class CaseAssignTable
    {
        public int AssignId { get; set; }
        public Guid CaseId { get; set; }
        public string EmpId { get; set; }
        public int AlreadyAssign { get; set; }
        public string CreatdUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}