using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HostMsgDetl : Entity
    {

        public string detl_id { get; set; }
        public string trans_id { get; set; }
        public string edata { get; set; }
        public string cdata { get; set; }
        public string dataorder { get; set; }
        public string datatype { get; set; }
        public string src_field { get; set; }
        public string dest_table { get; set; }
        public string dest_column { get; set; }
        public string datalength { get; set; }
        public string cCretMebrNo { get; set; }
        public string cCretDT { get; set; }
        public string cMantMebrNo { get; set; }
        public string cMantDT { get; set; }
    }
}
