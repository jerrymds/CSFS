using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class MeetingResultViewModel
    {
        public MeetingResult MeetingResult { get; set; }

        public List<MeetingResultDetail> MeetingResultDetailList { get; set; }
    }
}
