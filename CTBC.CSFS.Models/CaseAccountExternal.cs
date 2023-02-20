using System;
namespace CTBC.CSFS.Models
{
    public class CaseAccountExternal
    {
        public Guid CaseId { get; set; }
        public string FirstCate { get; set; }
        public string SecondCate { get; set; }
        public string Description { get; set; }
        public int UnitPrice { get; set; }
        public int SortOrder { get; set; }
        public int Quantity { get; set; }
        public int Amount { get; set; }

    }
}