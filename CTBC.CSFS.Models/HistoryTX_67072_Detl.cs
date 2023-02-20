using System;
namespace CTBC.CSFS.Models
{
    public class HistoryTX_67072_Detl
    {
        public int SNO { get; set; }
        public int FKSNO { get; set; }
        public string Branc { get; set; }
        public string LimNo { get; set; }
        public string Produ { get; set; }
        public string AppDat { get; set; }
        public string ExpDat { get; set; }
        public string Amt { get; set; }
        public string Curr { get; set; }
        public string BalAmt { get; set; }
        public string Sign { get; set; }
        public string DbbTyp { get; set; }
        public string Status { get; set; }
        public string StopCd { get; set; }
        public string HoldFlag { get; set; }
        public string CUST_ID { get; set; }
        public Guid CaseId { get; set; } //adam 20160427

    }
}