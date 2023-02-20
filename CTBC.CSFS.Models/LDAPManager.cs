using System;

namespace CTBC.CSFS.Models
{
    public class LDAPManager
    {
        public string EmpId { get; set; } 
        public string DepId { get; set; } 
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; } 
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; } 
    }
}