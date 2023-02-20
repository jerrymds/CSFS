using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class TransactionRecordsViewModel
    {
        // 介面顯示
        public CaseDataLog TransRecords { get; set; }

        // 清單顯示
        public IList<CaseDataLog> TransRecordsMaster { get; set; }

        public IList<CaseDataLog> TransRecordsDetail { get; set; }

        public string CaseId { get; set; }
    }
}
