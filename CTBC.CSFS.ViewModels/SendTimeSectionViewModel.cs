using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class SendTimeSectionViewModel
    {
        public SendTimeSection SendTimeSection { get; set; }
        public IList<SendTimeSection> SendTimeSectionList { get; set; }
    }
}
