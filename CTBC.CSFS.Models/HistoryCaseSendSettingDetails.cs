using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryCaseSendSettingDetails
    {
        public int DetailsId { get; set; }
        public Guid CaseId { get; set; }
        public int SerialID { get; set; }
        public int SendType { get; set; }
        public string GovName { get; set; }
        public string GovAddr { get; set; }
        public string GovCode { get; set; }
    }
}
