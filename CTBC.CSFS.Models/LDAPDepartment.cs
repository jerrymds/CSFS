using System;

namespace CTBC.CSFS.Models
{
    public class LDAPDepartment
    {
        public string DepName { get; set; } 
        public string DepId { get; set; } 
        public string DepDn { get; set; } 
        public string CreatedUser { get; set; } 
        public DateTime CreatedDate { get; set; } 
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; } 
    }
}