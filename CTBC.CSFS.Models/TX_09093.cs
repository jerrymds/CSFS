using System;

namespace CTBC.CSFS.Models
{
    public class TX_09093
    {
        public int SNO { get; set; }
        public string Account { get; set; }
        public string RESPONSE_OC08 { get; set; }
        public string CURRENCY_OC08 { get; set; }
        public string CMTRASH_OC08 { get; set; }
        public string DESCRIP_OC08 { get; set; }
        public string TrnNum { get; set; }
        public string RepMessage { get; set; }
        public string RAMOUNT_OC57 { get; set; }
        public string ACCT_NO_OC57 { get; set; }
        public string ACCNAME_OC57 { get; set; }
        public string RDATE_OC57 { get; set; }
        public string outputCode { get; set; }
        public DateTime? cCretDT { get; set; }
        public Guid CaseId { get; set; } 
    }
}