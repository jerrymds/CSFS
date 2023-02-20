using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseCustOutputF3
    {
        public int Id { get; set; }
        public string DocNo { get; set; }
        public Guid MasterId { get; set; }
        public Guid DetailsId { get; set; }
        public string CUST_ID_NO { get; set; }
        public string RENT_BRANCH { get; set; }
        public string RENT_KIND { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string NIGTEL_NO { get; set; }
        public string MOBIL_NO { get; set; }
        public string COMM_ADDR { get; set; }
        public string CUST_ADD { get; set; }
        public string BOX_NO { get; set; }
        public string MODIFIED_DATE { get; set; }
        public string RENT_START { get; set; }
        public string RENT_END { get; set; }
        public string MEMO { get; set; } 
    }
}
