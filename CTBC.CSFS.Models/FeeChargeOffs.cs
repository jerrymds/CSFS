using System;

namespace CTBC.CSFS.Models
{
    public class FeeChargeOffs
    {
        public int FeeId { get; set; }
        public Guid CaseId { get; set; }
        public int PayeeId { get; set; }
        public Decimal HangingAmount { get; set; }
        public DateTime? HangingDate { get; set; }
        public Decimal ChargeOffsAmount { get; set; }
        public DateTime? ChargeOffsDate { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Memo { get; set; }
    }
}