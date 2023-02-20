using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class MeetingResultDetail : Entity
    {
        public int AttatchDetailId { get; set; }
        public int ResultId { get; set; }
        public string AttatchDetailName { get; set; }
        public string AttatchDetailServerPath { get; set; }
        public string AttatchDetailServerName { get; set; }
        public int isDelete { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }
    }
}
