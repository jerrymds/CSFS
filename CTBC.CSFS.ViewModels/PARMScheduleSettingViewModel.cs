using System;
using System.Collections.Generic;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class PARMScheduleSettingViewModel
    {
        public IList<PARMScheduleSettingVO> PARMScheduleSettingList { get; set; }

        public PARMScheduleSettingVO PARMScheduleSettingVO { get; set; }
    }
}