using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class GssDoc
    {
        public int id { get; set; }
        public int DocType { get; set; }
        public string BatchNo { get; set; }
        public string CompanyID { get; set; }
        public string Batchmetadata { get; set; }
        public DateTime BatchDate { get; set; }
        public string TransferType { get; set; }
        public DateTime CreatedDate { get; set; }
        public Nullable<int> ParserStatus { get; set; }
        public string ParserMessage { get; set; }
    }
}
