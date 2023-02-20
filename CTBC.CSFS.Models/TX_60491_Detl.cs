using System;
namespace CTBC.CSFS.Models
{
    public class TX_60491_Detl
    {
        public int SNO { get; set; }
        public int FKSNO { get; set; }
        public string Account { get; set; }
        public string Branch { get; set; }
        public string BranchName { get; set; }
        public string StsDesc { get; set; }
        public string ProdCode { get; set; }
        public string ProdDesc { get; set; }
        public string Link { get; set; }
        public string Ccy { get; set; }
        public string Bal { get; set; }
        public string System { get; set; }
        public string SegmentCode { get; set; }
        public string CUST_ID { get; set; }
        public Guid CaseId { get; set; } //adam 20160427
        public string DocNo { get; set; } //adam 20220505

        public string AssetBranch { get; set; } //adam 20220512
        public string Flag_909113 { get; set; } //adam 20220512
        public string EtabsDatetime { get; set; } //adam 20220512
        public string Set { get; set; } //adam 20220512
    }
}