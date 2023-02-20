using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryCaseSendSetting
    {
        public int SerialId { get; set; }
        public Guid CaseId { get; set; }
        public string Template { get; set; }
        public string SendWord { get; set; }
        public string SendNo { get; set; }
        public DateTime SendDate { get; set; }
        public string Speed { get; set; }
        public string Security { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Attachment { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string SendNoId { get; set; }
        public string SendNoStart { get; set; }
        public string SendNoYear { get; set; }
        public string GovName { get; set; }
        public string GovAddr { get; set; }
        public string GovNameCc { get; set; }
        public string GovAddrCc { get; set; }
        public string OverDueMemo { get; set; }
        public string SendKind { get; set; }
    }
}
