using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CheckNoSettingViewModel
    {
        // 介面顯示
        public CheckNoSetting CheckNoSetting { get; set; }

        // 清單顯示
        public IList<CheckNoSetting> CheckNoSettingList { get; set; }
    }
}
