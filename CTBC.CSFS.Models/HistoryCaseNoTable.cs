using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public   class HistoryCaseNoTable:Entity
    {
        public string CaseType{get; set;}

        public string CaseDate {get; set;}

        public int CaseNo { get; set; }

    }
}
