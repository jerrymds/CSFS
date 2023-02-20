using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.DeathQuerySetting
{
    public class CaseDeadDetailDTO
    {
        public Guid NewID { get; set; }
        public string HeirID { get; set; }
        public string HeirName { get; set; }
        public string HeirBirthday { get; set; }
        public string ACMark { get; set; }

        public string TX67050_STATUS { get; set; }
        public string TX67050_Message { get; set; }
        public string TX60628_STATUS { get; set; }
        public string TX60628_Message { get; set; }
        public string TX60490_STATUS { get; set; }
        public string TX60490_Message { get; set; }
        public string TX9091_STATUS { get; set; }
        public string TX9091_Message { get; set; }
    }

    
}
