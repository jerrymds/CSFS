using System;

namespace CTCB.APLog
{
    public class APLogVO
    {
        public DateTime DataTimestamp { get; set; }

        public string TxnCode { get; set; }

        public string IP { get; set; }

        public string Parameters { get; set; }

        public string CusIDs { get; set; }

        public string LogonUser { get; set; }

    }
}