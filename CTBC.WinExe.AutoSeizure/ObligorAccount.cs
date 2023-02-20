using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.AutoSeizure
{
    public class ObligorAccount
    {
        
        public string Account { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string CaseId { get; set; }
        public bool isHuman { get; set; }
        public bool isDifficultWord { get; set; }
        public bool is450OK { get; set; }
        public int SeizureStatus { get; set; }
        public string AccountType { get; set; }        
        public string LinkAccount { get; set; }
        public string ProdCode { get; set; }
        public string Ccy { get; set; }
        public decimal Rate { get; set; }
        public decimal Twd { get; set; }
        public decimal Bal { get; set; }
        
    }
}
