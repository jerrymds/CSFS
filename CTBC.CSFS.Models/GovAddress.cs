using System;

namespace CTBC.CSFS.Models
{
    public class GovAddress:Entity
    {
        public int GovAddrId { get; set; }
        public string GovKind { get; set; }
        public string GovName { get; set; }
        public string GovAddr { get; set; }
        public bool IsEnabled { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int maxnum { get; set; }
    }
}