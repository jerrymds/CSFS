using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class Email_NoticeViewModel
    {
        public Email_Notice Email_Notice { get; set; }

        // 清單顯示
        public IList<Email_Notice> Email_NoticeList { get; set; }
    }
}
