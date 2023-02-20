using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class MeetingResult : Entity
    {
        public int ResultId { get; set; }
        public string ResultDate { get; set; }
        public int ResultStatus { get; set; }
        public string ResultCompleteDate { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }
        public string ResultDateShow { get; set; }
    }
}
